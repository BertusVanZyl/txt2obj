using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using txt2obj.Node;
using Newtonsoft.Json.Serialization;
using System.Collections;
using txt2obj.Variables;

namespace txt2obj.demo
{
    class Program
    {
        private static string GetResource(string name)
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            var ttt1 = assembly.GetManifestResourceNames();
            var resourceName = assembly.GetManifestResourceNames().First(x => x.Contains(name));
            Stream resource = assembly.GetManifestResourceStream(resourceName);
            var memoryStream = new MemoryStream();
            resource.CopyTo(memoryStream);
            return System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer());
        }

        public class SlipModel
        {
            public DateTime TransactionTime { get; set; }
            public List<SlipLineItemModel> LineItems { get; set; }
        }

        public class SlipLineItemModel
        {
            public string Description { get; set; }
            public  int Quantity { get; set; }
            public decimal LineTotal { get; set; }
            public decimal UnitPrice { get; set; }
        }

        public class ShouldSerializeContractResolver : DefaultContractResolver
        {
            public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyType != typeof(string))
                {
                    if (property.PropertyType.GetInterface(nameof(IEnumerable)) != null)
                        property.ShouldSerialize =
                            instance => (instance?.GetType().GetProperty(property.PropertyName).GetValue(instance) as IEnumerable<object>)?.Count() > 0;
                }

                if (property.PropertyType == typeof(VariableHolder))
                {

                }
                return property;
            }
        }
        static void Main(string[] args)
        {
            var sourceString = GetResource("slip1");

            var template = new Node.Node
            {
                ChildNodes = new List<INode>
                {
                    new Node.Node
                    {
                        Comment = "Parse the transaction date time",
                        Pattern = @"(?<datepart>\d\d\d\d-\d\d-\d\d) (?<timepart>\d\d:\d\d:\d\d)",
                        ChildNodes = new List<INode>
                        {
                            new Node.Node
                            {
                                Comment = "Save the date part into a new variable called dateandtimecombimed",
                                TargetVariable = "dateandtimecombimed",
                                FromVariable = "datepart"
                            },
                            new Node.Node()
                            {
                                Comment = "Take the value from timepart, and append it to dateandtimecombimed, using a setter",
                                TargetVariable = "dateandtimecombimed",
                                FromVariable = "timepart",
                                Setter = "|OLD| |NEW|"
                            },
                            new Node.Node()
                            {
                                Comment = "Write the value from dateandtimecombimed into the result model",
                                Target = "TransactionTime",
                                FromVariable = "dateandtimecombimed",
                                Format = "yyyy-MM-dd HH:mm:ss"
                            }
                        }
                    },
                    new Node.Node()
                    {
                        Comment = "Grab the part of the slip that contains the line items.",
                        Pattern = "LINE ITEMS START --->(?<lineitemssection>.*?)<--- LINE ITEMS END",
                        Target = "LineItems",
                        ChildNodes = new List<INode>
                        {
                            new Node.Node
                            {
                                Pattern = @"(?<desc>[^\n\r]+)(?<quantity>\d+?) (?<unitprice>\d+\.\d\d?) (?<linetotal>\d+\.\d\d?)",
                                ChildNodes = new List<INode>
                                {
                                    new Node.Node
                                    {
                                        Target = "Description",
                                        FromVariable = "desc"
                                    },
                                    new Node.Node
                                    {
                                        Target = "Quantity",
                                        FromVariable = "quantity"
                                    },
                                    new Node.Node
                                    {
                                        Target = "UnitPrice",
                                        FromVariable = "unitprice"
                                    },
                                    new Node.Node
                                    {
                                        Target = "LineTotal",
                                        FromVariable = "linetotal"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var templateJson = JsonConvert.SerializeObject(template,
                Newtonsoft.Json.Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new ShouldSerializeContractResolver()
                    
                });

            var parser = new Parser.Parser();
            var resultSlip = parser.Text2Object<SlipModel>(template, sourceString).Result;



        }
    }
}
