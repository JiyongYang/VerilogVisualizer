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
        public List<Module> root;

        public Form1()
        {
            InitializeComponent();

            root = new List<Module>();

            ReadXMLData();
        }

        private void ReadXMLData()
        {
            int XmlLevel = 0;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("VerilogTestStructure.xml");


            }
            catch(InvalidOperationException ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine(" PAUSE ");
        }

        private Module parsingXmlData(XmlReader reader, int XmlLevel)
        {
            Module temp = new Module("Error");
            while (reader.Read())
            {
                string indent = "";
                for (int i = 0; i < reader.Depth*4; i++)
                {
                    indent += " ";
                }

                Console.WriteLine(indent + reader.Name + "\t\t\t" + reader.NodeType.ToString());

                if (reader.NodeType == XmlNodeType.Element)
                {
                    //Console.WriteLine(reader.Depth + " " + reader.Name);

                    switch (reader.Name)
                    {
                        case "Module":
                        case "ModuleInsatnce":
                            temp = new Module(reader.GetAttribute("name"));
                            break;
                        case "VerilogPorts":
                            while (reader.ReadToNextSibling("Port"))
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Port")
                                {
                                    //Console.WriteLine(reader.NodeType.ToString());

                                    Port pt = new Port(reader.GetAttribute("type") == "In" ? Type.IN : Type.OUT, reader.GetAttribute("name"));
                                    temp.ports.Add(pt);

                                    // Break when next isn't <Port> element
                                    //if (!reader.ReadToNextSibling("Port")) break;
                                }
                            }
                            break;
                        case "VerilogConnections":
                            while (reader.ReadToNextSibling("VerilogConnection"))
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "VerilogConnection")
                                {
                                    VerilogConnection conn = new VerilogConnection(reader.GetAttribute("from"),
                                        reader.GetAttribute("fPort"), reader.GetAttribute("to"), reader.GetAttribute("tPort"));
                                    temp.verilogConnection.Add(conn);

                                    //if (!reader.ReadToNextSibling("VerilogConnection")) break;
                                }
                                
                            }
                            break;
                        case "VerilogInstance":
                            XmlLevel += 1;
                            Module rTemp = parsingXmlData(reader, XmlLevel);
                            if (XmlLevel == 1)
                                root.Add(temp);
                            else
                                temp.verilogInstance.Add(rTemp);
                            XmlLevel -= 1;
                            return temp;
                        default:
                            Console.WriteLine(reader.Name);
                            break;
                    }
                    //Console.WriteLine("Pause");
                }
            }

            return temp;
        }

        private Module parsingXmlData(XmlNode node, int XmlLevel)
        {
            return new Module("test");
        }
    }
}
