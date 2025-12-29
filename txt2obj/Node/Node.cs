using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using txt2obj.Helpers;
using txt2obj.Variables;

namespace txt2obj.Node
{
    public class Node
    {
        public Node()
        {
            this.ChildNodes = new List<Node>();
        }
        private object _lockObj = new object();
        public List<Node> ChildNodes { get; set; }
    
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
                childNode.Prepare(this);
            }
        }
        public void Prepare()
        {
            foreach (var childNode in this.ChildNodes)
            {
                childNode.Prepare(this);
            }
        }

        public void Validate(Type rootType)
        {
            if (rootType == null)
            {
                throw new ArgumentNullException(nameof(rootType));
            }

            ValidateInternal(this, rootType);
        }

        private static void ValidateInternal(Node node, Type currentType)
        {
            if (node == null)
            {
                return;
            }

            var nextType = currentType;

            if (!string.IsNullOrEmpty(node.Target))
            {
                var propertyType = HelperMethods.GetTypePropertyOrFieldType(currentType, node.Target);
                if (propertyType == null)
                {
                    throw new InvalidOperationException($"Target '{node.Target}' was not found on type '{currentType.FullName}'.");
                }

                if (HelperMethods.IsCollection(propertyType))
                {
                    if (node.ChildNodes != null)
                    {
                        foreach (var childNode in node.ChildNodes)
                        {
                            if (string.IsNullOrEmpty(childNode.Pattern))
                            {
                                throw new InvalidOperationException($"Collection child node for target '{node.Target}' requires a non-empty Pattern.");
                            }
                        }
                    }

                    nextType = HelperMethods.GetCollectionType(propertyType);
                    if (nextType == null)
                    {
                        throw new InvalidOperationException($"Collection target '{node.Target}' does not define an element type.");
                    }
                }
                else if (!HelperMethods.IsSimple(propertyType))
                {
                    nextType = propertyType;
                }
                else
                {
                    return;
                }
            }

            if (node.ChildNodes == null || node.ChildNodes.Count == 0)
            {
                return;
            }

            foreach (var childNode in node.ChildNodes)
            {
                ValidateInternal(childNode, nextType);
            }
        }
    }
}
