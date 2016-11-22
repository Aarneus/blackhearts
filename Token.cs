/*
*/
using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecate
{
    /// <summary>
    /// Description of Token.
    /// </summary>
    public class Token
    {
        public int type;
        public StateNode literal;
        
        
        private static Regex tokenRegex = new Regex("([0-9]+([.][0-9]+)?|[()]|\\^|=>|<-|[+\\-*\\/><!=]=?|\"\"|\"(\\\\\"|[^\"])*(?<!\\\\)\"|[a-zA-Z0-9_]+|[.])");
        
        private static Regex stringRegex = new Regex("^(\"\"|\"(\\\\\"|[^\"])*(?<!\\\\)\")$");
        private static Regex numberRegex = new Regex("^-?[0-9]+([.][0-9]+)?$");
        private static Regex variableRegex = new Regex("^[a-zA-Z0-9_]+$");
        
        public Token(int type, StateNode literal = null)
        {
            this.type = type;
            this.literal = literal;
        }
        
        public static Token getToken(string s, int prevToken, SymbolManager symbolManager) {
            Token token = null;
            
            // Commands or null
            if (s.Equals("let") || s.Equals("del") || s.Equals("and") || s.Equals("or") || s.Equals("null") || s.Equals("this")) {
                token = new Token(symbolManager.getInt(s), null);
            }
            // Literal string
            else if (Token.stringRegex.IsMatch(s)) {
                token = new Token(SymbolManager.LITERAL, s.Substring(1, s.Length - 2));
            }
            // Literal number
            else if (Token.numberRegex.IsMatch(s)) {
                token = new Token(SymbolManager.LITERAL, Convert.ToInt32(s));
            }
            // Variable path - first one is a variable, the others are literals
            else if (Token.variableRegex.IsMatch(s)) {
                // Path literal or rule name
                if (prevToken == SymbolManager.DOT || prevToken == SymbolManager.CALL) {
                    token = new Token(SymbolManager.LITERAL, symbolManager.getInt(s));
                }
                // Global variable name
                else if (!char.IsUpper(s[0])) {
                    token = new Token(SymbolManager.VARIABLE, symbolManager.getInt(s));
                }
                // Local variable name
                else {
                    token = new Token(SymbolManager.LOCAL, symbolManager.getInt(s));
                }
            }
            // Not a literal
            else {
                int op = symbolManager.getInt(s);
                if (op < SymbolManager.FIRST_SYMBOL) {
                    token = new Token(op, null);
                }
            }
            
            Debug.Assert(token != null);
            return token;
        }
        
         // Splits the string into tokens
        public static Token[] tokenize(string text, SymbolManager symbolManager) {
            
            // Split the string into string tokens
            Match match = Token.tokenRegex.Match(text);
            List<string> strings = new List<string>();
            while (match.Success) {
                strings.Add(match.Groups[0].Value);
                match = match.NextMatch();
            }
            
            // Create the actual token array from the string tokens
            Token[] tokens = new Token[strings.Count + 1];
            int i = 0;
            int prevToken = 0;
            foreach (string s in strings) {
                tokens[i] = Token.getToken(s, prevToken, symbolManager);
                prevToken = tokens[i].type;
                i++;
            }
            tokens[tokens.Length - 1] = new Token(SymbolManager.END_OF_EXPRESSION, null);
            return tokens;
        }
    }
}
