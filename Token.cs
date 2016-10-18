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
        
        public static Token GetToken(string s, SymbolManager symbolManager) {
            Token token = null;
            
            // Literals
            if (Token.stringRegex.IsMatch(s)) {
                token = new Token(SymbolManager.LITERAL, s.Substring(1, s.Length - 2));
            }
            else if (Token.numberRegex.IsMatch(s)) {
                token = new Token(SymbolManager.LITERAL, Convert.ToInt32(s));
            }
            else if (Token.variableRegex.IsMatch(s)) {
                token = new Token(SymbolManager.LITERAL, symbolManager.getInt(s));
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
