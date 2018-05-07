using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerilogVisualizerTest
{
    class State
    {
        private string ta;
        private string name;

        public string Ta
        {
            get { return ta; }
            set { ta = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public State(string _ta, string _name)
        {
            ta = _ta;
            name = _name;
        }
    }
}
