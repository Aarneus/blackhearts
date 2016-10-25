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
        private StateNode rootNode;
        private Dictionary<int, List<Rule>> rules;
        private SymbolManager symbolManager;
        private Random random;
        
        
        public StoryGenerator()
        {
            this.rootNode = new StateNode(42);
            this.rules = new Dictionary<int, List<Rule>>();
            this.symbolManager = new SymbolManager();
            this.random = new Random();
        }
        
        
        public string generate(string symbol) {
            
            Rule tempRule = new Rule(0, "\"" + symbol + "\"", this, this.symbolManager);
            string text = tempRule.execute(this, this.rootNode);
            return text;
        }
        
        // Chooses an applicable rule and executes it
        public string executeRule(int rule) {
            //TODO: add parameters and conditions
            // Choose a rule
            if (this.rules.ContainsKey(rule)) {
                int r = this.rules[rule].Count;
                r = this.random.Next(r);
                return this.rules[rule][r].execute(this, this.rootNode);
            }
            
            System.Console.WriteLine("NOPE: " + this.symbolManager.getString(rule));
            
            return "";
        }
        
        
        public void parseRuleDirectory(string filepath) {
            string[] files = System.IO.Directory.GetFiles(filepath, "*.hec");
            foreach (string file in files) {
                this.parseRuleFile(file);
            }
        }
        
        public void parseRuleFile(string filename) {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            while((line = file.ReadLine()) != null)
            {
               this.parseRuleString(line);
            }
            file.Close();
        }
        
        public void parseRuleString(string line) {
            line = line.Trim();
            if (!line.Equals("") && !line.StartsWith("#")) {
                Rule rule = new Rule(line, this, this.symbolManager);
                int name = rule.getName();
                this.addRule(name, rule);
            }
        }
        
        private void addRule(int name, Rule rule) {
            if (!this.rules.ContainsKey(name)) {
                this.rules[name] = new List<Rule>();
            }
            this.rules[name].Add(rule);
        }
        
        
    }
}
