using System;
using System.Collections.Generic;
using System.Text;

namespace txt2obj.TextProcessing.Processors
{
    public class ToLower : IStringProcessor
    {
        public string Name => "ToLower";
        public string[] Parameters { get; set; }
        public string Execute(string input)
        {
            return input.ToLower();
        }
    }
}
