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
        private RuleEval[] evals;
        private RuleEval[] parameters;
        
        private static Regex ruleRegex = new Regex("(.*) -> (\"[^\"]*\")(, )?(.*)");
        
        public Rule(int name, string text) : this(name, text, null, new string[0], new string[0]) {}
        public Rule(int name, string text, SymbolManager symbolManager, string[] evals) : this(name, text, symbolManager, evals, new string[0]) {}
        public Rule(int name, string text, SymbolManager symbolManager, string[] evals, string[] parameters)
        {
            this.name = name;
            this.text = text;
            
            this.evals = RuleEval.StringArrayToEvalArray(evals, symbolManager);
            this.parameters = RuleEval.StringArrayToEvalArray(parameters, symbolManager);
        }
        
        public Rule(string line, SymbolManager symbolManager) 
        {
            Match ruleMatch = Rule.ruleRegex.Match(line);
            if (ruleMatch.Success) {
                
                // Get the main parts of the rule
                string leftside = ruleMatch.Groups[1].Value.Trim();
                string text = ruleMatch.Groups[2].Value.Trim();
                string evalstring = ruleMatch.Groups[4].Value.Trim();
                
                System.Console.WriteLine(": " + leftside + "; " + text + "; " + evalstring);
                
                // Separate the name from the parameters
                string[] nameAndParameters = Rule.SplitHelper(leftside, ' ');
                int name = symbolManager.getInt(nameAndParameters[0]);
                int numberOfParameters = nameAndParameters.Length - 1;
                string[] parameters = new String[numberOfParameters];
                Array.Copy(nameAndParameters, 1, parameters, 0, numberOfParameters);
                
                // Separate the evaluations
                string[] evals = Rule.SplitHelper(evalstring, ',');
                
                // Finalize
                this.name = name;
                this.text = text;
                this.evals = RuleEval.StringArrayToEvalArray(evals, symbolManager);
                this.parameters = RuleEval.StringArrayToEvalArray(parameters, symbolManager);
            }
        }
        
        public int GetName() {
            return this.name;
        }
        
        public static string[] SplitHelper(string text, char separator) {
            return text.Split(new char[] {separator}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
