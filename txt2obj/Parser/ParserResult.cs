using System;
using System.Collections.Generic;
using System.Text;

namespace txt2obj.Parser
{
    public class ParserResult<T>
    {
        public T Result { get; set; }
        List<ParseError> Errors = new List<ParseError>();
    }
}
