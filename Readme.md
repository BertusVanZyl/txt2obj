# txt2obj

`txt2obj` turns unstructured text into strongly typed objects by running a tree of regular expression nodes over the input. Each node can capture data, move it into variables, post-process the value (uppercase, replace, etc.), and finally assign it to a property on your model. This keeps all parsing rules in one declarative structure instead of scattering regex calls throughout your code.

## Highlights

- **Tree-based templates** - compose complex parsers by nesting `Node` instances that mirror the layout of the target object graph.
- **Variable plumbing** - capture portions of text into named variables, reuse them in descendant nodes, and mutate them with setters.
- **Typed projection** - final output is deserialized into any CLR type via Newtonsoft.Json, so primitives, complex objects, and collections are supported out of the box.
- **Processing pipeline** - attach dot-chained processors (`ToUpper`, `Replace`, `ToLower`, or custom ones) to normalize data before assignment.
- **Date formatting helpers** - standardize captured strings into ISO-8601 `DateTime` values using the `Format` property.
- **Collection handling** - child nodes can emit repeated matches that are automatically grouped into lists or arrays based on your model.

## Installation

```bash
dotnet add package txt2obj
```

The package targets .NET 8.0 and depends only on `Newtonsoft.Json`.

## Quick Start

Consider parsing a till slip that contains a timestamp and repeating line items. Here is the example slip and the template that extracts it:


```csharp
var rawSlipText = @"DATE: 2020-01-02 16:22:23
LINE ITEMS START --->
Jar of cookies 22 23.55 518.10
Cigarettes 1 10.00 10.00
<--- LINE ITEMS END";

var template = new Node.Node
{
    ChildNodes = new List<Node.Node>
    {
        // Transaction timestamp
        new Node.Node
        {
            Pattern = @"(?<date>\d{4}-\d{2}-\d{2}) (?<time>\d{2}:\d{2}:\d{2})",
            ChildNodes = new List<Node.Node>
            {
                new Node.Node
                {
                    TargetVariable = "timestamp",
                    FromVariable = "date"
                },
                new Node.Node
                {
                    TargetVariable = "timestamp",
                    FromVariable = "time",
                    Setter = "|OLD| |NEW|" // append time to the stored date
                },
                new Node.Node
                {
                    Target = "TransactionTime",
                    FromVariable = "timestamp",
                    Format = "yyyy-MM-dd HH:mm:ss"
                }
            }
        },
        // Line items collection
        new Node.Node
        {
            Pattern = "LINE ITEMS START --->(?<items>.*?)<--- LINE ITEMS END",
            Target = "LineItems",
            ChildNodes = new List<Node.Node>
            {
                new Node.Node
                {
                    Pattern = @"(?<desc>[^\n\r]+)(?<qty>\d+) (?<unit>\d+\.\d{2}) (?<total>\d+\.\d{2})",
                    ChildNodes = new List<Node.Node>
                    {
                        new Node.Node { Target = "Description", FromVariable = "desc" },
                        new Node.Node { Target = "Quantity", FromVariable = "qty" },
                        new Node.Node { Target = "UnitPrice", FromVariable = "unit" },
                        new Node.Node { Target = "LineTotal", FromVariable = "total" }
                    }
                }
            }
        }
    }
};

var parser = new Parser.Parser();
var result = parser.Text2Object<SlipModel>(template, rawSlipText).Result;
```

`result` is a fully populated `SlipModel`, and you never had to manually iterate the regex matches.

Check the `txt2obj.demo` project for a runnable version that loads `slip1.txt` and shows how to build a template.

## Templates in YAML

If you prefer to store the node structure outside code, you can serialize it in YAML and load it at runtime.

```yaml
ChildNodes:
  - Pattern: "(?<date>\\d{4}-\\d{2}-\\d{2}) (?<time>\\d{2}:\\d{2}:\\d{2})"
    ChildNodes:
      - TargetVariable: timestamp
        FromVariable: date
      - TargetVariable: timestamp
        FromVariable: time
        Setter: "|OLD| |NEW|"
      - Target: TransactionTime
        FromVariable: timestamp
        Format: "yyyy-MM-dd HH:mm:ss"
  - Pattern: "LINE ITEMS START --->(?<items>.*?)<--- LINE ITEMS END"
    Target: LineItems
    ChildNodes:
      - Pattern: "(?<desc>[^\\n\\r]+)(?<qty>\\d+) (?<unit>\\d+\\.\\d{2}) (?<total>\\d+\\.\\d{2})"
        ChildNodes:
          - Target: Description
            FromVariable: desc
          - Target: Quantity
            FromVariable: qty
          - Target: UnitPrice
            FromVariable: unit
          - Target: LineTotal
            FromVariable: total
```

