using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using txt2obj.TextMatcher;

namespace txt2obj.TextProcessing
{
    public class StringProcessorHolder
    {
        private Dictionary<string, IStringProcessor> ProcessorsDictionary = new Dictionary<string, IStringProcessor>();
        private object _lockObj = new object();
        public void Add(IStringProcessor processor)
        {
            lock (this._lockObj)
            {
                if (this.ProcessorsDictionary.ContainsKey(processor.Name))
                {
                    this.ProcessorsDictionary.Remove(processor.Name);
                }
                this.ProcessorsDictionary.Add(processor.Name, processor);
            }
        }

        public string ProcessAll(string input, string processString)
        {
            var list = CreateProcessorList(processString);
            foreach (var processor in list)
            {
                input = processor.Execute(input);
            }

            return input;
        }

        public string ReplaceHex(string input)
        {
            var regexMatcher = new RegexTextMatcher();
            var matches = regexMatcher.GetMatches("(?<hex>0x[0-9A-F]{2})", input);
            foreach (var match in matches.Where(x => x.Name == "hex"))
            {
                var num = byte.Parse(match.Value.Substring(2), System.Globalization.NumberStyles.HexNumber);
                var str = System.Text.Encoding.UTF8.GetString(new byte[] {num});
                input = input.Replace(match.Value, str);
            }

            return input;
        }

        public List<IStringProcessor> CreateProcessorList(string processString)
        {
            var regex = "(?<name>[a-zA-Z0-9_]+)\\((?<par>[a-zA-Z0-9_,/.]*)\\)\\.?";
            var regexMatcher = new RegexTextMatcher();
            var matches = regexMatcher.GetMatches(regex, processString)
                .Where(x => x.Name == "par" || x.Name == "name")
                .OrderBy(x => x.Position);

            var names = matches.Where(x => x.Name == "name").ToList();
            var pars = matches.Where(x => x.Name == "par").ToList();

            if (names.Count() != pars.Count())
            {
                //malformed
                return null;
            }

            var processors =names.Zip(pars, (nameMatch, parMatch) =>
                {
                    var p = this.ProcessorsDictionary.ContainsKey(nameMatch.Value)
                        ? this.ProcessorsDictionary[nameMatch.Value]
                        : null;
                    if (p == null) return null;

                    var parArray = parMatch.Value.Split(',')
                        .Select(ReplaceHex)
                        .Where(x => !String.IsNullOrEmpty(x))
                        .ToArray();

                    var type = p.GetType();
                    p = (IStringProcessor) Activator.CreateInstance(type);
                    p.Parameters = parArray;
                    return p;
                }).Where(x => x != null)
                .ToList();

            return processors;
        }
    }
}
