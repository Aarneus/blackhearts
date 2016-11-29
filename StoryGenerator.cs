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
        
        
        public string Generate(string symbol) {
            
            Rule tempRule = new Rule(-3, "\"" + symbol + "\"", this, this.symbolManager);
            string text = tempRule.Execute(new StateNode[0], this, this.rootNode);
            return text;
        }
        
        // Chooses an applicable rule and executes it
        public string ExecuteRule(int rule, StateNode[] parameters) {
            // Choose a rule
            if (this.rules.ContainsKey(rule)) {
                Rule chosen = null;
                int count = 1;
                
                foreach (Rule r in this.rules[rule]) {
                    if (r.Check(parameters, this.rootNode)) {
                        if (this.random.Next(count) == 0) {
                            chosen = r;
                            count += 1;
                        }
                    }
                }
                
                if (chosen != null) {
                    return chosen.Execute(parameters, this, this.rootNode);
                }
            }
            
            return "";
        }
        
        
        public void ParseRuleDirectory(string filepath) {
            string[] files = System.IO.Directory.GetFiles(filepath, "*.hec");
            foreach (string file in files) {
                this.ParseRuleFile(file);
            }
        }
        
        public void ParseRuleFile(string filename) {
            string line;
            string rule = "";
            string set_beginning = "";
            string set_end = "";
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            while((line = file.ReadLine()) != null)
            {
                line = line.Trim();
                
                // Collect the rule from multiple lines if the line ends with a comma
                rule += line;
                if (!line.TrimEnd().EndsWith(",")) {
                    
                    // Begins a set of rules
                    if (!rule.Contains("=>")) {
                        if (rule.Contains(":>")) {
                            string[] parts = Rule.SplitHelper(rule, ":>");
                            set_beginning = parts[0];
                            set_end = parts[1];
                       }
                       else {
                            set_beginning = rule;
                            set_end = "";
                       }
                    }
                    // A continued list line
                    else if (rule.StartsWith("=>")) {
                       this.ParseRuleString(set_beginning + " " + rule + ", " + set_end);
                    }
                    // A self-contained rule
                    else {
                       this.ParseRuleString(rule);
                    }
                    rule = "";
                }
            }
            file.Close();
        }
        
        public void ParseRuleString(string line) {
            line = line.Trim();
            if (!line.Equals("") && !line.StartsWith("#")) {
                Rule rule = new Rule(line, this, this.symbolManager);
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
