/*
*/
using System;
using System.Collections.Generic;

namespace Hecate
{
	/// <summary>
	/// Description of StateNode.
	/// </summary>
	public class StateNode
	{
		private Dictionary<int, StateNode> children;
		private int valueInt;
		private string valueString;
		
	    public StateNode(int valueInt=0, string valueString="") {
			this.children = new Dictionary<int, StateNode>();
			this.valueInt = valueInt;
			this.valueString = valueString;
        }
		
		// Returns the child variable according to the given name
		public StateNode getSubvariable(int name) {
		    if (this.children.ContainsKey(name)) {
		        return this.children[name];
		    }
		    throw new ArgumentException();
		}
		
		
		// Implicit operators
		// Initialize as int
		public static implicit operator StateNode(int value) {
	            return new StateNode(value); 
        }
		
		// Initialize as string
		public static implicit operator StateNode(string value) {
	            return new StateNode(-1, value); 
	    }
		
		// Treated as int
		public static implicit operator int(StateNode var) {
		    if (var.valueString.Equals("")) {
		        throw new ArithmeticException();
		    }
		    return var.valueInt;
		}
		
		// Treated as string
		public static implicit operator string(StateNode var) {
		    if (var.valueString.Equals("")) {
		        return "" + var.valueInt;
		    }
		    return var.valueString;
		}
	}
}
