using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nevron.Diagram;
using Nevron.Diagram.Shapes;

namespace VerilogVisualizerTest
{
    class UDNGroup : NGroup
    {
        private string udFullName;
        private string udUuid;
        private string udId;
        private List<Port> udPorts;
        
        public string UDFullName
        {
            get { return udFullName; }
            set { udFullName = value; }
        }

        public string UDUuid
        {
            get { return udUuid; }
            set { udUuid = value; }
        }

        public string UDUdId
        {
            get { return udId; }
            set { udId = value; }
        }

        public List<Port> UDPorts
        {
            get { return udPorts; }
            set { udPorts = value; }
        }
    }
}
