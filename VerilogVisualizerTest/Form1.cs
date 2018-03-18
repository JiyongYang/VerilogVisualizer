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
using System.Xml.Linq;

namespace VerilogVisualizerTest
{
    public partial class Form1 : Form
    {
        public List<Module> Moduels;

        public Form1()
        {
            InitializeComponent();

            Moduels = new List<Module>();

            ReadXMLData();
        }

        private void ReadXMLData()
        {

            try
            {
                var root = XElement.Load("VerilogTestStructure2.xml");

                foreach (XElement node in root.Elements())
                {
                    Module temp = new Module(node.Attribute("name").Value);

                    //Console.WriteLine(node.Name.ToString());

                    parsingXmlData(node.Elements(), ref temp, 0);
                    Moduels.Add(temp);
                }

            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("[WARNING]----" + ex.ToString());
            }

            Console.WriteLine(" PAUSE ");
        }


        private void parsingXmlData(IEnumerable<XElement> nodes, ref Module temp, int depth)
        {
            foreach (var node in nodes)
            {
                switch(node.Name.ToString())
                {
                    case "VerilogPorts":
                        foreach (var ele in node.Elements("Port"))
                        {
                            //Console.WriteLine(ele.Attribute("name").Value);
                            Port pt = new Port(ele.Attribute("type").Value == "In" ? Type.IN : Type.OUT, ele.Attribute("name").Value);
                            temp.ports.Add(pt);
                        }
                        break;
                    case "VerilogConnections":
                        foreach (var ele in node.Elements("VerilogConnection"))
                        {
                            //Console.WriteLine(ele.Attribute("fPort").Value);
                            VerilogConnection conn = new VerilogConnection(ele.Attribute("from").Value,
                                    ele.Attribute("fPort").Value, ele.Attribute("to").Value, ele.Attribute("tPort").Value);
                            temp.verilogConnections.Add(conn);
                        }
                        break;
                    case "Modules":
                        foreach (XElement subNode in node.Elements())
                        {
                            //Console.WriteLine(subNode.Name.ToString());
                            Module subModule = new Module(subNode.Attribute("name").Value);
                            parsingXmlData(subNode.Elements(), ref subModule, depth + 1);
                            temp.verilogInstances.Add(subModule);
                        }
                        break;
                    default:
                        Console.WriteLine("[ERROR]----" + node.Name);
                        break;
                }

                //Console.WriteLine(node.Name.ToString());
            }
        }


        /*
        private Module parsingXmlData(XmlNodeList topNode, int XmlLevel)
        {
            Module temp = new Module("TEMP");

            foreach (XmlNode node in topNode)
            {
                switch (node.Name)
                {
                    case "Module":
                    case "ModuleInsatnce":
                        temp = new Module(node.Attributes["Name"].Value as string);
                        break;
                    case "VerilogPorts":
                        while ()
                        {
                            if (node.Name == "Port")
                            {
                                //Console.WriteLine(reader.NodeType.ToString());

                                Port pt = new Port(node.Attributes["type"].Value as string == "In" ? Type.IN : Type.OUT, node.Attributes["name"].Value as string);
                                temp.ports.Add(pt);

                                // Break when next isn't <Port> element
                                if (!(node.FirstChild.Name == "Port")) break;
                            }
                        }
                        break;
                    case "VerilogConnections":
                        while ()
                        {
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == "VerilogConnection")
                            {
                                VerilogConnection conn = new VerilogConnection(reader.GetAttribute("from"),
                                    reader.GetAttribute("fPort"), reader.GetAttribute("to"), reader.GetAttribute("tPort"));
                                temp.verilogConnections.Add(conn);

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
                            temp.verilogInstances.Add(rTemp);
                        XmlLevel -= 1;
                        return temp;
                    default:
                        Console.WriteLine(reader.Name);
                        break;
                }
            }

            return temp;
        }
        */
        /*
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
                                    temp.verilogConnections.Add(conn);

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
                                temp.verilogInstances.Add(rTemp);
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
        */
    }
}
