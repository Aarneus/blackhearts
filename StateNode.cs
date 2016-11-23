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
		private StateNode parent;
		private int parentName;
		
	    public StateNode(dynamic stateValue, StateNode parent=null, int parentName=0) {
			this.children = new Dictionary<int, StateNode>();
			this.stateValue = stateValue;
			this.parent = parent;
			this.parentName = parentName;
        }
		
		public dynamic getValue() {
		    return this.stateValue;
		}
		
		public StateNode setValue(dynamic setValue) {
		    this.stateValue = setValue;
		    return this;
		}
		
		// Returns the child variable according to the given name
		public StateNode getSubvariable(int name, bool createIfNull=false) {
		    if (this.children.ContainsKey(name)) {
		        return this.children[name];
		    }
		    else if (createIfNull) {
		        this.children[name] = new StateNode(0, this, name);
		        return this.children[name];
		    }
		    return null;
		}
		
		// Sets the value of the child variable
		public StateNode setSubvariable(int name, StateNode setValue) {
		    if (!this.children.ContainsKey(name)) {
		            this.children[name] = new StateNode(setValue.stateValue, this, name);
		    }
		    else {
		        this.children[name].setValue(setValue.stateValue);
		    }
		    return this.children[name];
		}
		
		// Replaces the node with the given tree
		public StateNode setSubtree(int name, StateNode tree) {
		    if (tree == null) {
		        this.children.Remove(name);
		    }
		    else {
    		    if (!(tree is StateNode)) {
    		        tree = new StateNode(tree);
    		    }
    		    this.children[name] = tree;
    		    tree.parent = this;
    		    tree.parentName = name;
		    }
	        return tree;
		}
		
		// Destroys the subvariable (and all named with it)
		public StateNode removeSubvariable(int name) {
		    if (this.children.ContainsKey(name)) {
		        StateNode subvar = this.children[name];
		        this.children.Remove(name);
		        return subvar;
		        
		    }
		    throw new ArgumentException("Invalid variable.");
		}
		
		// Removes this node from it's parent
		public StateNode removeFromParent() {
		    if (this.parent != null) {
		        this.parent.removeSubvariable(this.parentName);
		        return this;
		    }
		    throw new ArgumentException("Invalid parent variable.");
		}
		
		// Replaces the node in it's parent's tree
		public StateNode replaceWith(StateNode replacement) {
		    if (this.parent != null) {
		        this.parent.setSubtree(this.parentName, replacement);
		          return this;
		    }
		    throw new ArgumentException("Invalid parent variable");
		}
		
		// Debug print to the console the whole tree
		public void printTree(SymbolManager symbolManager, int name, int level=0) {
		    for (int i = 0; i < level; i++) {
		        System.Console.Write(" ");
		    }
		    System.Console.WriteLine(symbolManager.getString(name) + ": " + this.stateValue);
		    foreach (KeyValuePair<int, StateNode> node in children) {
		        node.Value.printTree(symbolManager, node.Key, level + 1);
		    }
		}
		
		// Compares two instances of StateNode
		public static bool EqualNodes(StateNode a, StateNode b) {
		    if (a == null) {
		        if (b == null) {
		            return true;
		        }
		        return false;
		    }
		    if (b == null) {
		        return false;
		    }
		    return a.Equals(b);
		}
		
		
		// Implicit operators
		public override bool Equals(object obj)
        {
		    StateNode sn = (StateNode)obj;
		    if (this.stateValue is string) {
		        return this.stateValue.Equals(sn);
		    }
		    return this.stateValue == sn;
        }
		
		public static StateNode Add(StateNode a, StateNode b) {
		    return a.stateValue + b.stateValue;
		}
		
		public override int GetHashCode()
        {
            return base.GetHashCode();
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
		    if (var == null) {
		        return "";
		    }
		    
		    if (!(var.stateValue is string)) {
		        return "" + var.stateValue;
		    }
		    return (string)var.stateValue;
		}
	}
}
