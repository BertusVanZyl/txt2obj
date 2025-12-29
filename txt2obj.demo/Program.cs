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
using YamlDotNet.Serialization;

namespace txt2obj.demo
{
    class Program
    {
        

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
            var parser = new Parser.Parser();
            var sourceString = GetResource("slip1");
            var json_template = GetResource("template1.json");
            var template = JsonConvert.DeserializeObject<Node.Node>(json_template);
            var resultSlip_from_json = parser.Text2Object<SlipModel>(template, sourceString).Result;

            var yaml_template = GetResource("template1.yaml");
            var yamlDeserializer = new DeserializerBuilder().Build();
            var template_from_yaml = yamlDeserializer.Deserialize<Node.Node>(yaml_template);
            var resultSlip_from_yaml = parser.Text2Object<SlipModel>(template_from_yaml, sourceString).Result;
        }
        
        /// <summary>
        /// Retrieves the content of an embedded resource specified by name from the assembly containing the 'Program' class.
        /// </summary>
        /// <param name="name">Part of the name of the resource to find.</param>
        /// <returns>Content of the resource as a UTF-8 encoded string.</returns>
        private static string GetResource(string name)
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            var resourceNames = assembly.GetManifestResourceNames().ToList();

            var resourceName = resourceNames
                .FirstOrDefault(x => x.EndsWith(name, StringComparison.OrdinalIgnoreCase))
                ?? resourceNames.FirstOrDefault(x => x.Contains(name, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                throw new InvalidOperationException($"Resource not found for '{name}'.");
            }
            Stream resource = assembly.GetManifestResourceStream(resourceName);
            var memoryStream = new MemoryStream();
            resource.CopyTo(memoryStream);
            return System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer());
        }
    }
}
