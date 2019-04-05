using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using txt2obj.test.TestClasses;
using Xunit;

namespace txt2obj.test
{
    public class BasicComplexObjects
    {
        [Fact]
        public void ComplexObjectProperty()
        {
            var str = "123456"; 
            var node = new Node.Node
            {
                Target = "Complex1",
                ChildNodes = new List<Node.INode>
                {
                    new Node.Node
                    {
                        Pattern = "(?<v1>.*)",
                        Target = "StringProperty",
                        FromVariable = "v1"
                    }
                }
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.Complex1.ShouldNotBeNull();
            obj.Result.Complex1.StringProperty.ShouldBe("123456");
        }
    }
}
