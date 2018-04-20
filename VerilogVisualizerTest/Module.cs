using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerilogVisualizerTest
{
    public class Module
    {
        private string id;
        private string metaInfo;
        private string name;
        private string type;
        private List<Port> ports;
        private List<Coupling> couplings;
        private List<Module> instances;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string MetaInfo
        {
            get { return metaInfo; }
            set { metaInfo = value; }
        }

        public List<Port> Ports
        {
            get { return ports; }
        }

        public List<Coupling> Couplings
        {
            get { return couplings; }
        }

        public List<Module> Instances
        {
            get { return instances; }
        }

        public Module(string name)
        {
            this.metaInfo = "None";
            this.name = name;
            type = "None";
            ports = new List<Port>();
            couplings = new List<Coupling>();
            instances = new List<Module>();
        }

        public Module(string name, string metaInfo)
        {
            this.metaInfo = metaInfo;
            this.name = name;
            type = "None";
            ports = new List<Port>();
            couplings = new List<Coupling>();
            instances = new List<Module>();
        }
    }
}
