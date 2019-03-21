using System;
using System.Collections.Generic;
using System.Text;
using txt2obj.Node;
using txt2obj.TextProcessing;

namespace txt2obj.Parser
{
    public interface IParser
    {
        ParserResult<T> Text2Object<T>(INode node, string text);
        ParserResult<T> Text2Object<T>(INode node, string text, T obj);
        void RegisterProcessor(IStringProcessor processor);
    }
}
