using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using txt2obj.test.TestClasses;
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

        [Fact]
        public void AnonymousType()
        {
            var str = "12345";
            var a = new {A = 1};

            var node = new Node.Node
            {
                Pattern = "12(?<v1>3)45",
                Target = "A",
                FromVariable = "v1"
            };

            var parser = new Parser.Parser();
            var obj = parser.Text2Object(node, str, a);
            obj.Result.A.ShouldBe(3);
        }

        [Fact]
        public void DateTimeNoFormatter()
        {
            var str = "aaa2019-02-03 11:33:44bbb";
            var node = new Node.Node
            {
                Pattern = "aaa(?<dt>.*?)bbb",
                Target = "DateTimeProperty",
                FromVariable = "dt"
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);

        }

        [Fact]
        public void DateTimeWithFormatter()
        {
            var str = "aaa02-2019-03 11:33:44bbb";
            var node = new Node.Node
            {
                Pattern = "aaa(?<dt>.*?)bbb",
                Target = "DateTimeProperty",
                FromVariable = "dt",
                Format = "MM-yyyy-dd HH:ss:mm"
            };
            var parser = new Parser.Parser();
            var obj = parser.Text2Object<TestObj1>(node, str);
            obj.Result.DateTimeProperty.Year.ShouldBe(2019);
            obj.Result.DateTimeProperty.Month.ShouldBe(02);
            obj.Result.DateTimeProperty.Day.ShouldBe(03);
            obj.Result.DateTimeProperty.Hour.ShouldBe(11);
            obj.Result.DateTimeProperty.Minute.ShouldBe(44);
            obj.Result.DateTimeProperty.Second.ShouldBe(33);
        }
    }
}
