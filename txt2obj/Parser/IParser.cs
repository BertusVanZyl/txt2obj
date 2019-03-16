using System;
using System.Collections.Generic;
using System.Text;
using txt2obj.Node;

namespace txt2obj.Parser
{
    public interface IParser
    {
        ParserResult<T> Text2Object<T>(INode node, string text);
    }
}
