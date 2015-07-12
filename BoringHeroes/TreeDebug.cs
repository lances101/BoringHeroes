using System.Windows.Forms;
using TreeSharp;

namespace BoringHeroes
{
    public partial class TreeDebug : Form
    {
        private readonly TreeNode root;

        public TreeDebug()
        {
            InitializeComponent();
            root = treeView1.Nodes.Add("Root");
        }

        public void AnalyzeComposite(Composite comp, TreeNode parent)
        {
            var par = AddNode(comp.Guid.ToString(),
                string.IsNullOrEmpty(comp.DebugName)
                    ? comp.GetType().Name
                    : comp.DebugName + " | " + comp.GetType().Name,
                parent != null ? parent : root);
            if (comp is GroupComposite)
            {
                foreach (var children in (comp as GroupComposite).Children)
                {
                    AnalyzeComposite(children, par);
                }
            }
        }

        public TreeNode AddNode(string key, string name, TreeNode par)
        {
            if (par != null)
            {
                return par.Nodes.Add(key, name);
            }
            return root.Nodes.Add(key, name);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }
    }
}