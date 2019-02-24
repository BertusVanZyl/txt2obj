using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shouldly;
using Xunit;

namespace txt2obj.test
{
    
    public class RegexTextMatcherTests
    {
        [Fact]
        public void Match001()
        {
            var matcher = new TextMatcher.RegexTextMatcher();
            var matches = matcher.GetMatches(".", "abc").ToList();
            matches.Count.ShouldBe(3);
            matches.Where(x => x.Name == "0").Count().ShouldBe(3);
            matches[0].Value.ShouldBe("a");
            matches[1].Value.ShouldBe("b");
            matches[2].Value.ShouldBe("c");
        }

        [Fact]
        public void Match002()
        {
            var matcher = new TextMatcher.RegexTextMatcher();
            var matches = matcher.GetMatches("(?<capturename>.)", "abc").ToList();
            matches.Count.ShouldBe(6);
            matches.Where(x => x.Name == "capturename").Count().ShouldBe(3);
            matches.Where(x => x.Name == "0").Count().ShouldBe(3);
            var hasName = matches.Where(x => x.Name == "capturename").OrderBy(x => x.Name).ToList();
            hasName[0].Value.ShouldBe("a");
            hasName[1].Value.ShouldBe("b");
            hasName[2].Value.ShouldBe("c");

            var wholeMatch = matches.Where(x => x.Name == "0").OrderBy(x => x.Name).ToList();
            wholeMatch[0].Value.ShouldBe("a");
            wholeMatch[1].Value.ShouldBe("b");
            wholeMatch[2].Value.ShouldBe("c");
        }

        [Fact]
        public void Match003()
        {
            var matcher = new TextMatcher.RegexTextMatcher();
            var matches = matcher.GetMatches("(?<capturename>..)", "abc").ToList();
            matches.Count().ShouldBe(2);
            matches = matches.OrderBy(x => x.Name).ToList();
            matches[0].Value.ShouldBe("ab");
            matches[0].Name.ShouldBe("0");
            matches[1].Value.ShouldBe("ab");
            matches[1].Name.ShouldBe("capturename");
        }

        [Fact]
        public void Match004()
        {
            var matcher = new TextMatcher.RegexTextMatcher();
            var matches = matcher.GetMatches("abc(?<capturename>.)", "abc1abc2").ToList();
            matches.Count().ShouldBe(4);
            matches = matches.OrderBy(x => x.Name).ThenBy(x => x.Value).ToList();
            matches[0].Value.ShouldBe("abc1");
            matches[0].Name.ShouldBe("0");
            matches[1].Value.ShouldBe("abc2");
            matches[1].Name.ShouldBe("0");
            matches[2].Value.ShouldBe("1");
            matches[2].Name.ShouldBe("capturename");
            matches[3].Value.ShouldBe("2");
            matches[3].Name.ShouldBe("capturename");
        }

    }
}
