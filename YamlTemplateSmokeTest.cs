using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using txt2obj.Node;

namespace txt2obj
{
    // Quick manual smoke test for the README YAML template.
    // Requires: dotnet add package YamlDotNet
    public static class YamlTemplateSmokeTest
    {
        private class SlipModel
        {
            public DateTime TransactionTime { get; set; }
            public SlipLineItemModel[] LineItems { get; set; }
        }

        private class SlipLineItemModel
        {
            public string Description { get; set; }
            public int Quantity { get; set; }
            public decimal LineTotal { get; set; }
            public decimal UnitPrice { get; set; }
        }

        private class YamlNode
        {
            public List<YamlNode> ChildNodes { get; set; }
            public string Comment { get; set; }
            public string Target { get; set; }
            public string Pattern { get; set; }
            public string FromVariable { get; set; }
            public string TargetVariable { get; set; }
            public string Format { get; set; }
            public string Setter { get; set; }
            public string Constant { get; set; }
            public string Process { get; set; }

            public Node.Node ToNode()
            {
                var node = new Node.Node
                {
                    Comment = Comment,
                    Target = Target,
                    Pattern = Pattern,
                    FromVariable = FromVariable,
                    TargetVariable = TargetVariable,
                    Format = Format,
                    Setter = Setter,
                    Constant = Constant,
                    Process = Process
                };

                if (ChildNodes != null && ChildNodes.Count > 0)
                {
                    node.ChildNodes = ChildNodes.Select(c => c.ToNode()).ToList();
                }

                return node;
            }
        }

        public static void Run()
        {
            var rawSlipText = @"DATE: 2020-01-02 16:22:23
LINE ITEMS START --->
Jar of cookies 22 23.55 518.10
Cigarettes 1 10.00 10.00
<--- LINE ITEMS END";

            var yaml = @"ChildNodes:
  - Pattern: ""(?<date>\\d{4}-\\d{2}-\\d{2}) (?<time>\\d{2}:\\d{2}:\\d{2})""
    ChildNodes:
      - TargetVariable: timestamp
        FromVariable: date
      - TargetVariable: timestamp
        FromVariable: time
        Setter: ""|OLD| |NEW|""
      - Target: TransactionTime
        FromVariable: timestamp
        Format: ""yyyy-MM-dd HH:mm:ss""
  - Pattern: ""LINE ITEMS START --->(?<items>.*?)<--- LINE ITEMS END""
    Target: LineItems
    ChildNodes:
      - Pattern: ""(?<desc>[^\\n\\r]+)(?<qty>\\d+) (?<unit>\\d+\\.\\d{2}) (?<total>\\d+\\.\\d{2})""
        ChildNodes:
          - Target: Description
            FromVariable: desc
          - Target: Quantity
            FromVariable: qty
          - Target: UnitPrice
            FromVariable: unit
          - Target: LineTotal
            FromVariable: total
";

            var deserializer = new DeserializerBuilder().Build();
            var yamlNode = deserializer.Deserialize<YamlNode>(yaml);
            var template = yamlNode.ToNode();

            var parser = new Parser.Parser();
            var result = parser.Text2Object<SlipModel>(template, rawSlipText).Result;

            if (result.LineItems == null || result.LineItems.Length != 2)
            {
                throw new Exception("YAML template did not parse the expected line items.");
            }
        }
    }
}
