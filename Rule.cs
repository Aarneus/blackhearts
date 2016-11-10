/*
*/
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Hecate
{
    /// <summary>
    /// Description of Rule.
    /// </summary>
    public class Rule
    {
        private int name;
        private string text;
        private StateExpression[] effects;
        private StateExpression[] conditions;
        private StateExpression[] parameters;
        private StateExpression[] insets;
        
        private StoryGenerator generator;
        private SymbolManager symbolManager;
        
        private static Regex ruleRegex = new Regex("(.*) => (\"\"|\"(\\\\\"|[^\"])*(?<!\\\\)\")(,)?(.*)");
        private static Regex insetRegex = new Regex("\\[[^\\]]*\\]");
        
        public Rule(int name, string text, StoryGenerator generator, SymbolManager symbolManager) : this(name, text, generator, symbolManager, new string[0], new string[0]) {}
        public Rule(int name, string text, StoryGenerator generator, SymbolManager symbolManager, string[] exprs) : this(name, text, generator, symbolManager, exprs, new string[0]) {}
        public Rule(int name, string text, StoryGenerator generator, SymbolManager symbolManager, string[] exprs, string[] parameters)
        {
            this.name = name;
            this.generator = generator;
            this.symbolManager = symbolManager;
            this.setText(text, symbolManager);
            this.setExpressions(StateExpression.StringArrayToExpressionArray(exprs, generator, symbolManager));
            this.parameters = StateExpression.StringArrayToExpressionArray(parameters, generator, symbolManager);
        }
        
        public Rule(string line, StoryGenerator generator, SymbolManager symbolManager) 
        {
            Match ruleMatch = Rule.ruleRegex.Match(line);
            if (ruleMatch.Success) {
                
                // Get the main parts of the rule
                string leftside = ruleMatch.Groups[1].Value.Trim();
                string text = ruleMatch.Groups[2].Value.Trim();
                string evalstring = ruleMatch.Groups[5].Value.Trim();
                
                
                //System.Console.WriteLine(": " + leftside + "; " + text + "; " + evalstring);
                
                // Separate the name from the parameters
                string[] nameAndParameters = Rule.splitHelper(leftside, ' ');
                int name = symbolManager.getInt(nameAndParameters[0]);
                int numberOfParameters = nameAndParameters.Length - 1;
                string[] parameters = new String[numberOfParameters];
                Array.Copy(nameAndParameters, 1, parameters, 0, numberOfParameters);
                
                // Separate the evaluations
                string[] evals = Rule.splitHelper(evalstring, ',');
                
                // Massage the parameter strings from "X" to "let X = 0"
                for (int i = 0; i < parameters.Length; i++) {
                    parameters[i] = "let " + parameters[i] + " = 0";
                }
                
                // Finalize
                this.name = name;
                this.generator = generator;
                this.symbolManager = symbolManager;
                this.setText(text, symbolManager);
                this.setExpressions(StateExpression.StringArrayToExpressionArray(evals, generator, symbolManager));
                this.parameters = StateExpression.StringArrayToExpressionArray(parameters, generator, symbolManager);
            }
        }
        
        
        // Executes the rule
        public string execute(StateNode[] parameters, StoryGenerator generator, StateNode rootNode) {
            string result = this.text;
            // Push local stack before evaluations
            StateExpression.PushLocalStack();
            
            // Bind parameters
            for (int i = 0; i < this.parameters.Length; i++) {
                StateNode parameter = this.parameters[i].evaluate(generator, rootNode);
                parameter.replaceWith(parameters[i]);
            }
            
            // Evaluate effects before insets to avoid rabbithole
            foreach (StateExpression e in this.effects) {
                e.evaluate(generator, rootNode);
            }
            
            // Replace all inset expressions with their evals
            for (int i = 0; i < this.insets.Length; i++) {
                StateNode eval = this.insets[i].evaluate(generator, rootNode);
                result = result.Replace("[" + i + "]", eval);
            }
            
            // Pop local stack after evaluations
            StateExpression.PopLocalStack();
            
            // Add special characters
            result = result.Replace("\\n", "\n");
            result = result.Replace("\\\"", "\"");
                
            return result;
        }
        
        // Returns true when the rule can be executed with these parameters
        public bool check(StateNode[] parameters, StateNode rootNode) {
            // Must be given at least as much parameters as required
            if (parameters.Length < this.parameters.Length) {
                return false;
            }
            
             // Push local stack before evaluations
            StateExpression.PushLocalStack();
            
            // Bind parameters
            for (int i = 0; i < this.parameters.Length; i++) {
                StateNode parameter = this.parameters[i].evaluate(generator, rootNode);
                parameter.replaceWith(parameters[i]);
            }
            
            // If all conditions return true
            foreach (StateExpression c in this.conditions) {
                int result = c.evaluate(this.generator, rootNode);
                if (result == 0) {
                    StateExpression.PopLocalStack();
                    return false;
                }
            }
            
            // Pop local stack after evaluations
            StateExpression.PopLocalStack();
            
            
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
                    expr = expr.Replace("\\\"", "\"");
                    
                    text = text.Remove(capture.Index + offset, capture.Length);
                    text = text.Insert(capture.Index + offset, replacer);
                    offset -= capture.Length - replacer.Length;
                    
                    this.insets[index] = new StateExpression(expr, this.generator, symbolManager);
                    index++;
        	    }
        	}
            
            //System.Console.Write("RULE: " + symbolManager.getString(this.name) + ": ");
            //System.Console.WriteLine(text);
            this.text = text.Substring(1, text.Length - 2);
        }
        
        // Save the expressions in two groups; conditions and effects
        private void setExpressions(StateExpression[] exprs) {
            List<StateExpression> conditions = new List<StateExpression>();
            List<StateExpression> effects = new List<StateExpression>();
            
            foreach (StateExpression ex in exprs) {
                if (ex.isCondition()) {
                    conditions.Add(ex);
                }
                else {
                    effects.Add(ex);
                }
            }
            this.conditions = conditions.ToArray();
            this.effects = effects.ToArray();
        }
        
        public static string[] splitHelper(string text, char separator) {
            return text.Split(new char[] {separator}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
