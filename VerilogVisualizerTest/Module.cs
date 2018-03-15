using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerilogVisualizerTest
{
    public enum Type { IN, OUT };

    public struct Port
    {
        public Type type;
        public string name;

        public Port(Type t1, string name)
        {
            this.type = t1;
            this.name = name;
        }
    }

    public struct VerilogConnection
    {
        public string from;
        public string fPort;
        public string to;
        public string tPort;

        public VerilogConnection(string s1, string s2, string s3, string s4)
        {
            this.from = s1;
            this.fPort = s2;
            this.to = s3;
            this.tPort = s4;
        }
    };

    public class Module
    {
        public string name { get; set; }
        public List<Port> ports;
        public List<VerilogConnection> verilogConnection;
        public List<Module> verilogInstance;

        public Module(string name)
        {
            this.name = name;
            ports = new List<Port>();
            verilogConnection = new List<VerilogConnection>();
            verilogInstance = new List<Module>();
        }

        /*
        public void addPorts(string port)
        {
            ports.Add(port);
        }

        public void addVerilogConnection(VerilogConnection conn)
        {
            verilogConnection.Add(conn);
        }

        public void addVerilogInstance(Module mod)
        {
            verilogInstance.Add(mod);
        }
        */

    }
}
