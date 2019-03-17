using System;
using System.Collections.Generic;
using System.Text;
using txt2obj.Variables;

namespace txt2obj.Node
{
    public interface INode
    {
        List<INode> ChildNodes { get; set; }
    
        Variable GetVariable(string name);
        void SetVariable(string name, string value);
        void Prepare();
        string Comment { get; set; }
        string Pattern { get; set; }
        string Target { get; set; }
        string FromVariable { get; set; }
    }
}
