/*
*/
using System;
using System.Collections.Generic;

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
        
        public SymbolManager()
        {
            this.integers = new Dictionary<string, int>();
            this.strings = new Dictionary<int, string>();
            this.currentSymbolIndex = 0;
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
