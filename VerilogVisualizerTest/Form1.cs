﻿using System;
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
using Nevron.GraphicsCore;

using Nevron.Diagram;
using Nevron.Diagram.Shapes;
using Nevron.Diagram.Layout;
using Nevron.Diagram.Filters;
using Nevron.Diagram.WinForm;
using Nevron.Filters;
using Nevron.Dom;
using Nevron.Diagram.DataStructures;


namespace VerilogVisualizerTest
{
    public partial class Form1 : Form
    {
        public Module topModule;
        public Dictionary<string, Module> ModulePool;
        //private NOrthogonalGraphLayout m_Layout;

        public Form1()
        {
            InitializeComponent();

            topModule = new Module("TopModule");
            ModulePool = new Dictionary<string, Module>();

            ReadXMLData();
            Form_Update();
        }

        private void Form_Update()
        {
            TreeList_Update();
        }

        private void TreeList_Update()
        {
            TreeNode topNode = new TreeNode(topModule.Name);

            for (int i = 0; i < topModule.Instances.Count; i++)
            {
                topNode.Nodes.Add(topModule.Instances[i].Id, topModule.Instances[i].Name);
            }

            treeView1.Nodes.Add(topNode);

            // 모든 트리 노드를 보여준다
            treeView1.ExpandAll();
        }

        private void TreeList_AddInstance(ref TreeNode node, Module mod)
        {

        }

        private string makeId(int depth, int offset)
        {
            return depth.ToString() + "_" + offset.ToString();
        }

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


