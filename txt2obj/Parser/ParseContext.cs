using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace txt2obj.Parser
{
    public class ParseContext
    {
        public ParseContext()
        {
            JObj = new JObject();
        }
        public JObject JObj { get; set; }
        public List<ParseError> Errors = new List<ParseError>();
    }
}
