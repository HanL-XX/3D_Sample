using System;
using System.Windows.Forms;
using System.Xml;

namespace TreeViewApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLoadXml_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML Files|*.xml";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadXmlToTreeView(openFileDialog1.FileName);
            }
        }

        private void LoadXmlToTreeView(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            treeViewXml.Nodes.Clear();

            foreach (XmlNode node in xmlDoc.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    string text = node.Attributes?["text"]?.Value ?? "Unnamed";
                    string id = node.Attributes?["id"]?.Value ?? "null";

                    XNode rootNode = new XNode(id, text);
                    treeViewXml.Nodes.Add(rootNode);

                    AddChildNodes(node, rootNode);
                }
            }
        }

        private void AddChildNodes(XmlNode xmlNode, XNode parentNode)
        {
            foreach (XmlNode childXmlNode in xmlNode.ChildNodes)
            {
                if (childXmlNode.Attributes != null)
                {
                    string text = childXmlNode.Attributes["text"]?.Value ?? "Unnamed";
                    string id = childXmlNode.Attributes["id"]?.Value ?? "";

                    XNode childNode = new XNode(id, text);
                    parentNode.Nodes.Add(childNode);

                    AddChildNodes(childXmlNode, childNode);
                }
            }
        }
        private void treeViewXml_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node is XNode xNode)
            {
                MessageBox.Show($"Node Selected: {xNode.Text}\nID: {xNode.Id}", "Node Info");
            }
        }
    }
}