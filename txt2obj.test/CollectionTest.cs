using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shouldly;
using Xunit;

namespace txt2obj.test
{
    public class CollectionTest
    {
        [Fact]
        public void CollectionTest01()
        {
            var str = "123456"; 
            var node = new Node.Node
            {
                //Pattern = "he(?<v1>ll)o",
                Target = "ListProperty",
                ChildNodes = new List<Node.INode>
                {
                    new Node.Node
                    {
                        Pattern = ".",
                        Target = "StringProperty",
                        FromVariable = "v1"
                    }
                }
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.ListProperty.Count().ShouldBe(str.Length);
            //obj.Result.Complex1.ShouldNotBeNull();
            //obj.Result.Complex1.StringProperty.ShouldBe("123456");
        }
    }
}
