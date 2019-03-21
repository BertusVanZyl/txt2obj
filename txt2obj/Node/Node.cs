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
        public string TargetVariable { get; set; }
        public string Format { get; set; }
        public string Setter { get; set; }
        public string Constant { get; set; }
        public string Process { get; set; }

        public VariableHolder Variables = new VariableHolder();

        public Variable GetVariable(string name)
        {
            var localVar = Variables[name];
            return localVar != null ? localVar : ParentNode?.GetVariable(name);
        }

        public void SetVariable(string name, string value)
        {
            //find a variable from parents first. If it exists, update that variable rather than creating a new one on this node
            var foundInParentTree = this.GetVariable(name);
            if (foundInParentTree != null)
            {
                foundInParentTree.Value = value;
            }
            else
            {
                this.Variables[name] = new Variable(name, value);
            }
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
