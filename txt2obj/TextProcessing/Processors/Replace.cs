using System;
using System.Collections.Generic;
using System.Text;

namespace txt2obj.TextProcessing.Processors
{
    public class Replace : IStringProcessor
    {
        public string Name => "Replace";
        public string[] Parameters { get; set; }
        public string Execute(string input)
        {
            return input.Replace(Parameters[0], Parameters[1]);
        }
    }
}
