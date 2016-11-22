﻿/*
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
        private StateNode ruleNode;
        private SymbolManager symbolManager;
        private StoryGenerator generator;
        private bool createSubvars = false;
        private string original_text = "";
        
        private static StateNode localNode;
        private static List<StateNode> localStack = new List<StateNode>();
        
        private static Regex literalRegex = new Regex("^(\".*\"|[0-9]+([.][0-9]+)?)$");
        
        private static bool debugging = false;
        
        public StateExpression(string text, StoryGenerator generator, SymbolManager symbolManager, Rule rule)
        {
            this.DebugLog(text + "\n");
            this.generator = generator;
            this.symbolManager = symbolManager;
            
            // Even one conditional token marks this expression as a condition
            this.tokens = Token.tokenize(text, symbolManager);
            this.condition = false;
            foreach (Token t in this.tokens) {
                if (SymbolManager.isConditionalOperator(t.type)) {
                    this.condition = true;
                    break;
                }
            }
            this.currentToken = 0;
            this.original_text = text;
            if (rule != null) {
                this.ruleNode = rule.getNode();
            }
        }
        
        // Evaluates the expression
        public StateNode evaluate(StoryGenerator generator, StateNode rootNode) {
            this.rootNode = rootNode;
            this.currentToken = 0;
            //try {
            StateNode returned = this.expression();
            return returned;
            //} catch (Exception ex) {
            //    throw new Exception("Error in expression: " + this.original_text + "\n" + ex.Message);
            //}
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
                throw new Exception("Syntax error: missing token");
            }
            this.currentToken++;
        }
        
        // Returns the left binding power of the token
        private int leftBindingPower(Token token) {
            switch (token.type) {
                case SymbolManager.ASSIGN:
                case SymbolManager.REPLACE:
                case SymbolManager.ADD_TO:
                case SymbolManager.SUB_TO:
                case SymbolManager.MULTIPLY_TO:
                case SymbolManager.DIVIDE_TO:
                case SymbolManager.LET:
                case SymbolManager.DEL:
                case SymbolManager.AND:
                case SymbolManager.OR:
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
                case SymbolManager.CAPITALIZE:
                    return 50;
                case SymbolManager.MULTIPLY:
                case SymbolManager.DIVIDE:
                    return 60;
                case SymbolManager.LEFT_PAREN:
                    return 70;
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
                case SymbolManager.THIS: return this.ruleNode;
                case SymbolManager.LOCAL: return StateExpression.localNode.getSubvariable(token.literal, this.createSubvars);
                case SymbolManager.ADD: return this.expression(100);
                case SymbolManager.SUB: return -this.expression(100);
                case SymbolManager.CAPITALIZE: 
                    string val = this.expression(10);
                    if (val.Length > 0) { return val.Substring(0, 1).ToUpper() + val.Substring(1); }
                    return val;
                case SymbolManager.NEGATE: 
                    StateNode result = this.expression(100);
                    if (result == null) { return 1; }
                    return result > 0 ? 0 : 1;
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
                    int current = this.tokens[this.currentToken].type;
                    if (current == SymbolManager.LEFT_PAREN) {
                        this.currentToken += 1;
                        current = this.tokens[this.currentToken].type;
                        while (current != SymbolManager.END_OF_EXPRESSION && current != SymbolManager.RIGHT_PAREN) {
                            StateNode param = this.expression(0);
                            parameters.Add(param);
                            current = this.tokens[this.currentToken].type;
                        }
                        this.currentToken += 1;
                    }
                    return this.generator.executeRule(rule, parameters.ToArray());
                default: throw new Exception("Syntax error: Invalid value!");
            }
        }
        
        // The left denotation of a token
        private StateNode leftDenotation(Token token, StateNode left) {
            StateNode right = this.expression(this.leftBindingPower(token));
            switch (token.type) {
                case SymbolManager.ADD: return StateNode.Add(left, right);
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
                case SymbolManager.REPLACE:
                    return left.replaceWith(right);
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
                case SymbolManager.AND:
                    return (left > 0) && (right > 0) ? 1 : 0;
                case SymbolManager.OR:
                    return (left > 0) || (right > 0) ? 1 : 0;
                default: throw new Exception("Syntax error: Invalid operator! " + token.type);
            }
        }
        
        public bool isCondition() {
            return this.condition;
        }
        
        public static StateExpression[] StringArrayToExpressionArray(string[] strings, StoryGenerator generator, SymbolManager symbolManager, Rule rule) {
            StateExpression[] exprs = new StateExpression[strings.Length];
            for (int i = 0; i < strings.Length; i++) {
                exprs[i] = new StateExpression(strings[i].Trim(), generator, symbolManager, rule);
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
