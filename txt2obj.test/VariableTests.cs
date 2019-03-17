using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
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
    }
}
