/*
*/
using System;
using System.Text.RegularExpressions;

namespace Hecate
{
    /// <summary>
    /// Description of Rule.
    /// </summary>
    public class Rule
    {
        private int name;
        private string text;
        private StateExpression[] exprs;
        private StateExpression[] parameters;
        private StateExpression[] insets;
        
        private StoryGenerator generator;
        
        private static Regex ruleRegex = new Regex("(.*) -> (\"[^\"]*\")(, )?(.*)");
        private static Regex insetRegex = new Regex("\\[[^\\]]*\\]");
        
        public Rule(int name, string text, StoryGenerator generator, SymbolManager symbolManager) : this(name, text, generator, symbolManager, new string[0], new string[0]) {}
        public Rule(int name, string text, StoryGenerator generator, SymbolManager symbolManager, string[] exprs) : this(name, text, generator, symbolManager, exprs, new string[0]) {}
        public Rule(int name, string text, StoryGenerator generator, SymbolManager symbolManager, string[] exprs, string[] parameters)
        {
            this.name = name;
            this.generator = generator;
            this.setText(text, symbolManager);
            this.exprs = StateExpression.StringArrayToExpressionArray(exprs, generator, symbolManager);
            this.parameters = StateExpression.StringArrayToExpressionArray(parameters, generator, symbolManager);
        }
        
        public Rule(string line, StoryGenerator generator, SymbolManager symbolManager) 
        {
            Match ruleMatch = Rule.ruleRegex.Match(line);
            if (ruleMatch.Success) {
                
                // Get the main parts of the rule
                string leftside = ruleMatch.Groups[1].Value.Trim();
                string text = ruleMatch.Groups[2].Value.Trim();
                string evalstring = ruleMatch.Groups[4].Value.Trim();
                
                //System.Console.WriteLine(": " + leftside + "; " + text + "; " + evalstring);
                
                // Separate the name from the parameters
                string[] nameAndParameters = Rule.splitHelper(leftside, ' ');
                int name = symbolManager.getInt(nameAndParameters[0]);
                int numberOfParameters = nameAndParameters.Length - 1;
                string[] parameters = new String[numberOfParameters];
                Array.Copy(nameAndParameters, 1, parameters, 0, numberOfParameters);
                
                // Separate the evaluations
                string[] evals = Rule.splitHelper(evalstring, ',');
                
                // Finalize
                this.name = name;
                this.generator = generator;
                this.setText(text, symbolManager);
                this.exprs = StateExpression.StringArrayToExpressionArray(evals, generator, symbolManager);
                this.parameters = StateExpression.StringArrayToExpressionArray(parameters, generator, symbolManager);
            }
        }
        
        
        // Executes the rule
        public string execute(StoryGenerator generator, StateNode rootNode) {
            // Replace all inset expressions with their evals
            string result = this.text;
            for (int i = 0; i < this.insets.Length; i++) {
                StateNode eval = this.insets[i].evaluate(generator, rootNode);
                result = result.Replace("[" + i + "]", eval);
            }
            
            // Evaluate all basic expressions
            foreach (StateExpression e in this.exprs) {
                e.evaluate(generator, rootNode);
            }
            
            // Add special characters
            result = result.Replace("\\n", "\n");
            
            return result;
        }
        
        // TODO
        public bool check(StateNode rootNode) {
            return true;
        }
        
        public int getName() {
            return this.name;
        }
        
        // Save the text in a more efficient form by separating the inset expressions
        private void setText(string text, SymbolManager symbolManager) {
            MatchCollection matches = Rule.insetRegex.Matches(text);
            this.insets = new StateExpression[matches.Count];
            int offset = 0;
            int index = 0;
            
        	foreach (Match match in matches) {
        	    foreach (Capture capture in match.Captures) {
                    string replacer = "[" + index + "]";
                    string expr = capture.Value.Substring(1, capture.Length - 2);
                    
                    text = text.Remove(capture.Index + offset, capture.Length);
                    text = text.Insert(capture.Index + offset, replacer);
                    offset -= capture.Length - replacer.Length;
                    
                    this.insets[index] = new StateExpression(expr, this.generator, symbolManager);
                    index++;
        	    }
        	}
            this.text = text.Substring(1, text.Length - 2);
        }
        
        public static string[] splitHelper(string text, char separator) {
            return text.Split(new char[] {separator}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
