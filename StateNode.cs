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
		private dynamic stateValue;
		
	    public StateNode(dynamic stateValue) {
			this.children = new Dictionary<int, StateNode>();
			this.stateValue = stateValue;
        }
		
		// Returns the child variable according to the given name
		public StateNode getSubvariable(int name) {
		    if (this.children.ContainsKey(name)) {
		        return this.children[name];
		    }
		    throw new ArgumentException();
		}
		
		
		// Implicit operators
		public override bool Equals(object obj)
        {
		    StateNode sn = (StateNode)obj;
		    if (this.stateValue is string) {
		        return this.stateValue.Equals(sn);
		    }
		    return this.stateValue == (double)sn;
        }

		
		// Initialize as int
		public static implicit operator StateNode(int stateValue) {
	            return new StateNode(stateValue); 
        }
		
		// Initialize as double
		public static implicit operator StateNode(double stateValue) {
	            return new StateNode(stateValue); 
        }
		
		// Initialize as string
		public static implicit operator StateNode(string stateValue) {
	            return new StateNode(stateValue); 
	    }
		
		// Treated as int
		public static implicit operator int(StateNode var) {
		    return (int)var.stateValue;
		}
		
		// Treated as double
		public static implicit operator double(StateNode var) {
		    return (double)var.stateValue;
		}
		
		// Treated as string
		public static implicit operator string(StateNode var) {
		    if (!(var.stateValue is string)) {
		        return "" + var.stateValue;
		    }
		    return (string)var.stateValue;
		}
	}
}
