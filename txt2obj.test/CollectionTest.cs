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
                Target = "ListProperty",
                ChildNodes = new List<Node.INode>
                {
                    new Node.Node
                    {
                        Pattern = "(?<v1>.)",
                        Target = "StringProperty",
                        FromVariable = "v1"
                    }
                }
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.ListProperty.Count().ShouldBe(str.Length);
            obj.Result.ListProperty.Select(x => x.StringProperty).Distinct().Count().ShouldBe(6);
            obj.Result.ListProperty.Select(x => x.StringProperty).Min().ShouldBe("1");
            obj.Result.ListProperty.Select(x => x.StringProperty).Max().ShouldBe("6");
        }

        [Fact]
        public void CollectionTest02()
        {
            var str = "123456"; 
            var node = new Node.Node
            {
                Target = "ListProperty",
                ChildNodes = new List<Node.INode>
                {
                    new Node.Node
                    {
                        Pattern = "(?<v1>.)",
                        Target = "StringProperty"
                    }
                }
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.ListProperty.Count().ShouldBe(str.Length);
            obj.Result.ListProperty.Select(x => x.StringProperty).Distinct().Count().ShouldBe(6);
            obj.Result.ListProperty.Select(x => x.StringProperty).Min().ShouldBe("1");
            obj.Result.ListProperty.Select(x => x.StringProperty).Max().ShouldBe("6");
        }

        [Fact]
        public void CollectionTest03()
        {
            var str = "123456"; 
            var node = new Node.Node
            {
                Target = "ArrayProperty",
                ChildNodes = new List<Node.INode>
                {
                    new Node.Node
                    {
                        Pattern = "(?<v1>.)",
                        Target = "StringProperty"
                    }
                }
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.ArrayProperty.Count().ShouldBe(str.Length);
            obj.Result.ArrayProperty.Select(x => x.StringProperty).Distinct().Count().ShouldBe(6);
            obj.Result.ArrayProperty.Select(x => x.StringProperty).Min().ShouldBe("1");
            obj.Result.ArrayProperty.Select(x => x.StringProperty).Max().ShouldBe("6");
        }

        [Fact]
        public void CollectionTest04()
        {
            var str = "a1b2a3b4a5b6"; 
            var node = new Node.Node
            {
                Target = "ArrayProperty",
                ChildNodes = new List<Node.INode>
                {
                    new Node.Node
                    {
                        Pattern = "a(?<a>.)",
                        Target = "IntegerField",
                        FromVariable = "a"
                    },
                    new Node.Node
                    {
                        Pattern = "b(?<b>.)",
                        Target = "IntegerField",
                        FromVariable = "b"
                    }
                }
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.ArrayProperty.Count().ShouldBe(6);
            obj.Result.ArrayProperty.Select(x => x.IntegerField).Distinct().Count().ShouldBe(6);
            obj.Result.ArrayProperty.Select(x => x.IntegerField).Min().ShouldBe(1);
            obj.Result.ArrayProperty.Select(x => x.IntegerField).Max().ShouldBe(6);
        }
    }
}
