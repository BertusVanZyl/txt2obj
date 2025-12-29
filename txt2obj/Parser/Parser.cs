using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using txt2obj.Extentions;
using txt2obj.Node;
using txt2obj.TextMatcher;
using txt2obj.TextProcessing;
using txt2obj.TextProcessing.Processors;

namespace txt2obj.Parser
{
    public class Parser : IParser
    {
        private StringProcessorHolder StringProcessorHolder = new StringProcessorHolder();
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        };
        public Parser()
        {
            this.RegisterProcessor(new ToUpper());
            this.RegisterProcessor(new ToLower());
            this.RegisterProcessor(new Replace());
        }
        public ParserResult<T> Text2Object<T>(Node.Node node, string text)
        {
            node.Prepare();
            var context = new ParseContext();
            ProcessNode(node, text, typeof(T),context);
            var obj = JsonSerializer.Deserialize<T>(context.JObj.ToJsonString(), SerializerOptions);
            return new ParserResult<T>
            {
                Result = obj
            };
        }

        public ParserResult<T> Text2Object<T>(Node.Node node, string text, T obj)
        {
            node.Prepare();
            var context = new ParseContext();
            ProcessNode(node, text, typeof(T),context);
            var o = JsonSerializer.Deserialize<T>(context.JObj.ToJsonString(), SerializerOptions);
            return new ParserResult<T>
            {
                Result = o
            };
        }

        private string ProcessStringAgainstNode(Node.Node node, string text)
        {
            var resultText = text;
            if (node.Constant.IsSet())
            {
                resultText = node.Constant;
            }
            if (node.Pattern.IsSet())
            {
                ITextMatcher matcher = new RegexTextMatcher();
                var matches = matcher.GetMatches(node.Pattern, text);
                foreach (var match in matches)
                {
                    node.SetVariable(match.Name, match.Value);
                }
                //if there is a match pattern, and no FromVariable, use the complete match
                if (!node.FromVariable.IsSet())
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
                if (node.FromVariable.IsSet())
                {
                    var variable = node.GetVariable(node.FromVariable);
                    if (variable != null)
                    {
                        resultText = variable.Value;
                    }
                }
            }

            if (!String.IsNullOrEmpty(node.Process))
            {
                resultText = this.StringProcessorHolder.ProcessAll(resultText, node.Process);
            }

            return resultText;
        }

        private JsonArray ProcessCollection(Node.Node node, string text, Type t)
        {
            var jArray = new JsonArray();
            List<Tuple<int,JsonObject>> objectsByIndex = new List<Tuple<int,JsonObject>>();
            ITextMatcher matcher = new RegexTextMatcher();
            var collectionType = Helpers.HelperMethods.GetCollectionType(t);
            foreach (var childNode in node.ChildNodes)
            {
                var collectionSubTypeContext = new ParseContext();
                var matchesForChildNode = matcher.GetMatches(childNode.Pattern, text);
                if (!String.IsNullOrEmpty(childNode.Pattern))
                {
                    List<TextMatch> matches = null;
                    if (childNode.FromVariable.IsSet())
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
                        objectsByIndex.Add(new Tuple<int,JsonObject>(match.Position, itemContext.JObj));
                    }
                }
            }

            var orderedByIndex = objectsByIndex.OrderBy(x => x.Item1).ToList();
            foreach (var jobj in orderedByIndex.Select(x => x.Item2))
            {
                jArray.Add(jobj);
            }
            return jArray;
        }

        private void ProcessNode(Node.Node node, string text, Type t, ParseContext context)
        {
            var resultText = ProcessStringAgainstNode(node, text);

            if (node.TargetVariable.IsSet())
            {
                if (node.Setter.IsSet())
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
            
            if (node.Target.IsSet())
            {
                var propertyType = Helpers.HelperMethods.GetTypePropertyOrFieldType(t, node.Target);
                
                if (Helpers.HelperMethods.IsSimple(propertyType))
                {
                    //if datetime, attempt to use Format to standardise the datetime into ISO 8601
                    if (node.Format.IsSet() && Helpers.HelperMethods.IsDateTime(propertyType))
                    {
                        resultText = Helpers.HelperMethods.StandardiseDateTime(resultText, node.Format, propertyType);
                    }
                    //simple object, just add text
                    context.JObj[node.Target] = CreateSimpleNode(propertyType, resultText);
                }
                else
                {
                    var newContext = new ParseContext();
                    //complex object, start new Jobj
                    if (Helpers.HelperMethods.IsCollection(propertyType))
                    {
                        var collectionArray = ProcessCollection(node, text, propertyType);
                        context.JObj[node.Target] = collectionArray;
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

        private static JsonNode CreateSimpleNode(Type propertyType, string value)
        {
            if (value == null)
            {
                return JsonValue.Create((string)null);
            }

            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (targetType == typeof(string))
            {
                return JsonValue.Create(value);
            }

            if (targetType == typeof(Guid))
            {
                if (Guid.TryParse(value, out var guidValue))
                {
                    return JsonValue.Create(guidValue);
                }
                return JsonValue.Create(value);
            }

            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateValue))
                {
                    return JsonValue.Create(dateValue);
                }
                return JsonValue.Create(value);
            }

            if (targetType == typeof(DateTimeOffset))
            {
                if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateValue))
                {
                    return JsonValue.Create(dateValue);
                }
                return JsonValue.Create(value);
            }

            if (targetType == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var timeValue))
                {
                    return JsonValue.Create(timeValue);
                }
                return JsonValue.Create(value);
            }

            if (targetType.IsEnum)
            {
                try
                {
                    var enumValue = Enum.Parse(targetType, value, true);
                    return JsonValue.Create(enumValue);
                }
                catch
                {
                    return JsonValue.Create(value);
                }
            }

            try
            {
                var converted = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                return JsonValue.Create(converted);
            }
            catch
            {
                return JsonValue.Create(value);
            }
        }
    }
}
