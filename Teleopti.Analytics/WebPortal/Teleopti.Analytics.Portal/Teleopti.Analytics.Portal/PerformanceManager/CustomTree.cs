using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Portal.PerformanceManager
{
    public class CustomTree : TreeView
    {
        protected override TreeNode CreateNode()
        {
            return new CustomTreeNode(this, false);
        }
    }

    internal class CustomTreeNode : TreeNode
    {
        public CustomTreeNode(CustomTree owner, bool isRoot) : base(owner, isRoot) { }
    }
}