using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace txt2obj.TextMatcher
{
    public class RegexTextMatcher : ITestMatcher
    {
        public IEnumerable<TextMatch> GetMatches(string pattern, string text)
        {
            var r = new Regex(pattern, RegexOptions.Singleline);
            var groupNames = r.GetGroupNames();
            var matches = r.Matches(text);
            foreach (var match in matches)
            {
                var m = match as Match;
                foreach (var groupName in groupNames)
                {
                    var group = m.Groups[groupName];
                    yield return new TextMatch
                    {
                        Name = groupName,
                        Value = group.Value
                    };
                }
            }
        }
    }
}
