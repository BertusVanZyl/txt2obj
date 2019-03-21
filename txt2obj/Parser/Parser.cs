using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using txt2obj.Node;
using txt2obj.TextMatcher;
using txt2obj.TextProcessing;
using txt2obj.TextProcessing.Processors;

namespace txt2obj.Parser
{
    public class Parser : IParser
    {
        private StringProcessorHolder StringProcessorHolder = new StringProcessorHolder();
        public Parser()
        {
            this.RegisterProcessor(new ToUpper());
        }
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
            if (!String.IsNullOrEmpty(node.Constant))
            {
                resultText = node.Constant;
            }
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
            var resultText = ProcessStringAgainstNode(node, text);

            if (!String.IsNullOrEmpty(node.TargetVariable))
            {
                if (!String.IsNullOrEmpty(node.Setter))
                {
                    var setterStr = node.Setter;
                    var targetVariable = node.GetVariable(node.TargetVariable);
                    if (targetVariable == null)
                    {
                        setterStr = setterStr.Replace("|OLD|", String.Empty);
                    }
                    else
                    {
                        setterStr = setterStr.Replace("|OLD|", targetVariable.Value);
                    }

                    setterStr = setterStr.Replace("|NEW|", resultText);
                    resultText = setterStr;
                }
                node.SetVariable(node.TargetVariable, resultText);
            }
            
            if (!String.IsNullOrEmpty(node.Target))
            {
                var propertyType = Helpers.HelperMethods.GetTypePropertyOrFieldType(t, node.Target);
                
                if (Helpers.HelperMethods.IsSimple(propertyType))
                {
                    //if datetime, attempt to use Format to standardise the datetime into ISO 8601
                    if (!String.IsNullOrEmpty(node.Format) && Helpers.HelperMethods.IsDateTime(propertyType))
                    {
                        resultText = Helpers.HelperMethods.StandardiseDateTime(resultText, node.Format, propertyType);
                    }
                    //simple object, just add text
                    context.JObj[node.Target] = resultText;
                }
                else
                {
                    var newContext = new ParseContext();
                    //complex object, start new Jobj
                    if (Helpers.HelperMethods.IsCollection(propertyType))
                    {
                        ProcessCollection(node, text, propertyType, newContext);
                        context.JObj[node.Target] = newContext.JObj[node.Target];
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
            else
            {
                //var newContext = new ParseContext();
                foreach (var childNode in node.ChildNodes)
                {
                    ProcessNode(childNode, resultText, t, context);
                }
                //context.JObj[node.Target] = newContext.JObj;
                //context.Errors.AddRange(newContext.Errors);
            }
        }

        public void RegisterProcessor(IStringProcessor processor)
        {
            this.StringProcessorHolder.Add(processor);
        }
    }
}
