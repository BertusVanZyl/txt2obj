using System;
using System.Collections.Generic;
using System.Text;

namespace txt2obj.Parser
{
    public class ParseError
    {
        public Exception Exception { get; set; }
        public string ErrorMessage { get; set; }
    }
}
