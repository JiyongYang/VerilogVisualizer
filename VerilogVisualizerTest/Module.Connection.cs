using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerilogVisualizerTest
{
    public class VerilogConnection
    {
        private string from;
        private string to;
        private string fPort;
        private string tPort;

        public string From
        {
            get { return from; }
        }

        public string To
        {
            get { return to; }
        }

        public string FPort
        {
            get { return fPort; }
        }

        public string TPort
        {
            get { return tPort; }
        }

        public VerilogConnection(string s1, string s2, string s3, string s4)
        {
            this.from = s1;
            this.fPort = s2;
            this.to = s3;
            this.tPort = s4;
        }
    };
}
