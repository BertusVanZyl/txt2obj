using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using Xunit;

namespace txt2obj.test
{
    public class BasicParseTests
    {
        [Fact]
        public void SimpleStringWholeMatch()
        {
            var str = "hello";
            var node = new Node.Node
            {
                Pattern = "(.*)",
                Target = "StringProperty"
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.StringProperty.ShouldBe(str);
        }

        [Fact]
        public void SimpleStringFromVariable()
        {
            var str = "hello";
            var node = new Node.Node
            {
                Pattern = "he(?<v1>ll)o",
                Target = "StringProperty",
                FromVariable = "v1"
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.StringProperty.ShouldBe("ll");
        }

        [Fact]
        public void SimpleIntFromVariable()
        {
            var str = "12345";
            var node = new Node.Node
            {
                Pattern = "12(?<v1>3)45",
                Target = "IntegerProperty",
                FromVariable = "v1"
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.IntegerProperty.ShouldBe(3);
        }

        [Fact]
        public void SimpleDecimalFromVariable()
        {
            var str = "123.45";
            var node = new Node.Node
            {
                Pattern = "12(?<v1>3.4)5",
                Target = "DecimalProperty",
                FromVariable = "v1"
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.DecimalProperty.ShouldBe(3.4M);
        }
    }
}
