using System.Windows.Forms;

public class XNode : TreeNode
{
    public string Id { get; set; }

    public XNode(string id, string text) : base(text)
    {
        this.Id = id;
    }
}