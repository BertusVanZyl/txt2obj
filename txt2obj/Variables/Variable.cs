using System;
using System.Collections.Generic;
using System.Text;

namespace txt2obj.Variables
{
    public class Variable
    {
        public Variable(string name, string value)
        {
            this.Value = value;
            this.Name = name;
        }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
