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
        public List<Module> Modules;
        //private NOrthogonalGraphLayout m_Layout;

        public Form1()
        {
            InitializeComponent();

            Modules = new List<Module>();

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

                    parsingXmlData(node.Elements(), ref temp, 0);
                    Modules.Add(temp);
                }

            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("[WARNING]----" + ex.ToString());
            }
            Console.WriteLine("[NOTICE]---- successfully load XML file");
        }

        private void parsingXmlData(IEnumerable<XElement> nodes, ref Module temp, int depth)
        {
            foreach (var node in nodes)
            {
                switch (node.Name.ToString())
                {
                    case "VerilogPorts":
                        foreach (var ele in node.Elements("Port"))
                        {
                            //Console.WriteLine(ele.Attribute("name").Value);
                            Port pt = new Port(ele.Attribute("type").Value == "In" ? PortType.IN : PortType.OUT, ele.Attribute("name").Value);
                            temp.Ports.Add(pt);
                        }
                        break;
                    case "VerilogConnections":
                        foreach (var ele in node.Elements("VerilogConnection"))
                        {
                            //Console.WriteLine(ele.Attribute("fPort").Value);
                            VerilogConnection conn = new VerilogConnection(ele.Attribute("from").Value,
                                    ele.Attribute("fPort").Value, ele.Attribute("to").Value, ele.Attribute("tPort").Value);
                            temp.VerilogConnections.Add(conn);
                        }
                        break;
                    case "Modules":
                        foreach (XElement subNode in node.Elements())
                        {
                            //Console.WriteLine(subNode.Name.ToString());
                            Module subModule = new Module(subNode.Attribute("name").Value);
                            parsingXmlData(subNode.Elements(), ref subModule, depth + 1);
                            temp.VerilogInstances.Add(subModule);
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
            nDrawingView1.ViewLayout = ViewLayout.Fit;

            // apply padding to the document bounds
            nDrawingView1.DocumentPadding = new Nevron.Diagram.NMargins(10);

            // init document
            document.BeginInit();

            // modify the connectors style sheet
            NStyleSheet styleSheet = (document.StyleSheets.GetChildByName(NDR.NameConnectorsStyleSheet, -1) as NStyleSheet);

            NTextStyle textStyle = new NTextStyle();
            textStyle.BackplaneStyle.Visible = true;
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

        private void InitDocument()
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

            NShape Inport1 = CreatePort("Input1", PortType.IN, 40, 40, new NPointF(50, 300));
            NShape Inport2 = CreatePort("Input2", PortType.IN, 40, 40, new NPointF(50, 400));
            NShape Outport1 = CreatePort("Output1", PortType.OUT, 40, 40, new NPointF(700, 350));

            document.ActiveLayer.AddChild(shape);
            document.ActiveLayer.AddChild(shape2);
            document.ActiveLayer.AddChild(shape3);
            document.ActiveLayer.AddChild(Inport1);
            document.ActiveLayer.AddChild(Inport2);
            document.ActiveLayer.AddChild(Outport1);

            NStep3Connector c1 = new NStep3Connector(false, 50, 0, true);
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
            c5.EndPlug.Connect(Outport1.Ports.GetChildByName("Output1", 0) as NPort);

        }

        private NGroup CreateInstance2(float x, float y, int width, int height, string name,
            int inPortCnt, int outPortCnt)
        {
            double nWidth = 100, nHeight = 100;
            double innerInputPortWidth = 0, innerOutputPortWidth = 0;

            innerInputPortWidth = ((nWidth - (nWidth * 0.1))/(double)inPortCnt)/3;
            innerOutputPortWidth = ((nWidth - (nWidth * 0.1))/(double)outPortCnt)/3;

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
                port.Location = new NPointF((int)(port.Width/(-2)), (int)((nWidth * 0.1) + ((nWidth/inPortCnt)*i)));

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
        
        private NShape CreatePort(string name, PortType type, int width, int height, NPointF location)
        {
            NShape port;

            port = new NPolygonShape(new NPointF[] { new NPointF(0, 0),
                new NPointF((int)(width * 1.5) , 0),
                new NPointF((int)(width * 2), (int)(width / 2)),
                new NPointF((int)(width * 1.5) , (int)(width)),
                new NPointF(0, (int)(width))
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
    }
}
