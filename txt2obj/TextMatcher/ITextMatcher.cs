using System;
using System.Collections.Generic;
using System.Text;

namespace txt2obj.TextMatcher
{
    public interface ITextMatcher
    {
        IEnumerable<TextMatch> GetMatches(string pattern, string text);
    }
}
