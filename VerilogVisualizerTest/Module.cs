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
            set { name = value; }
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
            set { ports = value; }
        }

        public List<Coupling> Couplings
        {
            get { return couplings; }
            set { couplings = value; }
        }

        public List<Module> Instances
        {
            get { return instances; }
            set { instances = value; }
        }

        public Module(string _name)
        {
            id = "None";
            metaInfo = "None";
            name = _name;
            type = "None";
            ports = new List<Port>();
            couplings = new List<Coupling>();
            instances = new List<Module>();
        }

        public Module(string _name, string _metaInfo)
        {
            id = "None";
            metaInfo = _metaInfo;
            name = _name;
            type = "None";
            ports = new List<Port>();
            couplings = new List<Coupling>();
            instances = new List<Module>();
        }

        public object ShallowCopy()
        {
            return this.MemberwiseClone();
        }

        /*
        public Module DeepCopy()
        {
            Module other = (Module)this.MemberwiseClone();
            other.MetaInfo = string.Copy(metaInfo);
            other.Name = string.Copy(name);
            other.id = string.Copy(id);
            other.type = string.Copy(type);
            other.ports = 

            return other;
        }
        */
    }
}
