using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerilogVisualizerTest
{
    public enum PortType { IN, OUT };

    public class Port
    {
        private PortType type;
        private string name;

        public PortType Type
        {
            get { return type; }
        }

        public string Name
        {
            get { return name; }
        }

        public Port(PortType t1, string name)
        {
            this.type = t1;
            this.name = name;
        }
    }
}
