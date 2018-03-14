using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VerilogVisualizerTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ReadXMLData();
        }

        private void ReadXMLData()
        {
            using (XmlReader reader = XmlReader.Create("VerilogTestStructure.xml"))

            /*
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("VerilogTestStructure.xml");

            XmlElement root = xdoc.DocumentElement;

            XmlNodeList nodes = root.ChildNodes;

            foreach (XmlNode node in nodes)
            {
                Console.WriteLine(node.ToString());
            }
            */
        }
    }    
}
