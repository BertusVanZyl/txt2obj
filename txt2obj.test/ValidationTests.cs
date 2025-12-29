using System;
using System.Collections.Generic;
using txt2obj.test.TestClasses;
using Xunit;

namespace txt2obj.test
{
    public class ValidationTests
    {
        [Fact]
        public void CollectionChildRequiresPattern()
        {
            var node = new Node.Node
            {
                Target = "ListProperty",
                ChildNodes = new List<Node.Node>
                {
                    new Node.Node
                    {
                        Target = "StringProperty"
                    }
                }
            };

            var parser = new Parser.Parser();

            Assert.Throws<InvalidOperationException>(() => parser.Text2Object<TestObj1>(node, "abc"));
        }

        [Fact]
        public void TargetMustExistOnType()
        {
            var node = new Node.Node
            {
                Target = "MissingProperty",
                Pattern = "(.*)"
            };

            var parser = new Parser.Parser();

            Assert.Throws<InvalidOperationException>(() => parser.Text2Object<TestObj1>(node, "abc"));
        }
    }
}
