using System;
using System.Globalization;
using System.Web.UI.WebControls;
using Teleopti.Analytics.Portal.AnalyzerProxy;
using Teleopti.Analytics.Portal.PerformanceManager.ViewModel;

namespace Teleopti.Analytics.Portal.PerformanceManager.View
{
    public partial class ReportTreeView : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTreeView();
            }
        }

        public void LoadTreeView()
        {
            TreeView2.Nodes.Clear();
            //TreeView2.NodeIndent = 50;

            var model = new ReportTreeViewModel();
            foreach (ReportTreeNodeViewModel nodeViewModel in model.ReportTreeNodeViewModels)
            {
                //        CustomTreeNode v = new CustomTreeNode(nodeViewModel.Name, nodeViewModel.Id);                   
                //        CustomTree1.Nodes.Add(v);
                TreeView2.Nodes.Add(new TreeNode(nodeViewModel.Name, nodeViewModel.Id));
            }
        }

        public event EventHandler ReportDeleted;

        protected void OnReportDeleted(EventArgs e)
        {
            if (ReportDeleted != null)
            {
                ReportDeleted(this, e);
            }
        }

        protected void TreeView2_SelectedNodeChanged(object sender, EventArgs e)
        {
            if (TreeView2.NodeStyle.CssClass == "deleteNodeMode")
            {
                // Delete report
                var olapInformation = new OlapInformation();

                using (var clientProxy = new ClientProxy(olapInformation.OlapServer, olapInformation.OlapDatabase))
                {
                    clientProxy.DeleteReport(int.Parse(TreeView2.SelectedValue, CultureInfo.InvariantCulture));
                    OnReportDeleted(e);
                }
            }
            else
            {
            	string queryString = ((IMasterPage)Page.Master).ModifyQueryString(Request.QueryString);
                string url = String.Format(CultureInfo.InvariantCulture, "ShowReport.aspx?reportid={0}&reportname={1}&{2}", 
											TreeView2.SelectedValue,
											TreeView2.SelectedNode.Text,
											queryString);
                Response.Redirect(url);
            }
        }

        public void SetDeleteMode(bool deleteMode)
        {
            if (deleteMode)
            {
                // Prepare for delete mode where u can delete reports, NOT view them
                TreeView2.NodeStyle.CssClass = "deleteNodeMode";
                TreeView2.HoverNodeStyle.CssClass = "hoverDeleteNode";
                TreeView2.NodeStyle.ImageUrl = "../images/icon_deleteSmall.gif";
                return;
            }

            // Prepare for view mode where u NOT can delete reports, only view them
            TreeView2.NodeStyle.CssClass = "normalNodeMode";
            TreeView2.HoverNodeStyle.CssClass = "hoverNormalNode";
            TreeView2.NodeStyle.ImageUrl = "../images/graph_small.gif";
        }
    }
}