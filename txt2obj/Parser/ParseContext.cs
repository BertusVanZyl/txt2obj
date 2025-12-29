using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace txt2obj.Parser
{
    public class ParseContext
    {
        public ParseContext()
        {
            JObj = new JsonObject();
        }
        public JsonObject JObj { get; set; }
        public List<ParseError> Errors = new List<ParseError>();
    }
}
