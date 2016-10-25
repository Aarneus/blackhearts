/*
*/
using System;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Hecate
{
    /// <summary>
    /// Description of Token.
    /// </summary>
    public class Token
    {
        public int type;
        public StateNode literal;
        
        private static Regex stringRegex = new Regex("^\"[^\"]*\"$");
        private static Regex numberRegex = new Regex("^-?[0-9]+([.][0-9]+)?$");
        private static Regex variableRegex = new Regex("^[a-zA-Z0-9_]+$");
        
        public Token(int type, StateNode literal = null)
        {
            this.type = type;
            this.literal = literal;
        }
        
        public static Token getToken(string s, int prevToken, SymbolManager symbolManager) {
            Token token = null;
            
            // Commands
            if (s.Equals("let") || s.Equals("del")) {
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
    }
}
