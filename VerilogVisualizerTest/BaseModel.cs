
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerilogVisualizerTest
{
    class BaseModel
    {
        private string name;
        private string type;
        private List<Port> ports;
        private List<State> states;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public List<Port> Ports
        {
            get { return ports; }
            set { ports = value; }
        }

        public List<State> States
        {
            get { return states; }
            set { states = value; }
        }

        public BaseModel(string _name, string _type)
        {
            name = _name;
            type = _type;
            ports = new List<Port>();
            states = new List<State>();
        }
    }
}
