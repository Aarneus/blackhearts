/*
*/
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Hecate
{
    /// <summary>
    /// Manages symbols;
    /// each string is given a corresponding int value for optimization
    /// </summary>
    public class SymbolManager
    {
        private Dictionary<string, int> integers;
        private Dictionary<int, string> strings;
        private int currentSymbolIndex;
        
        public const int VARIABLE = -1;
        public const int LITERAL = 0;
        public const int ADD = 1;
        public const int SUB = 2;
        public const int MULTIPLY = 3;
        public const int DIVIDE = 4;
        public const int ASSIGN = 5;
        public const int NEGATE = 6; //TODO
        public const int EQUALS = 7;
        public const int NOT_EQUALS = 8;
        public const int LESS_THAN = 9;
        public const int GREATER_THAN = 10;
        public const int LESS_OR = 11;
        public const int GREATER_OR = 12;
        public const int LEFT_PAREN = 13;
        public const int RIGHT_PAREN = 14;
        public const int DOT = 15;
        public const int ADD_TO = 16;
        public const int SUB_TO = 17;
        public const int MULTIPLY_TO = 18;
        public const int DIVIDE_TO = 19;
        public const int LET = 20;
        public const int DEL = 21;
        public const int END_OF_EXPRESSION = 22;
        
        public const int FIRST_SYMBOL = 23;
        
        public SymbolManager()
        {
            this.integers = new Dictionary<string, int> 
            {
                {"LITERAL", SymbolManager.LITERAL},
                {"+", SymbolManager.ADD},
                {"-", SymbolManager.SUB},
                {"*", SymbolManager.MULTIPLY},
                {"/", SymbolManager.DIVIDE},
                {"=", SymbolManager.ASSIGN},
                {"!", SymbolManager.NEGATE},
                {"==", SymbolManager.EQUALS},
                {"!=", SymbolManager.NOT_EQUALS},
                {"<", SymbolManager.LESS_THAN},
                {">", SymbolManager.GREATER_THAN},
                {"<=", SymbolManager.LESS_OR},
                {">=", SymbolManager.GREATER_OR},
                {"(", SymbolManager.LEFT_PAREN}, 
                {")", SymbolManager.RIGHT_PAREN},
                {".", SymbolManager.DOT},
                {"+=", SymbolManager.ADD_TO},
                {"-=", SymbolManager.SUB_TO},
                {"*=", SymbolManager.MULTIPLY_TO},
                {"/=", SymbolManager.DIVIDE_TO},
                {"let", SymbolManager.LET},
                {"del", SymbolManager.DEL},
                {"$$$", SymbolManager.END_OF_EXPRESSION}
            };
            this.strings = new Dictionary<int, string>();
            foreach (KeyValuePair<string, int> entry in this.integers) {
                this.strings.Add(entry.Value, entry.Key);
            }
            
            this.currentSymbolIndex = SymbolManager.FIRST_SYMBOL;
            
        }
        
        // Returns the symbol for the given string
        public int getInt(string symbol) {
            if (!this.integers.ContainsKey(symbol)) {
                return this.addSymbol(symbol);
            }
            return this.integers[symbol];
        }
        
        // Returns the string for the given symbol
        public string getString(int index) {
            if (!this.strings.ContainsKey(index)) {
                throw new IndexOutOfRangeException();
            }
            return this.strings[index];
        }
        
        // Adds the given string to the symbol database and returns its index number
        private int addSymbol(string symbol) {
            if (!this.integers.ContainsKey(symbol)) {
                this.strings[this.currentSymbolIndex] = symbol;
                this.integers[symbol] = this.currentSymbolIndex;
                this.currentSymbolIndex += 1;
            }
            return this.integers[symbol];
        }
        
        
    }
}
