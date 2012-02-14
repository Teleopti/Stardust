using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
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
        public CustomTreeNode(string text, string value) : base(text, value)
        {
        }

        public CustomTreeNode(CustomTree owner, bool isRoot) : base(owner, isRoot) { }

        public String JavaScriptValue = String.Empty;

//        protected override void RenderPreText(HtmlTextWriter w)
//        {
//            //w.Write("<input type='submit' value=" + JavaScriptValue + " OnClick='hej'  />");

         
//        }


//        protected override void RenderPostText(HtmlTextWriter writer)
//        {
//            base.RenderPostText(writer);
//            //writer.Write("<img src=\"icon_delete_small.gif\"  />");
//            writer.Write("<table><tr><td></td><td wi><img src=\"icon_delete_small.gif\" /></td></tr></table>");
        
////        <table><tr><td></td><td><img src="icon_delete_small.gif" /></td></tr></table>
        
//        }

    }
}