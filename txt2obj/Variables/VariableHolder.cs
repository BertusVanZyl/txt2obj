using System;
using System.Collections.Generic;
using System.Text;

namespace txt2obj.Variables
{
    public class VariableHolder
    {
        public VariableHolder()
        {

        }
        private object _lockObj = new object();
        private Dictionary<string, Variable> VariableDic = new Dictionary<string, Variable>();

        public Variable this[string key]
        {
            get
            {
                lock (this._lockObj)
                {
                    return this.VariableDic.ContainsKey(key) ? this.VariableDic[key] : null;
                }
            }
            set
            {
                lock (this._lockObj)
                {
                    if (this.VariableDic.ContainsKey(key))
                    {
                        this.VariableDic[key] = value;
                    }
                    else
                    {
                        this.VariableDic.Add(key, value);
                    }
                }
            }
        }



    }
}
