using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using txt2obj.Node;
using txt2obj.TextMatcher;

namespace txt2obj.Parser
{
    public class Parser : IParser
    {
        public ParserResult<T> Text2Object<T>(INode node, string text)
        {
            node.Prepare();
            var jobj = ProcessNode(node, text, typeof(T));
            var obj = jobj.ToObject<T>();
            return new ParserResult<T>
            {
                Result = obj
            };
        }

        JObject ProcessNode(INode node, string text, Type t)
        {
            var jobj = new JObject();
            var resultText = text;
            //if there is a pattern, run it against the text now.
            if (!String.IsNullOrEmpty(node.Pattern))
            {
                ITextMatcher matcher = new RegexTextMatcher();
                var matches = matcher.GetMatches(node.Pattern, text);
                foreach (var match in matches)
                {
                    node.SetVariable(match.Name, match.Value);
                }

                if (String.IsNullOrEmpty(node.FromVariable))
                {
                    var completeMatch = matches.FirstOrDefault(x => x.Name == "0");
                    if (completeMatch != null) resultText = completeMatch.Value;
                }
                else
                {
                    var variableMatch = matches.FirstOrDefault(x => x.Name == node.FromVariable);
                    if (variableMatch != null) resultText = variableMatch.Value;
                }
            }
            
            if (!String.IsNullOrEmpty(node.Target))
            {
                var property = t.GetProperty(node.Target);
                var propertyType = property.PropertyType;
                if (Helpers.HelperMethods.IsSimple(propertyType))
                {
                    jobj[property.Name] = resultText;
                }
            }

            return jobj;
            //foreach (var property in properties)
            //{
            //    if (property.GetType() == typeof(string))
            //    {
            //        jobj[property.Name] = property.GetValue()
            //    }
            //}
        }
    }
}
