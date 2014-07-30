using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Matrix;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Win.Reporting;

namespace Teleopti.Ccc.Win.Matrix
{
    public partial class MatrixNavigationView : BaseUserControl, IMatrixNavigationView
    {
        private readonly IComponentContext _componentContext;
        private readonly MatrixNavigationPresenter _presenter;

        public MatrixNavigationView(IMatrixNavigationModel model, IComponentContext componentContext)
        {
            _componentContext = componentContext;
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
            }

            _presenter = new MatrixNavigationPresenter(model, this);
            _presenter.Initialize();
        }

        public void SetColor()
        {
            BackColor = ColorHelper.OfficeBlue;
        }

        public void CreateAndAddTreeNodes(IEnumerable<MatrixTreeNode> treeNodes, IEnumerable<MatrixTreeNode> treeNodes2)
        {
            var treeView = new TreeViewAdv
                                        {
                                            Dock = DockStyle.Fill,
                                            LeftImageList = imageList1,
                                            BorderSingle = ButtonBorderStyle.None,
                                            Border3DStyle = Border3DStyle.Flat,
											Style = TreeStyle.Metro
                                        };

            treeView.KeyDown += treeViewKeyDown;
            treeView.MouseDoubleClick += treeViewMouseDoubleClick;

            var treeNodesAdv = from n in treeNodes select createTreeNode(n);
            treeNodesAdv.ToList().ForEach(n => treeView.Nodes.Add(n));

            var treeNodesAdv2 = from n in treeNodes2 select createTreeNode(n);
            treeNodesAdv2.ToList().ForEach(n => treeView.Nodes.Add(n));

			if (treeView.Nodes.Count > 0)
				treeView.SelectedNode = treeView.Nodes[0];

            Controls.Add(treeView);
        }

        void treeViewMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                openReport((TreeViewAdv)sender);
        }

        void treeViewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                openReport((TreeViewAdv) sender);
        }

        private void openReport(TreeViewAdv treeView)
        {
            if (treeView.SelectedNode != null && treeView.SelectedNode.Tag != null)
            {
                var applicationFunction = (IApplicationFunction)treeView.SelectedNode.Tag;

                var node = (MatrixTreeNode)treeView.SelectedNode.TagObject;
                _presenter.LinkClick(applicationFunction, node.RealTime);
            }
        }

        private static TreeNodeAdv createTreeNode(MatrixTreeNode node)
        {
            var treeNode = new TreeNodeAdv(node.DisplayName)
                               {
                                   LeftImageIndices = new[] { node.ImageIndex },
                                   Tag = node.ApplicationFunction,
                                   TagObject = node
                               };
            treeNode.Expand();
            if (node.Nodes != null)
            {
                node.Nodes.ToList().ForEach(n => treeNode.Nodes.Add(createTreeNode(n)));
            }
            return treeNode;
        }

        public void OpenUrl(Uri url)
        {
			//var proc = new Process
			//               {
			//                   EnableRaisingEvents = false,
			//                   StartInfo =
			//                       {
			//                           FileName = "iexplore.exe",
			//                           Arguments = url.ToString()
			//                       }
			//               };
            try
            {
              //  proc.Start();
	            Process.Start(url.ToString());
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageDialogs.ShowError(this, ex.Message, "Cannot load the Default Browser");
            }
        }

        public void OpenRealTime(IApplicationFunction func)
        {
            Cursor = Cursors.WaitCursor;
            ReportHandler.ShowReport(ReportHandler.CreateReportDetail(func), _componentContext, func);
            Cursor = DefaultCursor;
        }
    }
}