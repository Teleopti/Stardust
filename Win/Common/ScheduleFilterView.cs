using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.ScheduleFilter;
using Teleopti.Interfaces.Domain;
using System.Drawing;

namespace Teleopti.Ccc.Win.Common
{
    public partial class ScheduleFilterView : BaseRibbonForm, IScheduleFilterView
    {
        private readonly ScheduleFilterModel _scheduleFilterModel;
        private readonly ScheduleFilterPresenter _scheduleFilterPresenter;

        private TreeViewAdv _currentSelectedTree;

        public bool IsAnyNodeChecked(IPerson person)
        {
            IList<TreeNodeAdv> recursiveNodes = RecursiveNodes(_currentSelectedTree);

            foreach (TreeNodeAdv node in recursiveNodes)
            {
                if (node.Tag == null)
                    continue;

                IList<IPerson> persons = node.Tag as IList<IPerson>;
                if (persons != null)
                {
                    if (persons.Contains(person))
                    {
                        if (node.Checked)
                            return true;
                    }
                }
            }
            return false;
        }

        public ScheduleFilterView(ScheduleFilterModel scheduleFilterModel)
        {
            _scheduleFilterModel = scheduleFilterModel;
            InitializeComponent();

            _scheduleFilterPresenter = new ScheduleFilterPresenter(this, scheduleFilterModel);
            _scheduleFilterPresenter.Initialize();

        }

        public ScheduleFilterModel ScheduleFilterModel
        {
            get
            {
                return _scheduleFilterModel;
            }
        }

        public DateTime CurrentFilterDate
        {
            get { return _scheduleFilterPresenter.CurrentFilterDate; }
        }

        public void CloseFilterForm()
        {
            Close();
        }

        public void SetColor()
        {
            BackColor = ColorHelper.OfficeBlue;
            tabControlAdv.BackColor = ColorHelper.OfficeBlue;
        }

        public void ButtonOkText(string buttonText)
        {
            buttonOk.Text = buttonText;
        }

        public void ButtonCancelText(string buttonText)
        {
            buttonCancel.Text = buttonText;
        }

        public void CreateAndAddTreeNode(CccTreeNode node)
        {
            TreeNodeAdv treeNode = getTreeNode(node);
            addChildNodes(node, treeNode);
            addToTree(treeNode);
        }

        private static TreeNodeAdv getTreeNode(CccTreeNode node)
        {
            TreeNodeAdv treeNode = new TreeNodeAdv(node.DisplayName);
            treeNode.InteractiveCheckBox = true;
            treeNode.Checked = node.IsChecked;
            treeNode.LeftImageIndices = new[] { node.ImageIndex };
            treeNode.Tag = node.Tag;
            treeNode.Expand();
            return treeNode;
        }

        public void AddTabPages(IGroupPage page)
        {
            TabPageAdv tabPage = new TabPageAdv(page.Description.Name);
            tabControlAdv.TabPages.Add(tabPage);
            tabPage.Tag = page;
			if (page.IsUserDefined())
			{
				tabPage.Image = Properties.Resources.ccc_Add_16x16;
			}
        }
        public void AddTabPages(string displayText)
        {
            TabPageAdv tabPage = new TabPageAdv(displayText);
            tabControlAdv.TabPages.Add(tabPage);
        }

        private static void addChildNodes(CccTreeNode node, TreeNodeAdv rootNode)
        {
            foreach (CccTreeNode cccTreeNode in node.Nodes)
            {
                TreeNodeAdv childNode = new TreeNodeAdv(cccTreeNode.DisplayName);
                childNode.InteractiveCheckBox = true;
                addChildNodes(cccTreeNode, childNode);
                childNode.Checked = cccTreeNode.IsChecked;
                childNode.LeftImageIndices = new[] { cccTreeNode.ImageIndex };
                childNode.Tag = cccTreeNode.Tag;
                rootNode.Nodes.Add(childNode);
            }
        }

        private void addToTree(TreeNodeAdv node)
        {
            TreeViewAdv treeView = new TreeViewAdv();
            treeView.ShowCheckBoxes = true;
            treeView.Dock = DockStyle.Fill;
            treeView.LeftImageList = imageList1;
            treeView.AfterCheck += treeView_AfterCheck;
            treeView.MouseClick += treeView_MouseClick;
            treeView.BorderSingle = ButtonBorderStyle.None;
            treeView.Border3DStyle = Border3DStyle.Flat;

            tabControlAdv.SelectedTab.Controls.Add(treeView);
            treeView.Nodes.Add(node);
        }

        void treeView_MouseClick(object sender, MouseEventArgs e)
        {
            _currentSelectedTree = (TreeViewAdv)sender;
            if (e.Button == MouseButtons.Right)
                _scheduleFilterPresenter.OnMouseClick(_currentSelectedTree.PointToScreen(e.Location));
        }



        public object SelectedTabTag()
        {
            return tabControlAdv.SelectedTab.Tag;
        }

		public void SelectTab(string key)
		{
			TabPageAdv page = null;

			foreach (TabPageAdv tabPage in tabControlAdv.TabPages)
			{
				var p = tabPage.Tag as IGroupPage;
				if (p != null && p.Key == key)
				{
					page = tabPage;
					break;
				}
			}

			if (page != null)
			{
				tabControlAdv.SelectedTab = page;
				_scheduleFilterPresenter.OnTabPageSelectedIndexChanged(page.Tag);

			}
		}

