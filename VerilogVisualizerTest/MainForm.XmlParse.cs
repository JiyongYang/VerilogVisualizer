using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Linq;

namespace VerilogVisualizerTest
{
    partial class MainForm
    {
        private void ReadXMLData()
        {
            try
            {
                var root = XElement.Load("VerilogTestStructure.xml");

                foreach (XElement node in root.Elements())
                {
                    if (node.Name == "TopModule")
                    {
                        Console.WriteLine("[NOTICE]---- TopModule called");
                        topModule = new Module(node.Attribute("name").Value);
                        ParsingXML(node.Elements(), ref topModule, 0);
                    }
                    else if (node.Name == "ModulePool")
                    {
                        Console.WriteLine("[NOTICE]---- ModulePool called");
                        ParsingXML(node.Elements(), 0);
                    }
                    else
                    {
                        Console.WriteLine("[WARNING]---- should not be called");
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("[WARNING]----" + ex.ToString());
            }
            Console.WriteLine("[NOTICE]---- successfully load XML file");

            try
            {
                var root = XElement.Load("DEVS_ModelBase.xml");

                foreach (XElement node in root.Elements())
                {
                    if (node.Name == "BaseModel")
                    {
                        Console.WriteLine("[NOTICE]---- BaseModel called");
                        BaseModel temp_baseModel = new BaseModel(node.Attribute("name").Value, node.Attribute("type").Value);
                        ParsingBaseModelXML(node.Elements(), ref temp_baseModel, 0);

                        // [!@#$] Need to check key existence
                        BaseModels.Add(temp_baseModel.Name, temp_baseModel);
                    }
                    else
                    {
                        Console.WriteLine("[WARNING]---- should not be called");
                    }
                }

            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("[WARNING]----" + ex.ToString());
            }
            Console.WriteLine("[NOTICE]---- successfully load XML file");
        }

        private void ParsingBaseModelXML(IEnumerable<XElement> elements, ref BaseModel bm, int depth)
        {
            foreach (var e in elements)
            {
                switch (e.Name.ToString())
                {
                    case "Port":
                        Port pt = new Port(e.Attribute("type").Value == "In" ? PortType.IN : PortType.OUT, e.Value);
                        bm.Ports.Add(pt);
                        break;
                    case "State":
                        State st = new State(e.Attribute("ta").Value, e.Value);
                        bm.States.Add(st);
                        break;
                    default:
                        Console.WriteLine("[ERROR]----" + e.Name);
                        break;
                }
            }
        }

        private void ParsingXML(IEnumerable<XElement> elements, ref Module mod, int depth)
        {
            int idOffset = 0;
            mod.Id = depth.ToString();
            foreach (var e in elements)
            {
                switch (e.Name.ToString())
                {
                    case "Port":
                        Port pt = new Port(e.Attribute("type").Value == "In" ? PortType.IN : PortType.OUT, e.Value);
                        mod.Ports.Add(pt);
                        break;
                    case "Instance":
                        Module temp = new Module(e.Attribute("name").Value, "SI");
                        temp.Id = makeId(depth, idOffset);
                        ParsingXML_readInstance(e.Elements(), ref temp, depth + 1, 0);
                        mod.Instances.Add(temp);
                        idOffset += 1;
                        break;
                    default:
                        Console.WriteLine("[ERROR]----" + e.Name);
                        break;
                }
            }
        }

        private void ParsingXML(IEnumerable<XElement> elements, int depth)
        {
            foreach (var ele in elements)
            {
                if (ele.Name.ToString() == "Module")
                {
                    Module tModule = new Module(ele.Attribute("name").Value, "S");
                    foreach (var e in ele.Elements("Port"))
                    {
                        Port pt = new Port(e.Attribute("type").Value == "In" ? PortType.IN : PortType.OUT, e.Value);
                        tModule.Ports.Add(pt);
                    }

                    foreach (var e in ele.Elements("Instance"))
                    {
                        Module temp = new Module(e.Attribute("name").Value, "SI");
                        ParsingXML_readInstance(e.Elements(), ref temp, depth + 1, 1);
                        tModule.Instances.Add(temp);
                    }

                    ModulePool.Add(ele.Attribute("name").Value, tModule);
                }
            }
        }

        private void ParsingXML_readInstance(IEnumerable<XElement> elements, ref Module mod, int depth, int flg)
        {
            int idOffset = 0;
            if (flg == 0)
                mod.Id = depth.ToString();

            foreach (var ele in elements)
            {
                switch (ele.Name.ToString())
                {
                    case "Type":
                        mod.Type = ele.Value;
                        break;
                    case "Coupling":
                        Coupling cp = new Coupling(ele.Attribute("from").Value,
                                    ele.Attribute("fPort").Value, ele.Attribute("to").Value, ele.Attribute("tPort").Value);
                        mod.Couplings.Add(cp);
                        break;
                    case "Instance":
                        Module temp = new Module(ele.Attribute("name").Value, "SI");
                        if (flg == 0)
                            temp.Id = makeId(depth, idOffset);
                        ParsingXML_readInstance(ele.Elements(), ref temp, depth + 1, flg);
                        mod.Instances.Add(temp);
                        idOffset += 1;
                        break;
                    default:
                        Console.WriteLine("[ERROR]----" + ele.Name);
                        break;
                }
            }
        }

        private void parsingXmlData(IEnumerable<XElement> nodes, ref Module temp, int depth)
        {
            foreach (var node in nodes)
            {
                switch (node.Name.ToString())
                {
                    case "Port":
                        Port pt = new Port(node.Attribute("type").Value == "In" ? PortType.IN : PortType.OUT, node.Value);
                        temp.Ports.Add(pt);
                        break;
                    case "Instance":
                        string type = node.Element("type").Value;
                        temp.Type = type;

                        foreach (var ele in node.Elements("coupling"))
                        {
                            //Console.WriteLine(ele.Attribute("fPort").Value);
                            Coupling cp = new Coupling(ele.Attribute("from").Value,
                                    ele.Attribute("fPort").Value, ele.Attribute("to").Value, ele.Attribute("tPort").Value);
                            temp.Couplings.Add(cp);
                        }
                        break;
                    case "Modules":
                        foreach (XElement subNode in node.Elements())
                        {
                            //Console.WriteLine(subNode.Name.ToString());
                            Module subModule = new Module(subNode.Attribute("name").Value);
                            parsingXmlData(subNode.Elements(), ref subModule, depth + 1);
                            temp.Instances.Add(subModule);
                        }
                        break;
                    default:
                        Console.WriteLine("[ERROR]----" + node.Name);
                        break;
                }

                //Console.WriteLine(node.Name.ToString());
            }
        }
    }
}
