using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    public partial class MainForm : Form
    {
        public Module topModule;
        public Dictionary<string, Module> ModulePool;
        public Dictionary<string, BaseModel> BaseModels;

        private string topModuleName;

        List<UDNGroup> inPortList;
        List<UDNGroup> outPortList;
        List<UDNGroup> instanceList;
        MultiKeyDictionary<string, string, UDNGroup> objMDict;


        public MainForm()
        {
            InitializeComponent();

            topModule = new Module("TopModule");
            ModulePool = new Dictionary<string, Module>();
            BaseModels = new Dictionary<string, BaseModel>();

            inPortList = new List<UDNGroup>();
            outPortList = new List<UDNGroup>();
            instanceList = new List<UDNGroup>();
            objMDict = new MultiKeyDictionary<string, string, UDNGroup>();

            ReadXMLData();
            Form_Update();
            topModuleName = topModule.Name;
        }

        private void Form_Update()
        {
            TreeList_Update();
        }

        private void TreeList_Update()
        {
            // Structure list
            TreeNode topNode = new TreeNode(topModule.Name);

            for (int i = 0; i < topModule.Instances.Count; i++)
            {
                TreeNode temp;
                if (topModule.Instances[i].Instances.Count > 0)
                {
                    temp = new TreeNode(topModule.Instances[i].Name);
                    TreeList_AddInstance(ref temp, topModule.Instances[i].Instances);
                    topNode.Nodes.Add(temp);
                }
                else if(topModule.Instances[i].Type != "None")
                {
                    temp = new TreeNode(topModule.Instances[i].Name);
                    TreeList_AddInstance(ref temp, ModulePool[topModule.Instances[i].Type].Instances);
                    topNode.Nodes.Add(temp);
                }
                else
                {
                    topNode.Nodes.Add(topModule.Instances[i].Id, topModule.Instances[i].Name);
                }
            }

            treeView1.Nodes.Add(topNode);

            // Expend All Tree Nodes
            treeView1.ExpandAll();

            // Base model list

            foreach (KeyValuePair<string, BaseModel> item in BaseModels)
            {
                TreeNode tn = new TreeNode(item.Key);
                treeView2.Nodes.Add(tn);
            }

            treeView2.ExpandAll();
        }

        private void TreeList_AddInstance(ref TreeNode node, List<Module> mod)
        {
            TreeNode temp;
            for (int i = 0; i < mod.Count; i++)
            {
                if(mod[i].Instances.Count > 0)
                {
                    temp = new TreeNode(mod[i].Name);
                    TreeList_AddInstance(ref temp, mod[i].Instances);
                    node.Nodes.Add(temp);
                }
                else if(mod[i].Type != "None")
                {
                    temp = new TreeNode(mod[i].Name);
                    TreeList_AddInstance(ref temp, ModulePool[mod[i].Type].Instances);
                    node.Nodes.Add(temp);
                }
                else
                {
                    node.Nodes.Add(mod[i].Id, mod[i].Name);
                }
            }
        }

        private string makeId(int depth, int offset)
        {
            return depth.ToString() + "_" + offset.ToString();
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
        
        private void InitDocument()
        {
            
            // draw global port
            for (int i = 0; i < topModule.Ports.Count; i++)
            {
                UDNGroup gPort = CreateGlobalPort(topModule.Ports[i].Name, topModule.Ports[i].Type);
                document.ActiveLayer.AddChild(gPort);
                if (topModule.Ports[i].Type == PortType.IN)
                    inPortList.Add(gPort);
                else
                    outPortList.Add(gPort);
                objMDict[topModule.Name, topModule.Ports[i].Name] = gPort;
            }


            for (int i = 0; i < topModule.Instances.Count; i++)
            {
                string key = topModule.Instances[i].Name;
                UDNGroup instance = CreateInstance(key, ModulePool[topModule.Instances[i].Type].Ports , topModule.Instances[i].Id);
                document.ActiveLayer.AddChild(instance);
                instanceList.Add(instance);
                //objDict[key] = instance;
                for (int j = 0; j < ModulePool[topModule.Instances[i].Type].Ports.Count; j++)
                {
                    objMDict[key, ModulePool[topModule.Instances[i].Type].Ports[j].Name] = instance;
                }
                
            }

            setInstancesPos(instanceList, inPortList, outPortList);

            
            NRoutableConnector routableConnector;
            for (int i = 0; i < topModule.Instances.Count; i++)
            {
                for (int j = 0; j < topModule.Instances[i].Couplings.Count; j++)
                {
                    routableConnector = new NRoutableConnector(RoutableConnectorType.DynamicHV, RerouteAutomatically.Always);
                    routableConnector.StyleSheetName = NDR.NameConnectorsStyleSheet;
                    routableConnector.Style.StrokeStyle = new NStrokeStyle(1, Color.Blue);
                    document.ActiveLayer.AddChild(routableConnector);

                    var cou = topModule.Instances[i].Couplings[j];

                    var sourIns = objMDict[cou.From, cou.FPort];
                    var destIns = objMDict[cou.To, cou.TPort];

                    routableConnector.StartPlug.Connect(((NShape)(sourIns.Shapes.GetChildByName(cou.FPort, 0))).Ports.GetChildByName(cou.FPort, 0) as NPort);
                    routableConnector.EndPlug.Connect(((NShape)(destIns.Shapes.GetChildByName(cou.TPort, 0))).Ports.GetChildByName(cou.TPort, 0) as NPort);
                    routableConnector.DoubleClick += new NodeViewEventHandler(routableConnector_DoubleClick);

                    routableConnector.Reroute();
                }
            }
            

            document.SizeToContent();
        }


        private Module find_instance(string instanceName)
        {
            if (topModule.Name == instanceName)
                return topModule;

            for (int i = 0; i < topModule.Instances.Count; i++)
            {
                if (topModule.Instances[i].Name == instanceName)
                    return topModule.Instances[i];
            }

            for (int i = 0; i < topModule.Instances.Count; i++)
            {
                if (topModule.Instances[i].Instances.Count > 0)
                    return find_instance_recur(topModule.Instances[i].Instances, instanceName);
            }

            // error
            return null;
        }

        private Module find_instance_recur(List<Module> list, string instanceName)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name == instanceName)
                    return list[i];

                if (list[i].Instances.Count > 0)
                    return find_instance_recur(list[i].Instances, instanceName);
            }

            // error
            return null;
        }

        private Module finded_instance_update(Module mod)
        {
            if (mod.Type != "None" && !(mod.Ports.Count > 0))
            {
                Module temp = (Module)ModulePool[mod.Type].ShallowCopy();

                return temp;
                
                for (int i = 0; i < temp.Ports.Count; i++)
                {
                    mod.Ports.Add(temp.Ports[i]);
                }

                for (int i = 0; i < temp.Instances.Count; i++)
                {
                    mod.Instances.Add(temp.Instances[i]);
                }
            }
            return mod;
        }

        private void Update_instance(Module mod)
        {
            Module _module = (Module)mod.ShallowCopy();

            inPortList.Clear();
            outPortList.Clear();
            instanceList.Clear();
            objMDict.Clear();

            if(_module.Name != topModuleName)
                _module.Name = string.Copy(ModulePool[_module.Type].Name);

            // draw global port
            for (int i = 0; i < _module.Ports.Count; i++)
            {
                UDNGroup gPort = CreateGlobalPort(_module.Ports[i].Name, _module.Ports[i].Type);
                document.ActiveLayer.AddChild(gPort);
                if (_module.Ports[i].Type == PortType.IN)
                    inPortList.Add(gPort);
                else
                    outPortList.Add(gPort);
                objMDict[_module.Name, _module.Ports[i].Name] = gPort;
            }


            for (int i = 0; i < _module.Instances.Count; i++)
            {
                string key = _module.Instances[i].Name;
                UDNGroup instance = CreateInstance(key, ModulePool[_module.Instances[i].Type].Ports, _module.Instances[i].Id);
                document.ActiveLayer.AddChild(instance);
                instanceList.Add(instance);
                for (int j = 0; j < ModulePool[_module.Instances[i].Type].Ports.Count; j++)
                {
                    objMDict[key, ModulePool[_module.Instances[i].Type].Ports[j].Name] = instance;
                }

            }

            setInstancesPos(instanceList, inPortList, outPortList);

            
            NRoutableConnector routableConnector;
            for (int i = 0; i < _module.Instances.Count; i++)
            {
                for (int j = 0; j < _module.Instances[i].Couplings.Count; j++)
                {
                    routableConnector = new NRoutableConnector(RoutableConnectorType.DynamicHV, RerouteAutomatically.Always);
                    routableConnector.Name = "name";
                    routableConnector.StyleSheetName = NDR.NameConnectorsStyleSheet;
                    routableConnector.Style.StrokeStyle = new NStrokeStyle(1, Color.Blue);
                    document.ActiveLayer.AddChild(routableConnector);

                    var cou = _module.Instances[i].Couplings[j];

                    var sourIns = objMDict[cou.From, cou.FPort];
                    var destIns = objMDict[cou.To, cou.TPort];

                    routableConnector.StartPlug.Connect(((NShape)(sourIns.Shapes.GetChildByName(cou.FPort, 0))).Ports.GetChildByName(cou.FPort, 0) as NPort);
                    routableConnector.EndPlug.Connect(((NShape)(destIns.Shapes.GetChildByName(cou.TPort, 0))).Ports.GetChildByName(cou.TPort, 0) as NPort);
                    routableConnector.Reroute();
                 
                }

            }
            

            document.SizeToContent();

            _module = null;
        }

        private void setInstancesPos(List<UDNGroup> gList, List<UDNGroup> gPortInList, List<UDNGroup> gPortOutList)
        {
            int xPortPos = 50;
            int yPortPos = 200;

            int xInsPos = 200;
            int yInsPos = 100;

            for (int i = 0; i < gPortInList.Count; i++)
            {
                gPortInList[i].Location = new NPointF(xPortPos, yPortPos);
                yPortPos += 60;
            }

            for (int i = 0; i < gList.Count; i++)
            {
                gList[i].Location = new NPointF(xInsPos, yInsPos);
                xInsPos += (int)gList[i].Bounds.Width + 100;

                if ((i + 1) % 3 == 0 && i != 0)
                {
                    yInsPos = yInsPos + (int)gList[i - 2].Bounds.Height + 100;
                    //yInsPos = yInsPos + 100;//(int)gList[i - 2].Bounds.Height + 100;

                    //MessageBox.Show(string.Format("Y value: {0}", yInsPos));

                    if (xPortPos < xInsPos)
                        xPortPos = xInsPos;

                    xInsPos = 200;

                }

                // check instance max x pos
                if (xPortPos < xInsPos)
                    xPortPos = xInsPos;
            }

            yPortPos = 200;

            for (int i = 0; i < gPortOutList.Count; i++)
            {
                gPortOutList[i].Location = new NPointF(xPortPos, yPortPos);
                yPortPos += 60;
            }
        }

        private UDNGroup CreateInstance(string name, List<Port> ports, string id)
        {
            int instanceWidth = 50;
            int instanceHeight = 50;

            int InputMaxSize = 10;
            int InputCnt = 0;
            int OutputMaxSize = 10;
            int OutputCnt = 0;

            int offsetWidth = 9;
            int offsetHeight = 30;
            int widthPadding = 10;
            int heightPadding = 10;

            int textWidth = 30;
            int textHeight = 15;
            double textOffset = 1.5;

            int curInPtCnt = 0;
            int curOutPtCnt = 0;

            UDNGroup group = new UDNGroup();
            group.Name = name;

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
            nodeName.Style.TextStyle.FontStyle = new NFontStyle(new Font("Arial", 9));
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
                        port.Bounds.Width / 2,      (node.Bounds.Height / (InputCnt + 1)) * curInPtCnt,
                        ports[i].Name.Length * 9,   (int)(port.Bounds.Height * textOffset));
                    portName.Style.TextStyle = new NTextStyle();
                    portName.Style.TextStyle.FontStyle = new NFontStyle(new Font("Arial", 9));
                    portName.Style.TextStyle.StringFormatStyle.HorzAlign = Nevron.HorzAlign.Left;
                    group.Shapes.AddChild(portName);
                }
                else
                {
                    curOutPtCnt += 1;
                    port.Location = new NPointF((-port.Bounds.Width / 2) + node.Bounds.Width, (node.Bounds.Height / (OutputCnt + 1)) * curOutPtCnt);

                    NTextShape portName = new NTextShape(ports[i].Name,
                        node.Bounds.Width - (port.Bounds.Width / 2) - (ports[i].Name.Length * 9), (node.Bounds.Height / (OutputCnt + 1)) * curOutPtCnt,
                        ports[i].Name.Length * 9, (int)(port.Bounds.Height * textOffset));
                    portName.Style.TextStyle = new NTextStyle();
                    portName.Style.TextStyle.FontStyle = new NFontStyle(new Font("Arial", 9));
                    portName.Style.TextStyle.StringFormatStyle.HorzAlign = Nevron.HorzAlign.Right;
                    group.Shapes.AddChild(portName);
                }



                port.CreateShapeElements(ShapeElementsMask.Ports);

                NDynamicPort portInner;
                if (ports[i].Type == PortType.IN)
                {
                    portInner = new NDynamicPort(new NContentAlignment(-50, 0), DynamicPortGlueMode.GlueToContour);
                    portInner.Name = ports[i].Name;
                }
                else
                {
                    portInner = new NDynamicPort(new NContentAlignment(50, 0), DynamicPortGlueMode.GlueToContour);
                    portInner.Name = ports[i].Name;
                }
                port.Ports.AddChild(portInner);
            }

            group.UpdateModelBounds();

            return group;
        }

        private UDNGroup CreateGlobalPort(string name, PortType type)
        {
            int width = 10;
            int height = 15;

            UDNGroup group = new UDNGroup();
            group.Name = name;

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
                nodeName = new NTextShape(name, -(name.Length * 8), 0, name.Length * 8, height);
            }
            else
            {
                nodeName = new NTextShape(name, port.Bounds.Width, 0, name.Length * 8, height);
            }
            nodeName.Style.TextStyle = new NTextStyle();
            nodeName.Style.TextStyle.FontStyle = new NFontStyle(new Font("Arial", 9));
            if (type == PortType.IN)
            {
                nodeName.Style.TextStyle.StringFormatStyle.HorzAlign = Nevron.HorzAlign.Right;
            }
            else
            {
                nodeName.Style.TextStyle.StringFormatStyle.HorzAlign = Nevron.HorzAlign.Left;
            }
            
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

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;

            MessageBox.Show(string.Format("You selected: {0}", node.Text));

            nDrawingView1.LockRefresh = true;
            document.ActiveLayer.RemoveAllChildren();
            
            Update_instance(finded_instance_update(find_instance(node.Text)));

            nDrawingView1.LockRefresh = false;
        }

        private void treeView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode node = treeView2.SelectedNode;

            MessageBox.Show(string.Format("You selected: {0}", node.Text));

            UDNGroup instance = CreateInstance(node.Text, BaseModels[node.Text].Ports, "None");
            document.ActiveLayer.AddChild(instance);
            instanceList.Add(instance);

            setInstancesPos(instanceList, inPortList, outPortList);

            document.SizeToContent();
        }

        private void routableConnector_DoubleClick(NNodeViewEventArgs args)
        {
            NShape shape = args.Node as NShape;
            if (shape == null)
                return;

            MessageBox.Show(shape.Name + "\n [" + shape.FromShape.AggregateModel.Name + " , " + shape.StartPlug.InwardPort.Name
                + "]\n >> \n[" + shape.ToShape.AggregateModel.Name + " , " + shape.EndPlug.InwardPort.Name + "]", "Country clicked:", MessageBoxButtons.OK, MessageBoxIcon.None);
            args.Handled = true;
        }
    }
}
