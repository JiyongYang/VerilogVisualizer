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
                            Port pt = new Port(ele.Attribute("type").Value == "In" ? Type.IN : Type.OUT, ele.Attribute("name").Value);
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
            c4.StartPlug.Connect(shape1.Ports.GetChildByName("CI", 0) as NPort);
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
            CreateDiagram();
            //LayoutButton.PerformClick();
        }

        // drawing group module
        /*
        private void InitDocument()
        {
            List<NGroup> groups = new List<NGroup>();
            
            foreach (var mod in Modules)
            {
                NGroup group;
                if (mod.verilogInstances.Count > 0)
                {
                    group = CreateGroup(mod.name, mod.verilogInstances);
                }
                else
                {
                    group = CreateGroup(mod.name);
                }
                group.Location = new NPointF(10, 10);
                nDrawingDocument1.ActiveLayer.AddChild(group);
                groups.Add(group);
            }

            // Connections
            foreach (var mod in Modules)
            {
                ;
            }


            // Connect some shapes
            
            NGroup subgroupA1 = (NGroup)groupA.Shapes.GetChildAt(0);
            NShape shapeA1a = (NShape)subgroupA1.Shapes.GetChildAt(0);
            NGroup subgroupA2 = (NGroup)groupA.Shapes.GetChildAt(1);
            NShape shapeA2a = (NShape)subgroupA2.Shapes.GetChildAt(0);
            Connect(shapeA1a, shapeA2a);

            NGroup subgroupB2 = (NGroup)groupB.Shapes.GetChildAt(1);
            NShape shapeB2a = (NShape)subgroupB2.Shapes.GetChildAt(0);
            Connect(shapeA2a, shapeB2a);
            
    }



    private NGroup CreateGroup(string name, List<Module> subModules)
        {
            NGroup group = new NGroup();
            group.Name = name;

            foreach (var mod in subModules)
            {
                NGroup subGroup;
                if (mod.verilogInstances.Count > 0)
                {
                    subGroup = CreateSubgroup(mod.name, mod.verilogInstances);
                }
                else
                {
                    subGroup = CreateSubgroup(mod.name);
                }
                    
                //subGroup.Location = new NPointF(0, 0);
                group.Shapes.AddChild(subGroup);
            }
            
            // Create the decorators
            CreateDecorators(group, group.Name + " Decorators");

            // Update the model bounds so that the subgroups are inside the specified padding
            group.Padding = new Nevron.Diagram.NMargins(5, 5, 30, 5);
            group.UpdateModelBounds();
            group.AutoUpdateModelBounds = true;

            ApplyProtections(group, true, false);
            return group;
        }

        private NGroup CreateGroup(string name)
        {
            NGroup group = new NGroup();
            group.Name = name;

            NShape shape = CreateShape(name + "(Not subgroup)");
            //shape.Location = new NPointF(0, 0);
            group.Shapes.AddChild(shape);

            // Create the decorators
            CreateDecorators(group, group.Name + " Group");

            // Update the model bounds so that the subgroups are inside the specified padding
            group.Padding = new Nevron.Diagram.NMargins(5, 5, 30, 5);
            group.UpdateModelBounds();
            group.AutoUpdateModelBounds = true;

            ApplyProtections(group, true, false);
            return group;
        }

        private NGroup CreateSubgroup(string name)
        {
            NGroup subgroup = new NGroup();
            subgroup.Name = name;

            NShape shape1 = CreateShape(name + "shape");
            //shape1.Location = new NPointF(0, 0);
            subgroup.Shapes.AddChild(shape1);

            // Create the decorators
            CreateDecorators(subgroup, subgroup.Name + " Subgroup");

            // Update the model bounds so that the shapes are inside the specified padding
            subgroup.Padding = new Nevron.Diagram.NMargins(5, 5, 30, 5);
            subgroup.UpdateModelBounds();

            ApplyProtections(subgroup, true, true);
            return subgroup;
        }

        private NGroup CreateSubgroup(string name, List<Module> subModules)
        {
            NGroup subgroup = new NGroup();
            subgroup.Name = name;

            foreach (var mod in subModules)
            {
                NGroup subGroup;
                if (mod.verilogInstances.Count > 0)
                    subGroup = CreateSubgroup(mod.name, mod.verilogInstances);
                else
                    subGroup = CreateSubgroup(mod.name);
                //subGroup.Location = new NPointF(0, 0);
                subgroup.Shapes.AddChild(subGroup);
            }

            // Create 2 shapes
            NShape shape1 = CreateShape(name + "shape");
            shape1.Location = new NPointF(0, 0);
            subgroup.Shapes.AddChild(shape1);

            // Create the decorators
            CreateDecorators(subgroup, subgroup.Name + " Subgroup");

            // Update the model bounds so that the shapes are inside the specified padding
            subgroup.Padding = new Nevron.Diagram.NMargins(5, 5, 30, 5);
            subgroup.UpdateModelBounds();

            ApplyProtections(subgroup, true, true);
            return subgroup;
        }


        private NShape CreateShape(string name)
        {
            NShape shape = new NRectangleShape(0, 0, 100, 100);
            shape.Name = name;
            shape.Text = name + " Node";

            // Create a center port
            shape.CreateShapeElements(ShapeElementsMask.Ports);
            NDynamicPort port = new NDynamicPort(new NContentAlignment(0, 0), DynamicPortGlueMode.GlueToContour);
            shape.Ports.AddChild(port);

            ApplyProtections(shape, true, true);
            return shape;
        }

        private void CreateDecorators(NShape shape, string decoratorText)
        {
            // Create the decorators
            shape.CreateShapeElements(ShapeElementsMask.Decorators);

            // Create a frame decorator
            // We want the user to be able to select the shape when the frame is hit
            NFrameDecorator frameDecorator = new NFrameDecorator();
            frameDecorator.ShapeHitTestable = true;
            frameDecorator.Header.Margins = new Nevron.Diagram.NMargins(20, 0, 0, 0);
            frameDecorator.Header.Text = decoratorText;
            shape.Decorators.AddChild(frameDecorator);

            // Create an expand/collapse decorator
            NExpandCollapseDecorator decorator = new NExpandCollapseDecorator();
            shape.Decorators.AddChild(decorator);
        }

        private void Connect(NShape shape1, NShape shape2)
        {
            NLineShape line = new NLineShape();
            nDrawingDocument1.ActiveLayer.AddChild(line);
            line.StyleSheetName = "Connectors";
            line.FromShape = shape1;
            line.ToShape = shape2;
        }

        private void ApplyProtections(NShape shape, bool trackersEdit, bool move)
        {
            NAbilities protection = shape.Protection;
            protection.TrackersEdit = trackersEdit;
            protection.MoveX = move;
            protection.MoveY = move;
            shape.Protection = protection;
        }
        */

    }
}
