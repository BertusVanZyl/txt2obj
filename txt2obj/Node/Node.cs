using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using txt2obj.Variables;

namespace txt2obj.Node
{
    public class Node : INode
    {
        public Node()
        {
            this.ChildNodes = new List<INode>();
        }
        private object _lockObj = new object();
        public List<INode> ChildNodes { get; set; }
    
        public Node ParentNode { get; set; }
        public string Comment { get; set; }
        public string Target { get; set; }
        public string Pattern { get; set; }
        public string FromVariable { get; set; }

        public VariableHolder Variables = new VariableHolder();

        public Variable GetVariable(string name)
        {
            var localVar = Variables[name];
            return localVar != null ? localVar : ParentNode?.GetVariable(name);
        }

        public void SetVariable(string name, string value)
        {
            this.Variables[name] = new Variable(name,value);
        }

        private void Prepare(Node parentNode)
        {
            this.ParentNode = parentNode;
            foreach (var childNode in this.ChildNodes)
            {
                ((Node)childNode).Prepare(this);
            }
        }
        public void Prepare()
        {
            foreach (var childNode in this.ChildNodes)
            {
                ((Node)childNode).Prepare(this);
            }
        }
    }
}