```csharp
// dotnet add package YamlDotNet
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using txt2obj.Node;

public class YamlNode
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

var deserializer = new DeserializerBuilder().Build();
var yaml = File.ReadAllText("template.yaml");
var template = deserializer.Deserialize<YamlNode>(yaml).ToNode();

var parser = new Parser.Parser();
var result = parser.Text2Object<SlipModel>(template, rawSlipText).Result;
```

## Node Property Reference

| Property | Purpose |
| --- | --- |
| `Pattern` | .NET `System.Text.RegularExpressions.Regex` (compiled with `RegexOptions.Singleline`) applied to the current input. Named groups are written to variables. When omitted, the node simply forwards the incoming text (or `FromVariable`). |
| `Target` | Property or field on the target object to populate. Leave blank to act as a helper node (e.g., for variable manipulation) without writing to the output. |
| `ChildNodes` | Nested nodes that receive the current node's output text for complex properties. Collection handling has its own rules (see below). |
| `FromVariable` | Pulls the node input from an existing variable instead of using the latest match. Great for reusing captured values. |
| `TargetVariable` | Stores the node output into a named variable. Combine with `Setter` to append/prepend. |
| `Setter` | Template used when `TargetVariable` already exists. `|OLD|` is replaced with the previous value, `|NEW|` with the current one. |
| `Constant` | Overrides the node input with the provided literal string (useful for seeding variables). |
| `Process` | Dot-separated processor chain (e.g., `"Replace(hello,goodbye).ToUpper()"`). Parameters are comma-separated; spaces are not supported (use `0x20` for space). The syntax matches what `StringProcessorHolder.CreateProcessorList` expects. |
| `Format` | Optional input format string used when writing to `DateTime` members. Values are converted to ISO 8601 prior to assignment. |
| `Comment` | Documentation-only field; helpful when serializing templates for sharing. |

## Working with Variables

Variables travel down the node tree so that deep descendants can reuse matches captured by their ancestors. Typical workflow:

1. Capture text with a named group (e.g., `(?<sku>ABC\d+)`).
2. Store it by setting `TargetVariable = "sku"`.
3. Use it later via `FromVariable = "sku"` to populate multiple targets or keep building with setters.

`Node.SetVariable` searches up the parent chain, so updating a variable anywhere automatically updates the shared instance.

## String Processors

Processors are lightweight classes that implement `IStringProcessor`. Three ship with the package:

- `ToUpper()`
- `ToLower()`
- `Replace(old,new)`

Attach them using the `Process` property (`"Replace(hello,goodbye).ToUpper()"`). Internally the `StringProcessorHolder` parses the expression, instantiates each processor, injects the parameters (hex escapes such as `0x20` are supported), and runs them sequentially.

### Custom Processors

```csharp
public class TrimProcessor : IStringProcessor
{
    public string Name => "Trim";
    public string[] Parameters { get; set; }
    public string Execute(string input) => input.Trim();
}

var parser = new Parser.Parser();
parser.RegisterProcessor(new TrimProcessor());
```

Once registered, `Process = "Trim()"` becomes available to every node handled by that parser instance.

## Collections & Complex Types

When a node targets a complex property (non-collection), its `ChildNodes` are executed against the node's output text and the resulting `JObject` is assigned to the property. If the property is an `IEnumerable`, array, or `List`, `txt2obj` automatically:

1. Determines the element type via reflection.
2. Runs each child node independently against the collection node's input text (not the collection node's own `Pattern`/`Process` output).
3. Orders matches by their index in the source text so the items stay aligned with the input sequence.

See `txt2obj.test/CollectionTest.cs` for multiple patterns that fill lists and arrays from different regex strategies.

## Running the Demo & Tests

```bash
# Demo (prints parsed slip to the console)
dotnet run --project txt2obj.demo

# Test suite
dotnet test
```

## Contributing

Issues and pull requests are welcome. If you add new processors or helper APIs, please also add/extend unit tests under `txt2obj.test` and update this README accordingly.
