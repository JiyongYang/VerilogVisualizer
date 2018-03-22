using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerilogVisualizerTest
{
    public class Module
    {
        private string name { get; set; }
        private List<Port> ports;
        private List<VerilogConnection> verilogConnections;
        private List<Module> verilogInstances;

        public string Name
        {
            get { return name; }
        }

        public List<Port> Ports
        {
            get { return ports; }
        }

        public List<VerilogConnection> VerilogConnections
        {
            get { return verilogConnections; }
        }

        public List<Module> VerilogInstances
        {
            get { return verilogInstances; }
        }

        public Module(string name)
        {
            this.name = name;
            ports = new List<Port>();
            verilogConnections = new List<VerilogConnection>();
            verilogInstances = new List<Module>();
        }
    }
}
