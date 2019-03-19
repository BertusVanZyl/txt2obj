using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using txt2obj.Node;
using Xunit;

namespace txt2obj.test
{
    public class SetterTests
    {
        [Fact]
        public void BasicSetter()
        {
            var str = "12345";
            
            var node = new Node.Node
            {
                Pattern = "(?<v1>.*)",
                ChildNodes = new List<INode>
                {
                    new Node.Node
                    {
                        FromVariable = "v1",
                        TargetVariable = "v1",
                        Setter = "|OLD| |NEW|"
                    },
                    new Node.Node
                    {
                        FromVariable = "v1",
                        Target = "StringProperty"
                    }
                }

            };

            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.StringProperty.ShouldBe("12345 12345");

            
        }
    }
}
