using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class FilterDialog : BaseRibbonForm
    {
        private IList<ITeam> _selectedTeams;
        private ICollection<IPerson> _allPermittedPersons;
        private DateOnlyPeriod _period;
        private TreeNode _userInvoke;

        protected FilterDialog()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public FilterDialog(IList<ITeam> selectedTeams, ICollection<IPerson> allPermittedPersons, DateOnlyPeriod period)
            : this()
        {
            _allPermittedPersons = allPermittedPersons;
            _selectedTeams = selectedTeams;
            _period = period;
        }

        private void InitializeTreeView()
        {
            treeViewOrganization.BeginUpdate();   
            treeViewOrganization.Nodes.Clear();

            IBusinessUnit bu = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit;
            TreeNode rootNode = new TreeNode(bu.Description.ToString());
            rootNode.ImageIndex = 4;
            treeViewOrganization.Nodes.Add(rootNode);

            bool noSelection= false;


            
            if (_selectedTeams==null)
            {
                _selectedTeams = new List<ITeam>();
                if (_allPermittedPersons.Count > 0)
                {
                        foreach (ISite site in bu.SiteCollection)
                        {
                             foreach (ITeam team in site.TeamCollection)
                             {
                                 if (team.PersonsInHierarchy(_allPermittedPersons, _period).Count > 0) 
                                     _selectedTeams.Add(team);
                             }
                        }
                }
                else noSelection = true;
            }
            foreach (ISite site in bu.SiteCollection)
            {
                if (site.PersonsInHierarchy(_allPermittedPersons, _period).Count > 0)
                {
                    TreeNode siteNode = new TreeNode(site.Description.ToString());
                    siteNode.Tag = site;
                    siteNode.ImageIndex = 3;
                    rootNode.Nodes.Add(siteNode);
                    foreach (Team team in site.TeamCollection)
                    {
                        if (team.PersonsInHierarchy(_allPermittedPersons, _period).Count > 0)
                        {
                            TreeNode teamNode = new TreeNode(team.Description.ToString());
                            if (_selectedTeams.Contains(team))
                            {
                                teamNode.Checked = true;
                            }
                            teamNode.Tag = team;
                            teamNode.ImageIndex = 5;
                            siteNode.Nodes.Add(teamNode);
                        }
                    }
                }
            }
            if (noSelection)
            {
                treeViewOrganization.Nodes[0].Checked = true;
                _userInvoke = treeViewOrganization.Nodes[0];
                PropagateCheck(_userInvoke);
            }
            treeViewOrganization.CollapseAll();
            TraverseNodesAndMakeExpanded(treeViewOrganization.Nodes[0]);
         
            if(!noSelection)
                CheckAllNodes(treeViewOrganization.Nodes[0]);
            treeViewOrganization.EndUpdate();
            treeViewOrganization.Refresh();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void treeViewOrganization_AfterCheck(object sender, TreeViewEventArgs e)
        {
           if (e.Action == TreeViewAction.ByMouse)
               _userInvoke = e.Node;

           PropagateCheck(e.Node);   
        }

        private void PropagateCheck(TreeNode node)
        {
            if (IsDescendent(_userInvoke, node))
            {
                foreach (TreeNode childNode in node.Nodes)
                {
                    childNode.Checked = node.Checked;

                    if (childNode.Checked)
                    {
                        node.Expand();
                    }
                    else
                        node.Collapse();
                }  
            }

            if (IsDescendent(node, _userInvoke))
            {
                TreeNode parentNode = node.Parent;

                if (parentNode == null)
                    return;

                int childCheck = 0;
                
                foreach (TreeNode childNode in parentNode.Nodes)
                {
                    if (childNode.Checked)
                    {
                        childCheck += 1;
                    }
                }

                if (childCheck > 0)
                    parentNode.Checked = true;
                else
                    parentNode.Checked = false;
            }

            if (node.Parent != null)
            {
                if (node.Parent.Checked == false)
                    node.Parent.Collapse();
            }
        }

        private static bool IsDescendent(TreeNode parent, TreeNode desc)
        {
            if (parent == null)
                return false;

            if (desc == null)
                return false;

            return desc.FullPath.IndexOf(parent.FullPath,StringComparison.Ordinal) == 0;
        }


        internal IList<ITeam> SelectedTeams()
        {
            IList<ITeam> retList = new List<ITeam>();

            TraverseNodes(treeViewOrganization.Nodes[0], retList);
            return retList;
        }

        private void TraverseNodes(TreeNode topNode,ICollection<ITeam> retList)
        {
            foreach (TreeNode node in topNode.Nodes)
            {
                TraverseNodes(node, retList);
                if (node.Checked)
                {
                    ITeam team = node.Tag as ITeam;
                    if (team != null)
                    {
                        retList.Add(team);
                    }
                }
            }
        }

        public void CheckAllNodes(TreeNode topNode)
        {

            IList<TreeNode> nodes = new List<TreeNode>();

            CheckNodes(topNode, nodes);

            foreach (TreeNode node in nodes)
            {
                _userInvoke = node;
                PropagateCheck(node);
            } 
        }

        public void CheckNodes(TreeNode topNode, IList<TreeNode> nodes)
        {

            foreach (TreeNode node in topNode.Nodes)
            {
                CheckNodes(node, nodes);
                if (node.Checked)
                {
                    ITeam team = node.Tag as ITeam;
                    if (team != null)
                    {
                        nodes.Add(node);
                    }
                }
            }     
        }

        public void ChildNodes(TreeNode topNode, IList<TreeNode> nodeList)
        {
  
            foreach (TreeNode node in topNode.Nodes)
            {
                nodeList.Add(node);

                if (node.Nodes.Count > 0)
                {
                    ChildNodes(node, nodeList);
                }
            }
        }

        private void TraverseNodesAndMakeExpanded(TreeNode topNode)
        {
            if (!topNode.Checked)
            {
                foreach (TreeNode node in topNode.Nodes)
                {
                    TraverseNodesAndMakeExpanded(node);
                }
            }
            else
            {
                topNode.EnsureVisible();
            }
        }

        private void FilterDialog_Load(object sender, EventArgs e)
        {
            InitializeTreeView();
            BackColor = ColorHelper.FormBackgroundColor();
        }
    }
}