                    /*
                    Module temp = new Module(node.Attribute("name").Value);

                    parsingXmlData(node.Elements(), ref temp, 0);
                    Modules.Add(temp);
                    */
                }

            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("[WARNING]----" + ex.ToString());
            }
            Console.WriteLine("[NOTICE]---- successfully load XML file");
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

        private void Form1_Load(object sender, EventArgs e)
        {

            // begin view init
            nDrawingView1.BeginInit();

            // display the document in the view
            nDrawingView1.Document = document;

            // show ports
            nDrawingView1.GlobalVisibility.ShowPorts = true;

            // hide the grid
            nDrawingView1.Grid.Visible = false;

            // fit the document in the viewport 
            nDrawingView1.ViewLayout = ViewLayout.Normal;

            // apply padding to the document bounds
            nDrawingView1.DocumentPadding = new Nevron.Diagram.NMargins(10);

            // init document
            document.BeginInit();

            // modify the connectors style sheet
            NStyleSheet styleSheet = (document.StyleSheets.GetChildByName(NDR.NameConnectorsStyleSheet, -1) as NStyleSheet);

            NTextStyle textStyle = new NTextStyle();
            textStyle.BackplaneStyle.Visible = false;
            textStyle.BackplaneStyle.StandardFrameStyle.InnerBorderWidth = new NLength(0);
            styleSheet.Style.TextStyle = textStyle;

            styleSheet.Style.StrokeStyle = new NStrokeStyle(1, Color.Black);
            styleSheet.Style.StartArrowheadStyle.StrokeStyle = new NStrokeStyle(1, Color.Black);
            styleSheet.Style.EndArrowheadStyle.StrokeStyle = new NStrokeStyle(1, Color.Black);

            // create a stylesheet for the 2D Shapes
            styleSheet = new NStyleSheet("SHAPE2D");
            styleSheet.Style.FillStyle = new NColorFillStyle(Color.PapayaWhip);
            document.StyleSheets.AddChild(styleSheet);

            // create a stylesheet for the arrows, which inherits from the connectors stylesheet
            styleSheet = new NStyleSheet("ARROW", NDR.NameConnectorsStyleSheet);

            textStyle = new NTextStyle();
            textStyle.FontStyle.InitFromFont(new Font("Arial", 8));
            styleSheet.Style.TextStyle = textStyle;

            document.StyleSheets.AddChild(styleSheet);

            // init form fields
            //m_Layout = new NOrthogonalGraphLayout();
            //propertyGrid1.SelectedObject = m_Layout;

            InitDocument();

            // end nDrawingDocument1 init
            document.EndInit();

            //end view init
            nDrawingView1.EndInit();



        }



        /*
        private void CreateDiagram()
        {
            float width = 70;
            float height = 70;


            NShape shape1 = CreateInstance(0, 0, width, height, "Input1");
            shape1.Location = new NPointF(100, 300);
            NShape shape2 = CreateInstance(0, 0, width, height, "Input2");
            shape2.Location = new NPointF(100, 600);

            NShape shape3 = CreateInstance(0, 0, width, height, "Sum1");
            shape3.Location = new NPointF(300, 100);
            NShape shape4 = CreateInstance(0, 0, width, height, "Sum2");
            shape4.Location = new NPointF(400, 100);
            NShape shape5 = CreateInstance(0, 0, width, height, "Sum3");
            shape5.Location = new NPointF(500, 100);
            NShape shape6 = CreateInstance(0, 0, width, height, "Sum4");
            shape6.Location = new NPointF(600, 100);

            NShape shape7 = CreateInstance(0, 0, width, height, "Output1");
            shape7.Location = new NPointF(800, 450);

            document.ActiveLayer.AddChild(shape1);
            document.ActiveLayer.AddChild(shape2);
            document.ActiveLayer.AddChild(shape3);
            document.ActiveLayer.AddChild(shape4);
            document.ActiveLayer.AddChild(shape5);
            document.ActiveLayer.AddChild(shape6);
            document.ActiveLayer.AddChild(shape7);


            NStep3Connector c1 = new NStep3Connector(false, 50, 0, true);
            c1.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c1.Text = "c1";
            document.ActiveLayer.AddChild(c1);
            c1.StartPlug.Connect(shape1.Ports.GetChildByName("OUT", 0) as NPort);
            c1.EndPlug.Connect(shape3.Ports.GetChildByName("IN1", 0) as NPort);

            NStep3Connector c2 = new NStep3Connector(false, 90, 0, true);
            c2.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c2.Text = "c2";
            document.ActiveLayer.AddChild(c2);
            c2.StartPlug.Connect(shape1.Ports.GetChildByName("OUT", 0) as NPort);
            c2.EndPlug.Connect(shape4.Ports.GetChildByName("IN1", 0) as NPort);

            NStep3Connector c3 = new NStep3Connector(false, 90, 0, true);
            c3.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c3.Text = "c3";
            document.ActiveLayer.AddChild(c3);
            c3.StartPlug.Connect(shape2.Ports.GetChildByName("OUT", 0) as NPort);
            c3.EndPlug.Connect(shape4.Ports.GetChildByName("IN1", 0) as NPort);

            NStep3Connector c4 = new NStep3Connector(false, 90, 0, true);
            c4.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c4.Text = "c4";
            document.ActiveLayer.AddChild(c4);
            c4.StartPlug.Connect(shape1.Ports.GetChildByName("IN1", 0) as NPort);
            c4.EndPlug.Connect(shape4.Ports.GetChildByName("IN1", 0) as NPort);



        }
        */

        /*
        private NShape CreateInstance(float x, float y, float width, float height, string name)
        {


            NShape temp = new NRectangleShape(x, y, width, height);
            temp.Name = name;
            temp.Text = name;

            temp.CreateShapeElements(ShapeElementsMask.Ports);

            NDynamicPort port = new NDynamicPort(new NContentAlignment(-50, -20), DynamicPortGlueMode.GlueToContour);
            port.Name = "IN1";
            temp.Ports.AddChild(port);

            NDynamicPort port1 = new NDynamicPort(new NContentAlignment(-50, 0), DynamicPortGlueMode.GlueToContour);
            port1.Name = "IN2";
            temp.Ports.AddChild(port1);

            NDynamicPort port2 = new NDynamicPort(new NContentAlignment(-50, 20), DynamicPortGlueMode.GlueToContour);
            port2.Name = "CI";
            temp.Ports.AddChild(port2);

            NDynamicPort port3 = new NDynamicPort(new NContentAlignment(50, -20), DynamicPortGlueMode.GlueToContour);
            port3.Name = "OUT";
            temp.Ports.AddChild(port3);

            NDynamicPort port4 = new NDynamicPort(new NContentAlignment(50, 0), DynamicPortGlueMode.GlueToContour);
            port4.Name = "CO";
            temp.Ports.AddChild(port4);

            return temp;
        }
        */



        private void InitDocument()
        {
            NStyleSheet styleSheet = new NStyleSheet("CustomConnectors");
            //styleSheet.Style.StartArrowheadStyle = new NArrowheadStyle(ArrowheadShape.Circle, "CustomConnectorStart", new NSizeL(6, 6), new NColorFillStyle(Color.FromArgb(247, 150, 56)), new NStrokeStyle(1, Color.FromArgb(68, 90, 108)));
            styleSheet.Style.EndArrowheadStyle = new NArrowheadStyle(ArrowheadShape.Arrow, "CustomConnectorStart", new NSizeL(6, 6), new NColorFillStyle(Color.FromArgb(247, 150, 56)), new NStrokeStyle(1, Color.FromArgb(68, 90, 108)));
            styleSheet.Style.StrokeStyle = new NStrokeStyle(1, Color.FromArgb(68, 90, 108));
            document.StyleSheets.AddChild(styleSheet);


            List<NGroup> instanceList = new List<NGroup>();

            NGroup Inport1 = CreateGlobalPort("Input1", PortType.IN);
            NGroup Inport2 = CreateGlobalPort("Input2", PortType.IN);
            NGroup Outport1 = CreateGlobalPort("Output1asdfasdfasdf", PortType.OUT);

            Inport1.Location = new NPointF(50, 200);
            Inport2.Location = new NPointF(50, 300);
            Outport1.Location = new NPointF(900, 250);

            document.ActiveLayer.AddChild(Inport1);
            document.ActiveLayer.AddChild(Inport2);
            document.ActiveLayer.AddChild(Outport1);

            NGroup shape = CreateInstance("Sum1", randomPortGen());
            //shape.Location = new NPointF(200, 100);
            document.ActiveLayer.AddChild(shape);
            instanceList.Add(shape);

            NGroup shape1 = CreateInstance("Sum2", randomPortGen());
            //shape1.Location = new NPointF(200, 300);
            document.ActiveLayer.AddChild(shape1);
            instanceList.Add(shape1);

            NGroup shape2 = CreateInstance("Sum3", randomPortGen());
            //shape2.Location = new NPointF(500, 100);
            document.ActiveLayer.AddChild(shape2);
            instanceList.Add(shape2);

            NGroup shape3 = CreateInstance("Sum4", randomPortGen());
            //shape3.Location = new NPointF(500, 300);
            document.ActiveLayer.AddChild(shape3);
            instanceList.Add(shape3);

            setInstancesPos(instanceList);
            /*
            NStep3Connector c1 = new NStep3Connector(false, 50, 0, true);
            c1.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c1.Text = "c1";
            document.ActiveLayer.AddChild(c1);
            c1.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OutPt0", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            c1.EndPlug.Connect(((NShape)(shape1.Shapes.GetChildByName("InPt0", 0))).Ports.GetChildByName("IN", 0) as NPort);

            NStep3Connector c2 = new NStep3Connector(false, 80, 0, true);
            c2.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c2.Text = "c2";
            document.ActiveLayer.AddChild(c2);
            c2.StartPlug.Connect(((NShape)(shape1.Shapes.GetChildByName("OutPt0", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            c2.EndPlug.Connect(((NShape)(shape2.Shapes.GetChildByName("InPt1", 0))).Ports.GetChildByName("IN", 0) as NPort);

            NStep3Connector c3 = new NStep3Connector(false, 90, 0, true);
            c3.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c3.Text = "c3";
            document.ActiveLayer.AddChild(c3);
            c3.StartPlug.Connect(((NShape)(shape3.Shapes.GetChildByName("OutPt0", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            c3.EndPlug.Connect(((NShape)(shape2.Shapes.GetChildByName("InPt0", 0))).Ports.GetChildByName("IN", 0) as NPort);

            NStep3Connector c4 = new NStep3Connector(false, 50, 0, true);
            c4.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c4.Text = "c4";
            document.ActiveLayer.AddChild(c4);
            c4.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OutPt1", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            c4.EndPlug.Connect(((NShape)(shape2.Shapes.GetChildByName("InPt0", 0))).Ports.GetChildByName("IN", 0) as NPort);

            NStep3Connector c5 = new NStep3Connector(false, 50, 0, true);
            c5.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c5.Text = "c5";
            document.ActiveLayer.AddChild(c5);
            c5.StartPlug.Connect(((NShape)(Inport1.Shapes.GetChildByName("Input1", 0))).Ports.GetChildByName("Input1", 0) as NPort);
            c5.EndPlug.Connect(((NShape)(shape.Shapes.GetChildByName("InPt1", 0))).Ports.GetChildByName("IN", 0) as NPort);
            //c5.StartPlug.Connect(Inport1.Ports.GetChildByName("Input1", 0) as NPort);

            NStep3Connector c6 = new NStep3Connector(false, 50, 0, true);
            c6.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c6.Text = "c6";
            document.ActiveLayer.AddChild(c6);
            c6.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OutPt1", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            c6.EndPlug.Connect(((NShape)(shape3.Shapes.GetChildByName("InPt0", 0))).Ports.GetChildByName("IN", 0) as NPort);

            NStep3Connector c7 = new NStep3Connector(false, 20, 0, true);
            c7.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c7.Text = "c7";
            document.ActiveLayer.AddChild(c7);
            c7.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OutPt1", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            c7.EndPlug.Connect(((NShape)(Outport1.Shapes.GetChildByName("Output1asdfasdfasdf", 0))).Ports.GetChildByName("Output1asdfasdfasdf", 0) as NPort);
            //c7.EndPlug.Connect(Outport1.Ports.GetChildByName("Output1asdfasdfasdf", 0) as NPort);
            */
            NRoutableConnector routableConnector = new NRoutableConnector(RoutableConnectorType.DynamicHV, RerouteAutomatically.Always);
            routableConnector.StyleSheetName = NDR.NameConnectorsStyleSheet;
            routableConnector.Style.StrokeStyle = new NStrokeStyle(1, Color.Blue);
            document.ActiveLayer.AddChild(routableConnector);

            routableConnector.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OutPt1", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            routableConnector.EndPlug.Connect(((NShape)(Outport1.Shapes.GetChildByName("Output1asdfasdfasdf", 0))).Ports.GetChildByName("Output1asdfasdfasdf", 0) as NPort);
            routableConnector.Reroute();

            routableConnector = new NRoutableConnector(RoutableConnectorType.DynamicHV, RerouteAutomatically.Always);
            routableConnector.StyleSheetName = NDR.NameConnectorsStyleSheet;
            routableConnector.Style.StrokeStyle = new NStrokeStyle(1, Color.Blue);
            document.ActiveLayer.AddChild(routableConnector);

            routableConnector.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OutPt1", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            routableConnector.EndPlug.Connect(((NShape)(shape3.Shapes.GetChildByName("InPt0", 0))).Ports.GetChildByName("IN", 0) as NPort);
            routableConnector.Reroute();

            routableConnector = new NRoutableConnector(RoutableConnectorType.DynamicHV, RerouteAutomatically.Always);
            routableConnector.StyleSheetName = NDR.NameConnectorsStyleSheet;
            routableConnector.Style.StrokeStyle = new NStrokeStyle(1, Color.Blue);
            document.ActiveLayer.AddChild(routableConnector);

            routableConnector.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OutPt1", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            routableConnector.EndPlug.Connect(((NShape)(shape2.Shapes.GetChildByName("InPt0", 0))).Ports.GetChildByName("IN", 0) as NPort);
            routableConnector.Reroute();

            routableConnector = new NRoutableConnector(RoutableConnectorType.DynamicHV, RerouteAutomatically.Always);
            routableConnector.StyleSheetName = NDR.NameConnectorsStyleSheet;
            routableConnector.Style.StrokeStyle = new NStrokeStyle(1, Color.Blue);
            document.ActiveLayer.AddChild(routableConnector);

            routableConnector.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OutPt1", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            routableConnector.EndPlug.Connect(((NShape)(shape2.Shapes.GetChildByName("InPt1", 0))).Ports.GetChildByName("IN", 0) as NPort);
            routableConnector.Reroute();

            routableConnector = new NRoutableConnector(RoutableConnectorType.DynamicHV, RerouteAutomatically.Always);
            routableConnector.StyleSheetName = NDR.NameConnectorsStyleSheet;
            routableConnector.Style.StrokeStyle = new NStrokeStyle(1, Color.Blue);
            document.ActiveLayer.AddChild(routableConnector);

            routableConnector.StartPlug.Connect(((NShape)(Inport1.Shapes.GetChildByName("Input1", 0))).Ports.GetChildByName("Input1", 0) as NPort);
            routableConnector.EndPlug.Connect(((NShape)(shape.Shapes.GetChildByName("InPt1", 0))).Ports.GetChildByName("IN", 0) as NPort);
            routableConnector.Reroute();

            routableConnector = new NRoutableConnector(RoutableConnectorType.DynamicHV, RerouteAutomatically.Always);
            routableConnector.StyleSheetName = NDR.NameConnectorsStyleSheet;
            routableConnector.Style.StrokeStyle = new NStrokeStyle(1, Color.Blue);
            document.ActiveLayer.AddChild(routableConnector);

            routableConnector.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OutPt1", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            routableConnector.EndPlug.Connect(((NShape)(shape2.Shapes.GetChildByName("InPt0", 0))).Ports.GetChildByName("IN", 0) as NPort);
            routableConnector.Reroute();

            routableConnector = new NRoutableConnector(RoutableConnectorType.DynamicHV, RerouteAutomatically.Always);
            routableConnector.StyleSheetName = NDR.NameConnectorsStyleSheet;
            routableConnector.Style.StrokeStyle = new NStrokeStyle(1, Color.Blue);
            document.ActiveLayer.AddChild(routableConnector);

            routableConnector.StartPlug.Connect(((NShape)(shape3.Shapes.GetChildByName("OutPt0", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            routableConnector.EndPlug.Connect(((NShape)(shape2.Shapes.GetChildByName("InPt0", 0))).Ports.GetChildByName("IN", 0) as NPort);
            routableConnector.Reroute();

            routableConnector = new NRoutableConnector(RoutableConnectorType.DynamicHV, RerouteAutomatically.Always);
            routableConnector.StyleSheetName = NDR.NameConnectorsStyleSheet;
            routableConnector.Style.StrokeStyle = new NStrokeStyle(1, Color.Blue);
            document.ActiveLayer.AddChild(routableConnector);

            routableConnector.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OutPt0", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            routableConnector.EndPlug.Connect(((NShape)(shape1.Shapes.GetChildByName("InPt0", 0))).Ports.GetChildByName("IN", 0) as NPort);
            routableConnector.Reroute();

            document.SizeToContent();
        }

        private void setInstancesPos(List<NGroup> gList)
        {
            int xInsPos = 200;
            int yInsPos = 100;

            for (int i = 0; i < gList.Count; i++)
            {
                gList[i].Location = new NPointF(xInsPos, yInsPos);
                xInsPos += (int)gList[i].Bounds.Width + 100;
                if ((i + 1) % 3 == 0 && i != 0)
                {
                    yInsPos += yInsPos + (int)gList[i - 2].Bounds.Height + 100;
                    xInsPos = 200;
                }
            }
        }

        private void InitDocument2()
        {
            double scale = 1.5;

            NGroup shape = CreateInstance2(0, 0, 0, 0, "Sum1", 3, 2);
            shape.Location = new NPointF(200, 100);
            shape.Width = shape.Width * (float)scale;
            shape.Height = shape.Height * (float)scale;

            NGroup shape2 = CreateInstance2(0, 0, 0, 0, "Sum2", 2, 3);
            shape2.Location = new NPointF(500, 100);
            shape2.Width = shape2.Width * (float)scale;
            shape2.Height = shape2.Height * (float)scale;

            NGroup shape3 = CreateInstance2(0, 0, 0, 0, "Sum3", 2, 2);
            shape3.Location = new NPointF(200, 500);
            shape3.Width = shape3.Width * (float)scale;
            shape3.Height = shape3.Height * (float)scale;

            NShape Inport1 = CreateGlobalPort("Input1", PortType.IN);
            NShape Inport2 = CreateGlobalPort("Input2", PortType.IN);
            NShape Outport1 = CreateGlobalPort("Output1asdfasdfasdf", PortType.OUT);

            document.ActiveLayer.AddChild(shape);
            document.ActiveLayer.AddChild(shape2);
            document.ActiveLayer.AddChild(shape3);
            document.ActiveLayer.AddChild(Inport1);
            document.ActiveLayer.AddChild(Inport2);
            document.ActiveLayer.AddChild(Outport1);

            Inport1.Location = new NPointF(50, 300);
            Inport2.Location = new NPointF(50, 400);
            Outport1.Location = new NPointF(700, 350);





            /*NStep3Connector c1 = new NStep3Connector(false, 50, 0, true);
            c1.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c1.Text = "c1";
            document.ActiveLayer.AddChild(c1);
            c1.StartPlug.Connect(Inport1.Ports.GetChildByName("Input1", 0) as NPort);
            c1.EndPlug.Connect(((NShape)(shape.Shapes.GetChildByName("IN1", 0))).Ports.GetChildByName("IN", 0) as NPort);

            NStep3Connector c2 = new NStep3Connector(false, 90, 0, true);
            c2.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c2.Text = "c2";
            document.ActiveLayer.AddChild(c2);
            c2.StartPlug.Connect(Inport1.Ports.GetChildByName("Input1", 0) as NPort);
            c2.EndPlug.Connect(((NShape)(shape2.Shapes.GetChildByName("IN1", 0))).Ports.GetChildByName("IN", 0) as NPort);

            NStep3Connector c3 = new NStep3Connector(false, 70, 0, true);
            c3.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c3.Text = "c3";
            document.ActiveLayer.AddChild(c3);
            c3.StartPlug.Connect(Inport2.Ports.GetChildByName("Input2", 0) as NPort);
            c3.EndPlug.Connect(((NShape)(shape.Shapes.GetChildByName("IN2", 0))).Ports.GetChildByName("IN", 0) as NPort);

            NStep3Connector c4 = new NStep3Connector(false, 70, 0, true);
            c4.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c4.Text = "c4";
            document.ActiveLayer.AddChild(c4);
            c4.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OUT0", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            c4.EndPlug.Connect(((NShape)(shape2.Shapes.GetChildByName("IN0", 0))).Ports.GetChildByName("IN", 0) as NPort);

            NStep3Connector c5 = new NStep3Connector(false, 20, 0, true);
            c5.StyleSheetName = NDR.NameConnectorsStyleSheet;
            c5.Text = "c5";
            document.ActiveLayer.AddChild(c5);
            c5.StartPlug.Connect(((NShape)(shape.Shapes.GetChildByName("OUT0", 0))).Ports.GetChildByName("OUT", 0) as NPort);
            c5.EndPlug.Connect(Outport1.Ports.GetChildByName("Output1", 0) as NPort);*/


            document.SizeToContent();
        }

        private NGroup CreateInstance(string name, List<Port> ports)
        {
            int instanceWidth = 50;
            int instanceHeight = 50;

            int InputMaxSize = 10;
            int InputCnt = 0;
            int OutputMaxSize = 10;
            int OutputCnt = 0;

            int offsetWidth = 5;
            int offsetHeight = 30;
            int widthPadding = 10;
            int heightPadding = 10;

            int textWidth = 30;
            int textHeight = 15;

            int curInPtCnt = 0;
            int curOutPtCnt = 0;

            NGroup group = new NGroup();

            // find max input/output port size
            for (int i = 0; i < ports.Count; i++)
            {
                if (ports[i].Type == PortType.IN)
                {
                    InputCnt += 1;
                    if (InputMaxSize < ports[i].Name.Length)
                        InputMaxSize = ports[i].Name.Length;
                }
                else
                {
                    OutputCnt += 1;
                    if (OutputMaxSize < ports[i].Name.Length)
                        OutputMaxSize = ports[i].Name.Length;
                }
            }

            instanceWidth = (InputMaxSize * offsetWidth) + (OutputMaxSize * offsetWidth) + widthPadding;
            instanceHeight = (InputCnt > OutputCnt ? InputCnt : OutputCnt) * offsetHeight + heightPadding;

            textWidth = instanceWidth;

            // Add Instance
            NRectangleShape node = new NRectangleShape(0, 0, (int)instanceWidth, (int)instanceHeight);
            node.Name = name;
            group.Shapes.AddChild(node);

            NTextShape nodeName = new NTextShape(name, 0, -15, textWidth, textHeight);
            nodeName.Style.TextStyle = new NTextStyle();
            nodeName.Style.TextStyle.FontStyle = new NFontStyle(new Font("Arial", 7));
            group.Shapes.AddChild(nodeName);

            // Add Port

            for (int i = 0; i < ports.Count; i++)
            {
                NShape port = createPort(ports[i].Name, ports[i].Type);
                group.Shapes.AddChild(port);
                if (ports[i].Type == PortType.IN)
                {
                    curInPtCnt += 1;
                    port.Location = new NPointF(-port.Bounds.Width / 2, (node.Bounds.Height / (InputCnt + 1)) * curInPtCnt);

                    NTextShape portName = new NTextShape(ports[i].Name,
                        port.Bounds.Width / 2, (node.Bounds.Height / (InputCnt + 1)) * curInPtCnt,
                        ports[i].Name.Length * 5, port.Bounds.Height);
                    portName.Style.TextStyle = new NTextStyle();
                    portName.Style.TextStyle.FontStyle = new NFontStyle(new Font("Arial", 6));
                    portName.Style.TextStyle.StringFormatStyle.HorzAlign = Nevron.HorzAlign.Left;
                    group.Shapes.AddChild(portName);
                }
                else
                {
                    curOutPtCnt += 1;
                    port.Location = new NPointF((-port.Bounds.Width / 2) + node.Bounds.Width, (node.Bounds.Height / (OutputCnt + 1)) * curOutPtCnt);

                    NTextShape portName = new NTextShape(ports[i].Name,
                        node.Bounds.Width - (port.Bounds.Width / 2) - (ports[i].Name.Length * 5), (node.Bounds.Height / (OutputCnt + 1)) * curOutPtCnt,
                        ports[i].Name.Length * 5, port.Bounds.Height);
                    portName.Style.TextStyle = new NTextStyle();
                    portName.Style.TextStyle.FontStyle = new NFontStyle(new Font("Arial", 6));
                    portName.Style.TextStyle.StringFormatStyle.HorzAlign = Nevron.HorzAlign.Right;
                    group.Shapes.AddChild(portName);
                }



                port.CreateShapeElements(ShapeElementsMask.Ports);

                NDynamicPort portInner;
                if (ports[i].Type == PortType.IN)
                {
                    portInner = new NDynamicPort(new NContentAlignment(-50, 0), DynamicPortGlueMode.GlueToContour);
                    portInner.Name = "IN";
                }
                else
                {
                    portInner = new NDynamicPort(new NContentAlignment(50, 0), DynamicPortGlueMode.GlueToContour);
                    portInner.Name = "OUT";
                }
                port.Ports.AddChild(portInner);
            }

            group.UpdateModelBounds();

            return group;
        }

        private NGroup CreateInstance2(float x, float y, int width, int height, string name,
            int inPortCnt, int outPortCnt)
        {
            double nWidth = 100, nHeight = 100;
            double innerInputPortWidth = 0, innerOutputPortWidth = 0;

            innerInputPortWidth = ((nWidth - (nWidth * 0.1)) / (double)inPortCnt) / 3;
            innerOutputPortWidth = ((nWidth - (nWidth * 0.1)) / (double)outPortCnt) / 3;

            //NCompositeShape instance = new NCompositeShape();


            NGroup group = new NGroup();

            // create node
            NRectangleShape node = new NRectangleShape(0, 0, (int)nWidth, (int)nHeight);
            node.Text = name;
            group.Shapes.AddChild(node);

            // create and add input ports
            for (int i = 0; i < inPortCnt; i++)
            {
                string portName = "IN" + i;


                NPolygonShape port = new NPolygonShape(new NPointF[] { new NPointF(0, 0),
                new NPointF((int)(innerInputPortWidth * 1.5) , 0),
                new NPointF((int)(innerInputPortWidth * 2), (int)(innerInputPortWidth / 2)),
                new NPointF((int)(innerInputPortWidth * 1.5) , (int)(innerInputPortWidth)),
                new NPointF(0, (int)(innerInputPortWidth))
                });

                port.Name = portName;
                port.Text = portName;


                group.Shapes.AddChild(port);
                port.Location = new NPointF((int)(port.Width / (-2)), (int)((nWidth * 0.1) + ((nWidth / inPortCnt) * i)));

                port.CreateShapeElements(ShapeElementsMask.Ports);

                NDynamicPort portInner = new NDynamicPort(new NContentAlignment(-50, 0), DynamicPortGlueMode.GlueToContour);
                portInner.Name = "IN";
                port.Ports.AddChild(portInner);
            }


            // create and add input ports
            for (int i = 0; i < outPortCnt; i++)
            {
                string portName = "OUT" + i;


                NPolygonShape port = new NPolygonShape(new NPointF[] { new NPointF(0, 0),
                new NPointF((int)(innerOutputPortWidth * 1.5) , 0),
                new NPointF((int)(innerOutputPortWidth * 2), (int)(innerOutputPortWidth / 2)),
                new NPointF((int)(innerOutputPortWidth * 1.5) , (int)(innerOutputPortWidth)),
                new NPointF(0, (int)(innerOutputPortWidth))
                });

                port.Name = portName;
                port.Text = portName;

                group.Shapes.AddChild(port);
                port.Location = new NPointF((int)(port.Width / (-2)) + (int)nWidth, (int)((nWidth * 0.1) + ((nWidth / outPortCnt) * i)));

                port.CreateShapeElements(ShapeElementsMask.Ports);

                NDynamicPort portInner = new NDynamicPort(new NContentAlignment(50, 0), DynamicPortGlueMode.GlueToContour);
                portInner.Name = "OUT";
                port.Ports.AddChild(portInner);
            }

            group.UpdateModelBounds();

            return group;
        }

        private NShape CreateGlobalPort(string name, PortType type, int width, int height, NPointF location)
        {
            int sizeOfPort = 0;

            sizeOfPort = name.Length * 5;
            //width = sizeOfPort;

            NShape port;

            port = new NPolygonShape(new NPointF[] { new NPointF(0, 0),
                new NPointF((int)(width * 1.5) , 0),
                new NPointF((int)(width * 1.5 + 20), (int)(height / 2)),
                new NPointF((int)(width * 1.5) , (int)(height)),
                new NPointF(0, (int)(height))
                });

            port.Name = name;
            port.Text = name;

            port.CreateShapeElements(ShapeElementsMask.Ports);

            NDynamicPort portInner;
            if (type == PortType.IN)
                portInner = new NDynamicPort(new NContentAlignment(50, 0), DynamicPortGlueMode.GlueToContour);
            else
                portInner = new NDynamicPort(new NContentAlignment(-50, 0), DynamicPortGlueMode.GlueToContour);
            portInner.Name = name;
            port.Ports.AddChild(portInner);

            port.Location = location;

            return port;
        }

        private NGroup CreateGlobalPort(string name, PortType type)
        {
            int width = 10;
            int height = 10;

            NGroup group = new NGroup();

            NShape port = new NPolygonShape(new NPointF[] { new NPointF(0, 0),
                new NPointF((int)(width * 1.5) , 0),
                new NPointF((int)(width * 1.5 + 10), (int)(height / 2)),
                new NPointF((int)(width * 1.5) , (int)(height)),
                new NPointF(0, (int)(height))
                });

            group.Shapes.AddChild(port);

            port.Name = name;

            port.CreateShapeElements(ShapeElementsMask.Ports);

            NDynamicPort portInner;
            if (type == PortType.IN)
                portInner = new NDynamicPort(new NContentAlignment(50, 0), DynamicPortGlueMode.GlueToContour);
            else
                portInner = new NDynamicPort(new NContentAlignment(-50, 0), DynamicPortGlueMode.GlueToContour);
            portInner.Name = name;
            port.Ports.AddChild(portInner);

            NTextShape nodeName;
            if (type == PortType.IN)
            {
                nodeName = new NTextShape(name, -(name.Length * 5), 0, name.Length * 5, height);
            }
            else
            {
                nodeName = new NTextShape(name, port.Bounds.Width, 0, name.Length * 5, height);
            }
            nodeName.Style.TextStyle = new NTextStyle();
            nodeName.Style.TextStyle.FontStyle = new NFontStyle(new Font("Arial", 7));
            group.Shapes.AddChild(nodeName);

            group.UpdateModelBounds();

            return group;
        }

        private NShape createPort(string name, PortType type)
        {
            int width = 10;
            int height = 10;

            NShape port;

            port = new NPolygonShape(new NPointF[] { new NPointF(0, 0),
                new NPointF((int)(width * 1.5) , 0),
                new NPointF((int)(width * 1.5 + 10), (int)(height / 2)),
                new NPointF((int)(width * 1.5) , (int)(height)),
                new NPointF(0, (int)(height))
                });

            port.Name = name;

            port.CreateShapeElements(ShapeElementsMask.Ports);

            NDynamicPort portInner;
            if (type == PortType.IN)
                portInner = new NDynamicPort(new NContentAlignment(-50, 0), DynamicPortGlueMode.GlueToContour);
            else
                portInner = new NDynamicPort(new NContentAlignment(50, 0), DynamicPortGlueMode.GlueToContour);
            portInner.Name = name;
            port.Ports.AddChild(portInner);


            return port;
        }

        private List<Port> randomPortGen()
        {
            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            List<Port> temp = new List<Port>();

            for (int i = 0; i < r.Next(2, 10); i++)
            {
                temp.Add(new Port(PortType.IN, "InPt" + i));
            }

            for (int i = 0; i < r.Next(2, 10); i++)
            {
                temp.Add(new Port(PortType.OUT, "OutPt" + i));
            }

            return temp;
        }

        private List<Port> randomPortGen_ranStr()
        {
            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            List<Port> temp = new List<Port>();

            for (int i = 0; i < r.Next(0, 10); i++)
            {
                temp.Add(new Port(PortType.IN, "InPt" + RandomString(new Random(new System.DateTime().Millisecond).Next(0, 10), r)));
            }

            for (int i = 0; i < r.Next(0, 10); i++)
            {
                temp.Add(new Port(PortType.OUT, "OutPt" + RandomString(new Random(new System.DateTime().Millisecond).Next(0, 10), r)));
            }

            return temp;
        }

        private static string RandomString(int length, Random r)
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[r.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }
    }
}
