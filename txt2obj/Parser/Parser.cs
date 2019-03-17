using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            var context = new ParseContext();
            ProcessNode(node, text, typeof(T),context);
            var obj = context.JObj.ToObject<T>();
            return new ParserResult<T>
            {
                Result = obj
            };
        }

        public ParserResult<T> Text2Object<T>(INode node, string text, T obj)
        {
            node.Prepare();
            var context = new ParseContext();
            ProcessNode(node, text, typeof(T),context);
            var o = context.JObj.ToObject<T>();
            return new ParserResult<T>
            {
                Result = o
            };
        }

        private string ProcessStringAgainstNode(INode node, string text)
        {
            var resultText = text;
            if (!String.IsNullOrEmpty(node.Pattern))
            {
                ITextMatcher matcher = new RegexTextMatcher();
                var matches = matcher.GetMatches(node.Pattern, text);
                foreach (var match in matches)
                {
                    node.SetVariable(match.Name, match.Value);
                }
                //if there is a match pattern, and no FromVariable, use the complete match
                if (String.IsNullOrEmpty(node.FromVariable))
                {
                    var completeMatch = matches.FirstOrDefault(x => x.Name == "0");
                    if (completeMatch != null)
                    {
                        resultText = completeMatch.Value;
                    }
                }
                else
                {
                    var variable = node.GetVariable(node.FromVariable);
                    if (variable != null)
                    {
                        resultText = variable.Value;
                    }
                }
            }
            else
            {
                //no pattern
                if (!String.IsNullOrEmpty(node.FromVariable))
                {
                    var variable = node.GetVariable(node.FromVariable);
                    if (variable != null)
                    {
                        resultText = variable.Value;
                    }
                }
            }
            

            return resultText;
        }

        private void ProcessCollection(INode node, string text, Type t, ParseContext context)
        {
            var jArray = new JArray();
            List<Tuple<int,JObject>> objectsByIndex = new List<Tuple<int,JObject>>();
            ITextMatcher matcher = new RegexTextMatcher();
            var collectionType = Helpers.HelperMethods.GetCollectionType(t);
            foreach (var childNode in node.ChildNodes)
            {
                var collectionSubTypeContext = new ParseContext();
                var matchesForChildNode = matcher.GetMatches(childNode.Pattern, text);
                if (!String.IsNullOrEmpty(childNode.Pattern))
                {
                    List<TextMatch> matches = null;
                    if (!String.IsNullOrEmpty(childNode.FromVariable))
                    {
                        matches = matchesForChildNode.Where(y => y.Name == childNode.FromVariable).ToList();
                    }
                    else
                    {
                        matches = matchesForChildNode.Where(y => y.Name == "0").ToList();
                    }

                    foreach (var match in matches)
                    {
                        var itemContext = new ParseContext();
                        ProcessNode(childNode, match.Value, collectionType, itemContext);
                        objectsByIndex.Add(new Tuple<int,JObject>(match.Position, itemContext.JObj));
                        //objectsByIndex.Add(match.Position, itemContext.JObj);
                        //jArray.Add(itemContext.JObj);
                    }
                }
            }

            var orderedByIndex = objectsByIndex.OrderBy(x => x.Item1).ToList();
            foreach (var jobj in orderedByIndex.Select(x => x.Item2))
            {
                jArray.Add(jobj);
            }
            
            context.JObj[node.Target] = jArray;
        }

        private void ProcessNode(INode node, string text, Type t, ParseContext context)
        {
            //var jobj = new JObject();
            var resultText = ProcessStringAgainstNode(node, text);
            
            if (!String.IsNullOrEmpty(node.Target))
            {
                var property = t.GetProperty(node.Target);
                var propertyType = property.PropertyType;
                if (Helpers.HelperMethods.IsSimple(propertyType))
                {
                    //simple object, just add text
                    context.JObj[property.Name] = resultText;
                }
                else
                {
                    var newContext = new ParseContext();
                    //complex object, start new Jobj
                    if (Helpers.HelperMethods.IsCollection(propertyType))
                    {
                        ProcessCollection(node, text, propertyType, newContext);
                    }
                    else
                    {
                        foreach (var childNode in node.ChildNodes)
                        {
                            ProcessNode(childNode, resultText, propertyType, newContext);
                        }
                        context.JObj[node.Target] = newContext.JObj;
                        context.Errors.AddRange(newContext.Errors);
                    }
                }
            }
        }
    }
}
