using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using txt2obj.Node;
using txt2obj.test.TestClasses;
using txt2obj.Variables;
using Xunit;

namespace txt2obj.test
{
    public class VariableTests
    {
        [Fact]
        public async Task VariableSearch01()
        {
            var childNode = new Node.Node
            {
                Comment = "childnode"
            };
            
            
            var parentNode = new Node.Node
            {   
                Comment ="parentnode",
                ChildNodes = new List<Node.INode>
                {
                    childNode
                }
            };
            parentNode.Prepare();
            childNode.GetVariable("TEST").ShouldBe(null);
            parentNode.SetVariable("TEST","1");
            childNode.GetVariable("TEST").Value.ShouldBe("1");
        }

        [Fact]
        public async Task TargetVariable01()
        {
            INode node = new Node.Node
            {
                Constant = "C1",
                TargetVariable = "Var1",
                ChildNodes = new List<INode>
                {
                    new Node.Node
                    {
                        FromVariable = "Var1",
                        Target = "StringProperty"
                    }
                }
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, "");
            obj.Result.StringProperty.ShouldBe(node.Constant);
        }
    }
}
