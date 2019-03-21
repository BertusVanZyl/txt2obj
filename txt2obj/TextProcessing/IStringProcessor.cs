using System;
using System.Collections.Generic;
using System.Text;

namespace txt2obj.TextProcessing
{
    public interface IStringProcessor
    {
        string Name { get;}
        string[] Parameters { get; set; }

        string Execute(string input);
    }
}
