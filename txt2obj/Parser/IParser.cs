using System;
using System.Collections.Generic;
using System.Text;
using txt2obj.Node;
using txt2obj.TextProcessing;

namespace txt2obj.Parser
{
    public interface IParser
    {
        ParserResult<T> Text2Object<T>(Node.Node node, string text);
        ParserResult<T> Text2Object<T>(Node.Node node, string text, T obj);
        void RegisterProcessor(IStringProcessor processor);
    }
}
