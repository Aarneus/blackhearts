/*
*/
using System;
using System.Collections.Generic;

namespace Hecate
{
    /// <summary>
    /// Description of StoryGenerator.
    /// </summary>
    public class StoryGenerator
    {
        private StateNode rootVariable;
        private Dictionary<int, List<Rule>> rules;
        private SymbolManager symbolManager;
        
        
        
        public StoryGenerator()
        {
            this.rootVariable = new StateNode(0);
            this.rules = new Dictionary<int, List<Rule>>();
            this.symbolManager = new SymbolManager();
        }
        
        
        public string GenerateStory(string startingSymbol) {
            return "WIP";
        }
        
        
        
        public void ParseRuleDirectory(string filepath) {
            //TODO
        }
        
        public void ParseRuleFile(string filename) {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            while((line = file.ReadLine()) != null)
            {
               this.ParseRuleString(line);
            }
            file.Close();
        }
        
        public void ParseRuleString(string line) {
            line = line.Trim();
            if (!line.Equals("") && !line.StartsWith("#")) {
                Rule rule = new Rule(line, this.symbolManager);
                int name = rule.GetName();
                this.AddRule(name, rule);
            }
        }
        
        private void AddRule(int name, Rule rule) {
            if (!this.rules.ContainsKey(name)) {
                this.rules[name] = new List<Rule>();
            }
            this.rules[name].Add(rule);
        }
        
        
    }
}
