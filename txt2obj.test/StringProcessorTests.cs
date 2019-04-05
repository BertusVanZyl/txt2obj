using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shouldly;
using txt2obj.test.TestClasses;
using txt2obj.TextProcessing;
using txt2obj.TextProcessing.Processors;
using Xunit;

namespace txt2obj.test
{
    public class StringProcessorTests
    {
        public class TestStringProcessor : IStringProcessor
        {
            public string Name => "Test";

            public string[] Parameters { get; set; }

            public string Execute(string input)
            {
                return $"{input}"+this.Parameters.Aggregate("", (s, s1) => s+s1);
            }
        }
        [Fact]
        public void ProcessStringTest01()
        {
            StringProcessorHolder h = new StringProcessorHolder();
            h.Add(new TestStringProcessor());
            h.Add(new ToUpper());
            var processorstring = "Test(1).Test(2,0x2830x29).Test(0x61).ToUpper()";
            var list = h.CreateProcessorList(processorstring);
            list.Count.ShouldBe(4);
            list[0].Parameters[0].ShouldBe("1");
            list[1].Parameters[0].ShouldBe("2");
            list[1].Parameters[1].ShouldBe("(3)");
            list[2].Parameters[0].ShouldBe("a");
            list[3].Parameters.ShouldBeEmpty();

            var result = h.ProcessAll("HELLO", processorstring);
            result.ShouldBe("HELLO12(3)A");
        }

        [Fact]
        public void ReplaceTest()
        {
            var node = new Node.Node
            {
                Process = "Replace(hello,goodbye)",
                Target = "StringProperty"
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, "hello hello hello");
            obj.Result.StringProperty.ShouldBe("goodbye goodbye goodbye");
        }

        [Fact]
        public void ToUpperTest()
        {
            var node = new Node.Node
            {
                Process = "ToUpper()",
                Target = "StringProperty"
            };
            var parser = new Parser.Parser();
            var str = "hello hello hello";
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.StringProperty.ShouldBe(str.ToUpper());
        }

        [Fact]
        public void ToLowerTest()
        {
            var node = new Node.Node
            {
                Process = "ToLower()",
                Target = "StringProperty"
            };
            var parser = new Parser.Parser();
            var str = "UPPERCASE lowercase";
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.StringProperty.ShouldBe(str.ToLower());
        }
    }
}
