/*
*/
using System;
using System.Text.RegularExpressions;

namespace Hecate
{
    /// <summary>
    /// Description of RuleEval.
    /// </summary>
    public class RuleEval
    {
        private enum EvalType {Number, String, Operator, Path, Fixed, Variable, SubEval}
        
        private RuleEval[] subEvals;
        private EvalType type;
        private int valueInt;
        private string valueString;
        
        private static Regex stringRegex = new Regex("^\"[^\"]*\"$");
        private static Regex numberRegex = new Regex("^-?[0-9]+([.][0-9]+)?$");
        private static Regex evalRegex = new Regex("^[^()]+$");
        private static Regex variableRegex = new Regex("^[^. ]+$");        
        private static Regex fixedRegex = new Regex("^[a-z]");
        private static Regex operatorRegex = new Regex("^[!-+*/%<>=][=]?$");
        private static Regex pathRegex = new Regex("^[().a-zA-Z0-9_]+$");
        private static Regex simpleRegex = new Regex("^((\"[^\"]*\")|([^()]+)|([^ \"]+))$");

        private static bool debugging = false;
        
        public RuleEval(string text, SymbolManager symbolManager)
        {
            // Parse the string into the evaluation
            text = text.Trim();
            if (!text.Equals("")) {
                
                // Simple cases
                if (RuleEval.simpleRegex.IsMatch(text)) {
                        
                    // String
                    if (RuleEval.stringRegex.IsMatch(text)) {
                        this.type = EvalType.String;
                        this.valueString = text.Substring(1, text.Length - 2);
                        this.DebugLog("STR: " + this.valueString);
                    }
                    
                    // Number
                    else if (RuleEval.numberRegex.IsMatch(text)) {
                        this.type = EvalType.Number;
                        this.valueInt = Convert.ToInt32(text);
                        this.DebugLog("INT: " + this.valueInt);
                    }
                    
                    // Operator
                    else if (RuleEval.operatorRegex.IsMatch(text)) {
                        this.type = EvalType.Operator;
                        this.valueInt = symbolManager.getInt(text);
                        this.DebugLog("OPR: " + text);
                    }
                    
                    // Variable
                    else if (RuleEval.variableRegex.IsMatch(text)) {
                        bool fixedVariable = RuleEval.fixedRegex.IsMatch(text);
                        this.type = fixedVariable ? EvalType.Fixed : EvalType.Variable;
                        this.valueInt = symbolManager.getInt(text.ToLower());
                        
                        this.DebugLog((fixedVariable ? "FIX: " : "VAR: ") + text);
                    }
                    
                    // Path
                    else if (RuleEval.pathRegex.IsMatch(text)) {
                        this.type = EvalType.Path;
                        string[] variables = Rule.SplitHelper(text, '.');
                        this.DebugLog("PTH: " + text);
                        this.subEvals = RuleEval.StringArrayToEvalArray(variables, symbolManager);
                    }
                    
                    // Simple subeval
                    else if (RuleEval.evalRegex.IsMatch(text)) {
                        string[] subevals = Rule.SplitHelper(text, ' ');
                        this.type = EvalType.SubEval;
                        this.DebugLog("EVA: " + text);
                        this.subEvals = RuleEval.StringArrayToEvalArray(subevals, symbolManager);
                    }
                }
                
                // Complex eval
                else {
                    this.DebugLog("CMX: " + text);
                }
            }
            
            else {
                this.DebugLog("AAA: " + text);
            }
        
        }
        
        private void DebugLog(string text) {
            if (RuleEval.debugging) {
                System.Console.WriteLine(text);
            }
        }
        
        public static RuleEval[] StringArrayToEvalArray(string[] strings, SymbolManager symbolManager) {
            RuleEval[] evals = new RuleEval[strings.Length];
            for (int i = 0; i < strings.Length; i++) {
                evals[i] = new RuleEval(strings[i], symbolManager);
            }
            return evals;
        }
    }
}