        public void ClearTabsAndTrees()
        {
            tabControlAdv.TabPages.Clear();
        }

        public void ClearSelectedTabControls()
        {
            tabControlAdv.SelectedTab.Controls.Clear();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _scheduleFilterPresenter.OnCloseForm();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _scheduleFilterPresenter.OnCancel();
        }

        void treeView_AfterCheck(object sender, TreeNodeAdvEventArgs e)
        {
            _currentSelectedTree = e.Node.TreeView;
            bool selected = e.Node.Checked;
            object tag = e.Node.Tag;

            if (tag == null)
                return;

            IList<IPerson> persons = tag as IList<IPerson>;
            if (persons != null)
            {
                foreach (IPerson selectedPerson in persons)
                {
                    if (selected
                        && !_scheduleFilterModel.SelectedPersons.Contains(selectedPerson))
                    {
                        _scheduleFilterModel.SelectedPersons.Add(selectedPerson);
                    }
                    else if (!selected)
                    {
                        if (!IsAnyNodeChecked(selectedPerson))
                            _scheduleFilterModel.SelectedPersons.Remove(selectedPerson);
                    }
                }
                return;
            }

            KeyValuePair<TeamPersonKey, IPerson> keyValuePair = (KeyValuePair<TeamPersonKey, IPerson>)tag;
            TeamPersonKey teamPersonKey = keyValuePair.Key;
            //}

            if (selected && !_scheduleFilterModel.SelectedPersonDictionary.ContainsKey(teamPersonKey))
                _scheduleFilterModel.SelectedPersonDictionary.Add(teamPersonKey, teamPersonKey.Person);
            else if (!selected)
                _scheduleFilterModel.SelectedPersonDictionary.Remove(teamPersonKey);

            //_scheduleFilterPresenter.OnSelectedPersonChange(e.Node.Tag, e.Node.Checked);
        }

        private static IList<TreeNodeAdv> RecursiveNodes(TreeViewAdv treeView)
        {
            IList<TreeNodeAdv> recursiveNodes = new List<TreeNodeAdv>();

            foreach (TreeNodeAdv node in treeView.Nodes)
            {
                RecursiveNodes(recursiveNodes, node);
            }
            return recursiveNodes;
        }

        private static void RecursiveNodes(IList<TreeNodeAdv> list, TreeNodeAdv node)
        {
            list.Add(node);
            foreach (TreeNodeAdv subNode in node.Nodes)
            {
                RecursiveNodes(list, subNode);
            }
        }

        private void tabControlAdv_SelectedIndexChanged(object sender, EventArgs e)
        {
            _scheduleFilterPresenter.OnTabPageSelectedIndexChanged(SelectedTabTag());
        }

        public void RemoveSelectedIndexChangedHandler()
        {
            tabControlAdv.SelectedIndexChanged -= tabControlAdv_SelectedIndexChanged;
        }

        public void AddSelectedIndexChangedHandler()
        {
            tabControlAdv.SelectedIndexChanged += tabControlAdv_SelectedIndexChanged;
        }

        private void toolStripMenuItemSearch_Click(object sender, EventArgs e)
        {

            _scheduleFilterPresenter.OnToolStripMenuItemSearch();
        }

        public void OpenContextMenu(Point point)
        {

            contextMenuStrip1.Show(point);

        }

        #region IScheduleFilterView Members


        public void DisplaySearch()
        {
            SearchPerson searchForm = new SearchPerson(_scheduleFilterPresenter.PersonCollection);
            searchForm.ShowDialog(this);

            if (searchForm.DialogResult == DialogResult.OK)
            {
                if (searchForm.SelectedPerson != null)
                {
                    TreeNodeAdv tNode;
                    TreeNodeAdvCollection nodeCollection = _currentSelectedTree.Nodes[0].Nodes;

                    foreach (TreeNodeAdv treeNode in nodeCollection)
                    {
                        tNode = Search(treeNode, searchForm.SelectedPerson);

                        if (tNode != null)
                        {
                            _currentSelectedTree.SelectedNode = tNode;
                            _currentSelectedTree.EnsureVisibleSelectedNode = true;
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        private static TreeNodeAdv Search(TreeNodeAdv treeNode, IPerson person)
        {
			if (treeNode.Tag is KeyValuePair<TeamPersonKey, IPerson>)
			{
				var kvp = (KeyValuePair<TeamPersonKey, IPerson>)treeNode.Tag;

				if (person.Equals(kvp.Value))
				{
					return treeNode;
				}
			}

			if(treeNode.Tag is IList<IPerson>)
			{
				var list = (IList<IPerson>) treeNode.Tag;

				if (list.Count > 0)
				{
					if (person.Equals(list[0]))
					{
						return treeNode;
					}
				}
			}
               
            if (treeNode.HasChildren)
            {
                foreach (TreeNodeAdv tNode in treeNode.Nodes)
                {
                    var result = Search(tNode, person);
                    if (result != null)
                        return result;
                }
            }

            if (treeNode.NextNode != null)
            {
                var result = Search(treeNode.NextNode, person);
                if (result != null)
                    return result;

            }

            return null;
        }
    }
}
