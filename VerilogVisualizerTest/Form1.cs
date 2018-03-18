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
using Nevron.Diagram.WinForm;
using Nevron.Filters;


namespace VerilogVisualizerTest
{
    public partial class Form1 : Form
    {
        public List<Module> Modules;
        private static readonly NFilter ExpandCollapseDecoratorFilter = new NInstanceOfTypeFilter(typeof(NExpandCollapseDecorator));


        public Form1()
        {
            InitializeComponent();

            Modules = new List<Module>();

            ReadXMLData();

            testReadStructure();
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

        private void testReadStructure()
        {
            if (Modules.Count > 0)
            {
                foreach (Module mod in Modules)
                {
                    Console.WriteLine(mod.name);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            // begin view init
            nDrawingView1.BeginInit();

            // display the document in the view
            nDrawingView1.Document = nDrawingDocument1;

            // show ports
            nDrawingView1.GlobalVisibility.ShowPorts = true;

            // hide the grid
            nDrawingView1.Grid.Visible = false;

            // fit the document in the viewport 
            nDrawingView1.ViewLayout = ViewLayout.Fit;

            // apply padding to the document bounds
            nDrawingView1.DocumentPadding = new Nevron.Diagram.NMargins(10);

            // init document
            nDrawingDocument1.BeginInit();

            // create the flowcharting shapes factory
            NFlowChartingShapesFactory factory = new NFlowChartingShapesFactory(nDrawingDocument1);

            // modify the connectors style sheet
            NStyleSheet styleSheet = (nDrawingDocument1.StyleSheets.GetChildByName(NDR.NameConnectorsStyleSheet, -1) as NStyleSheet);

            NTextStyle textStyle = new NTextStyle();
            textStyle.BackplaneStyle.Visible = true;
            textStyle.BackplaneStyle.StandardFrameStyle.InnerBorderWidth = new NLength(0);
            styleSheet.Style.TextStyle = textStyle;

            styleSheet.Style.StrokeStyle = new NStrokeStyle(1, Color.Black);
            styleSheet.Style.StartArrowheadStyle.StrokeStyle = new NStrokeStyle(1, Color.Black);
            styleSheet.Style.EndArrowheadStyle.StrokeStyle = new NStrokeStyle(1, Color.Black);

            InitDocument();

            /*
            // create the begin shape 
            NShape begin = factory.CreateShape(FlowChartingShapes.Termination);
            begin.Bounds = new NRectangleF(100, 100, 100, 100);
            begin.Text = "BEGIN";
            nDrawingDocument1.ActiveLayer.AddChild(begin);

            // create the step1 shape
            NShape step1 = factory.CreateShape(FlowChartingShapes.Process);
            step1.Bounds = new NRectangleF(100, 400, 100, 100);
            step1.Text = "STEP1";
            nDrawingDocument1.ActiveLayer.AddChild(step1);

            // connect begin and step1 with bezier link
            NBezierCurveShape bezier = new NBezierCurveShape();
            bezier.StyleSheetName = NDR.NameConnectorsStyleSheet;
            bezier.Text = "BEZIER";
            bezier.FirstControlPoint = new NPointF(100, 300);
            bezier.SecondControlPoint = new NPointF(200, 300);
            nDrawingDocument1.ActiveLayer.AddChild(bezier);
            bezier.FromShape = begin;
            bezier.ToShape = step1;

            // create question1 shape
            NShape question1 = factory.CreateShape(FlowChartingShapes.Decision);
            question1.Bounds = new NRectangleF(300, 400, 100, 100);
            question1.Text = "QUESTION1";
            nDrawingDocument1.ActiveLayer.AddChild(question1);

            // connect step1 and question1 with line link
            NLineShape line = new NLineShape();
            line.StyleSheetName = NDR.NameConnectorsStyleSheet;
            line.Text = "LINE";
            nDrawingDocument1.ActiveLayer.AddChild(line);
            line.FromShape = step1;
            line.ToShape = question1;

            // create the step2 shape
            NShape step2 = factory.CreateShape(FlowChartingShapes.Process);
            step2.Bounds = new NRectangleF(500, 100, 100, 100);
            step2.Text = "STEP2";
            nDrawingDocument1.ActiveLayer.AddChild(step2);

            // connect step2 and question1 with HV link
            NStep2Connector hv1 = new NStep2Connector(false);
            hv1.StyleSheetName = NDR.NameConnectorsStyleSheet;
            hv1.Text = "HV1";
            nDrawingDocument1.ActiveLayer.AddChild(hv1);
            hv1.FromShape = step2;
            hv1.ToShape = question1;

            // connect question1 and step2 and with HV link
            NStep2Connector hv2 = new NStep2Connector(false);
            hv2.StyleSheetName = NDR.NameConnectorsStyleSheet;
            hv2.Text = "HV2";
            nDrawingDocument1.ActiveLayer.AddChild(hv2);
            hv2.FromShape = question1;
            hv2.ToShape = step2;

            // create a self loof as bezier on step2
            NBezierCurveShape selfLoop = new NBezierCurveShape();
            selfLoop.StyleSheetName = NDR.NameConnectorsStyleSheet;
            selfLoop.Text = "SELF LOOP";
            nDrawingDocument1.ActiveLayer.AddChild(selfLoop);
            selfLoop.FromShape = step2;
            selfLoop.ToShape = step2;
            selfLoop.Reflex();

            // create step3 shape
            NShape step3 = factory.CreateShape(FlowChartingShapes.Process);
            step3.Bounds = new NRectangleF(700, 600, 100, 100);
            step3.Text = "STEP3";
            nDrawingDocument1.ActiveLayer.AddChild(step3);

            // connect question1 and step3 with an HVH link
            NStep3Connector hvh1 = new NStep3Connector(false, 50, 0, true);
            hvh1.StyleSheetName = NDR.NameConnectorsStyleSheet;
            hvh1.Text = "HVH1";
            nDrawingDocument1.ActiveLayer.AddChild(hvh1);
            hvh1.FromShape = question1;
            hvh1.ToShape = step3;

            // create end shape
            NShape end = factory.CreateShape(FlowChartingShapes.Termination);
            end.Bounds = new NRectangleF(300, 700, 100, 100);
            end.Text = "END";
            nDrawingDocument1.ActiveLayer.AddChild(end);

            // connect step3 and end with VH link
            NStep2Connector vh1 = new NStep2Connector(true);
            vh1.StyleSheetName = NDR.NameConnectorsStyleSheet;
            vh1.Text = "VH1";
            nDrawingDocument1.ActiveLayer.AddChild(vh1);
            vh1.FromShape = step3;
            vh1.ToShape = end;

            // connect question1 and end with curve link (uses explicit ports)
            NRoutableConnector curve = new NRoutableConnector(RoutableConnectorType.DynamicCurve);
            curve.StyleSheetName = NDR.NameConnectorsStyleSheet;
            curve.Text = "CURVE";
            nDrawingDocument1.ActiveLayer.AddChild(curve);
            curve.StartPlug.Connect(question1.Ports.GetChildAt(3) as NPort);
            curve.EndPlug.Connect(end.Ports.GetChildAt(1) as NPort);
            curve.InsertPoint(1, new NPointF(500, 600));
            */


            // set a shadow to the nDrawingDocument1. Since styles are inheritable all objects will reuse this shadow
            nDrawingDocument1.Style.ShadowStyle = new NShadowStyle(
                ShadowType.GaussianBlur,
                Color.Gray,
                new NPointL(5, 5),
                1,
                new NLength(3));
            
            // shadows must be displayed behind document content
            nDrawingDocument1.ShadowsZOrder = ShadowsZOrder.BehindDocument;

            // end nDrawingDocument1 init
            nDrawingDocument1.EndInit();

            //end view init
            nDrawingView1.EndInit();
            

            /*
            // begin view init
            nDrawingView1.BeginInit();

            nDrawingView1.Grid.Visible = false;
            nDrawingView1.HorizontalRuler.Visible = false;
            nDrawingView1.VerticalRuler.Visible = false;
            nDrawingView1.GlobalVisibility.ShowPorts = true;

            // init document
            nDrawingDocument1.BeginInit();
            InitDocument();
            nDrawingDocument1.EndInit();

            // end view init
            nDrawingView1.EndInit();
            */
        }

        
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
            /*
            NGroup subgroupA1 = (NGroup)groupA.Shapes.GetChildAt(0);
            NShape shapeA1a = (NShape)subgroupA1.Shapes.GetChildAt(0);
            NGroup subgroupA2 = (NGroup)groupA.Shapes.GetChildAt(1);
            NShape shapeA2a = (NShape)subgroupA2.Shapes.GetChildAt(0);
            Connect(shapeA1a, shapeA2a);

            NGroup subgroupB2 = (NGroup)groupB.Shapes.GetChildAt(1);
            NShape shapeB2a = (NShape)subgroupB2.Shapes.GetChildAt(0);
            Connect(shapeA2a, shapeB2a);
            */
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
        

    }
}
