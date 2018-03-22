using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerilogVisualizerTest
{
    public enum Type { IN, OUT };

    public class Port
    {
        private Type type;
        private string name;

        public Type Type
        {
            get { return type; }
        }

        public string Name
        {
            get { return name; }
        }

        public Port(Type t1, string name)
        {
            this.type = t1;
            this.name = name;
        }
    }
}
