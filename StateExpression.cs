/*
*/
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Hecate
{
    /// <summary>
    /// Description of StateExpression.
    /// Uses Pratt's Top Down Recursion Parser
    /// </summary>
    public class StateExpression
    {
        
        private Token[] tokens;
        private int currentToken;
        private bool condition;
        private StateNode rootNode;
        private SymbolManager symbolManager;
        private StoryGenerator generator;
        private bool createSubvars = false;
        
        private static StateNode localNode;
        private static List<StateNode> localStack = new List<StateNode>();
        
        private static Regex tokenRegex = new Regex("([0-9]+([.][0-9]+)?|[()]|=>|[+\\-*\\/><!=]=?|\".*\"|[a-zA-Z0-9_]+|[.])");
        private static Regex literalRegex = new Regex("^(\".*\"|[0-9]+([.][0-9]+)?)$");
        
        private static bool debugging = false;
        
        public StateExpression(string text, StoryGenerator generator, SymbolManager symbolManager)
        {
            this.DebugLog(text + "\n");
            this.generator = generator;
            this.symbolManager = symbolManager;
            this.condition = false;
            this.tokens = this.tokenize(text);
            this.currentToken = 0;
        }
        
        // Evaluates the expression
        public StateNode evaluate(StoryGenerator generator, StateNode rootNode) {
            this.rootNode = rootNode;
            this.currentToken = 0;
            StateNode returned = this.expression();
            this.DebugLog("RESULT: " + returned + "\n");
            return returned;
        }
        
        // The expression parser
        private StateNode expression(int rightBindingPower=0) {
            int t = this.currentToken;
            this.currentToken++;
            StateNode left = this.nullDenotation(this.tokens[t]);
            while (rightBindingPower < this.leftBindingPower(this.tokens[this.currentToken])) {
                t = this.currentToken;
                this.currentToken++;
                left = this.leftDenotation(this.tokens[t], left);
            }
            return left;
        }
        
        // Advances the parser only if the current token is of the specified type
        private void advance(int type) {
            if (this.tokens[this.currentToken].type != type) {
                throw new Exception("Syntax error: missing ending parenthesis.");
            }
            this.currentToken++;
        }
        
        // Returns the left binding power of the token
        private int leftBindingPower(Token token) {
            switch (token.type) {
                case SymbolManager.ASSIGN:
                case SymbolManager.ADD_TO:
                case SymbolManager.SUB_TO:
                case SymbolManager.MULTIPLY_TO:
                case SymbolManager.DIVIDE_TO:
                case SymbolManager.LET:
                case SymbolManager.DEL:
                    return 10;
                case SymbolManager.EQUALS:
                case SymbolManager.NOT_EQUALS:
                case SymbolManager.LESS_THAN:
                case SymbolManager.GREATER_THAN:
                case SymbolManager.LESS_OR:
                case SymbolManager.GREATER_OR:
                    return 40;
                case SymbolManager.ADD:
                case SymbolManager.SUB:
                    return 50;
                case SymbolManager.MULTIPLY:
                case SymbolManager.DIVIDE:
                    return 60;
                case SymbolManager.LEFT_PAREN:
                case SymbolManager.DOT:
                case SymbolManager.CALL:
                    return 80;
                case SymbolManager.END_OF_EXPRESSION:
                    return 0;
                default: return 0;
            }
        }
        
        // The value of the literal
        private StateNode nullDenotation(Token token) {
            StateNode expr;
            switch (token.type) {
                case SymbolManager.NULL: return null;
                case SymbolManager.LITERAL: return token.literal;
                case SymbolManager.VARIABLE: return this.rootNode.getSubvariable(token.literal, this.createSubvars);
                case SymbolManager.LOCAL: return StateExpression.localNode.getSubvariable(token.literal, this.createSubvars);
                case SymbolManager.ADD: return this.expression(100);
                case SymbolManager.SUB: return -this.expression(100);
                case SymbolManager.NEGATE: return this.expression(100) > 0 ? 0 : 1;
                case SymbolManager.LEFT_PAREN:
                    bool temp = this.createSubvars;
                    this.createSubvars = false;
                    expr = this.expression();
                    this.createSubvars = temp;
                    this.advance(SymbolManager.RIGHT_PAREN);
                    return expr;
                case SymbolManager.LET:
                    this.createSubvars = true;
                    expr = this.expression(79);
                    this.createSubvars = false;
                    return expr;
                case SymbolManager.DEL:
                    StateNode node = this.expression(79);
                    return node.removeFromParent();
                case SymbolManager.CALL:
                    int rule = this.expression(79);
                    List<StateNode> parameters = new List<StateNode>();
                    while (this.tokens[this.currentToken].type != SymbolManager.END_OF_EXPRESSION) {
                        StateNode param = this.expression(0);
                        parameters.Add(param);
                    }
                    return this.generator.executeRule(rule, parameters.ToArray());
                default: throw new Exception("Syntax error: Invalid value!");
            }
        }
        
        // The left denotation of a token
        private StateNode leftDenotation(Token token, StateNode left) {
            StateNode right = this.expression(this.leftBindingPower(token));
            switch (token.type) {
                case SymbolManager.ADD: return (int)left + (int)right;
                case SymbolManager.SUB: return left - right;
                case SymbolManager.MULTIPLY: return left * right;
                case SymbolManager.DIVIDE: return left / right;
                case SymbolManager.EQUALS: return StateNode.EqualNodes(left, right) ? 1 : 0;
                case SymbolManager.NOT_EQUALS: return StateNode.EqualNodes(left, right) ? 0 : 1;
                case SymbolManager.LESS_THAN: return left < right ? 1 : 0;
                case SymbolManager.GREATER_THAN: return left > right ? 1 : 0;
                case SymbolManager.LESS_OR: return left <= right ? 1 : 0;
                case SymbolManager.GREATER_OR: return left >= right ? 1 : 0;
                case SymbolManager.DOT: 
                    return left == null ? null : left.getSubvariable(right, this.createSubvars);
                case SymbolManager.ASSIGN:
                    return left.setValue(right.getValue());
                case SymbolManager.ADD_TO:
                    return left.setValue((int)left + (int)right);
                case SymbolManager.SUB_TO: 
                    return left.setValue(left - right);
                case SymbolManager.MULTIPLY_TO: 
                    return left.setValue(left * right);
                case SymbolManager.DIVIDE_TO: 
                    return left.setValue(left / right);
                default: throw new Exception("Syntax error: Invalid operator!");
            }
        }
        
        // Splits the string into tokens
        private Token[] tokenize(string text) {
            
            // Split the string into string tokens
            Match match = StateExpression.tokenRegex.Match(text);
            List<string> strings = new List<string>();
            while (match.Success) {
                this.DebugLog("[" + match.Groups[0] + "]");
                
                strings.Add(match.Groups[0].Value);
                match = match.NextMatch();
            }
            this.DebugLog("\n");
            
            // Create the actual token array from the string tokens
            this.tokens = new Token[strings.Count + 1];
            int i = 0;
            int prevToken = 0;
            foreach (string s in strings) {
                tokens[i] = Token.getToken(s, prevToken, this.symbolManager);
                prevToken = tokens[i].type;
                
                // Even one conditional token marks this expression as a condition
                if (SymbolManager.isConditionalOperator(tokens[i].type)) {
                    this.condition = true;
                }
                
                this.DebugLog("[" + tokens[i].type + "]");
                i++;
            }
            this.DebugLog("\n");
            tokens[tokens.Length - 1] = new Token(SymbolManager.END_OF_EXPRESSION, null);
            return tokens;
        }
        
        public bool isCondition() {
            return this.condition;
        }
        
        
        public static StateExpression[] StringArrayToExpressionArray(string[] strings, StoryGenerator generator, SymbolManager symbolManager) {
            StateExpression[] exprs = new StateExpression[strings.Length];
            for (int i = 0; i < strings.Length; i++) {
                exprs[i] = new StateExpression(strings[i], generator, symbolManager);
            }
            return exprs;
        }
        
        // Add a layer of scope insulation for a rule
        public static void PushLocalStack() {
            StateExpression.localStack.Add(StateExpression.localNode);
            StateExpression.localNode = new StateNode(0, null, 0);
        }
        
        // Remove a layer of scope insulation when a rule ends
        public static void PopLocalStack() {
            int last_index = StateExpression.localStack.Count - 1;
            StateExpression.localNode = StateExpression.localStack[last_index];
            StateExpression.localStack.RemoveAt(last_index);
        }
        
        private void DebugLog(string text) {
            if (StateExpression.debugging) {
                System.Console.Write(text);
            }
        }
    }
}
