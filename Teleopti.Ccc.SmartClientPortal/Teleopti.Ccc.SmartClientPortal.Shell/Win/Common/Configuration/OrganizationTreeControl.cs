using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
    public partial class OrganizationTreeControl : BaseUserControl, ISettingPage
    {
        private IList<ISite> _siteCollection;
        private IList<ITeam> _teamCollection;
        private SiteRepository _siteRepository;
        private IList<TreeNodeAdv> _nodeCollection;
        private TeamRepository _teamRepository;
        private const string orgBusinessUnit = "BU";
        private const string orgSite = "Site";
        private const string orgTeam = "Team";
        private IUnitOfWork _unitOfWork;
        private StringBuilder _errorMessage = new StringBuilder();
        private const int nameLength = 50;

        private string _newTeamName = string.Empty;
        private string _newSiteName = string.Empty;

        private const string newNameFormat = "<{0} {1}>";  // Format of the default scenario name.
        private const string lessThanChar = "<";                    // Character representing Less than sign.
        private const string greaterThanChar = ">";                 // Character representing Greater than sign.
        private const string spaceChar = " ";                       // Character representing <SPACE>.

        private readonly IList<IAggregateRoot> _newNodesCollection = new List<IAggregateRoot>();
        private readonly IList<IAggregateRoot> _removedNodesCollection = new List<IAggregateRoot>();
        private static readonly LocalizedUpdateInfo Localizer = new LocalizedUpdateInfo();

        public OrganizationTreeControl()
        {
            InitializeComponent();
        }

        private void loadSites()
        {
            _siteCollection = _siteRepository.LoadAll().ToList();
        }

        private void loadTeams()
        {
            _teamCollection = _teamRepository.LoadAll().ToList();
        }

        private void createTreeNodes()
        {
            _nodeCollection = new List<TreeNodeAdv>();

            //Create busienssunit node & add into the nodes.
            var bu = ((ITeleoptiIdentityWithUnsafeBusinessUnit)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity).BusinessUnit();
            
            var rootNode = new TreeNodeAdv(bu.Description.ToString())
                               {
                                   TagObject = bu,
                                   Tag = orgBusinessUnit,
                                   LeftImageIndices = new[] { 0 }
                               };
            _nodeCollection.Add(rootNode);

            //Create sites & add into root Node.
            foreach (var site in _siteCollection.OrderBy(s => s.Description.Name).ToList())
            {
                
                var siteNode = new TreeNodeAdv(site.Description.ToString())
                                   {
                                       TagObject = site,
                                       Tag = orgSite,
                                       HelpText = changedInfo((IChangeInfo)site),
                                       LeftImageIndices = new[] { 1 }
                                   };
                rootNode.Nodes.Add(siteNode);

                //Create teams & add into site node
                foreach (var team in site.TeamCollection.OrderBy(s => s.Description.Name))
                {
                    if (((IDeleteTag)team).IsDeleted) continue;
                    var teamNode = new TreeNodeAdv(team.Description.ToString())
                                    {
                                        TagObject = team,
                                        Tag = orgTeam,
                                        HelpText = changedInfo((IChangeInfo)team),
                                        LeftImageIndices = new[] { 2 }
                                    };
                    siteNode.Nodes.Add(teamNode);
                }
            }
        }

        private static string changedInfo(IChangeInfo entity)
        {
            return Localizer.UpdatedByText(entity, Resources.UpdatedByColon);
        }

        private void bindTreeNodeList()
        {
            treeViewAdvHierarchyTree.AfterSelect -= treeViewAdvHierarchyTreeAfterSelect;
            treeViewAdvHierarchyTree.BeginUpdate();

            treeViewAdvHierarchyTree.Nodes.Clear();

            foreach (var node in _nodeCollection)
            {
                treeViewAdvHierarchyTree.Nodes.Add(node);
            }
            treeViewAdvHierarchyTree.Nodes[0].Expand();

            treeViewAdvHierarchyTree.EndUpdate();
            //set default focus to bu.
            treeViewAdvHierarchyTree.SelectedNode = treeViewAdvHierarchyTree.Nodes[0];

            treeViewAdvHierarchyTree.AfterSelect += treeViewAdvHierarchyTreeAfterSelect;
        }

        private ITeam createTeam(ISite site)
        {
            var nextCount = getNextTeamId();

            // Formats the name.
            var name = string.Format(CultureInfo.InvariantCulture, newNameFormat, _newTeamName, nextCount);

            ITeam team = new Team();
			team.SetDescription(new Description(name));
            site.AddTeam(team);
            _teamRepository.Add(team);

            return team;
        }

        private static TreeNodeAdv addNode<T>(TreeNodeAdv parentNode, T node, string nodeDescription)
        {
            var newNode = new TreeNodeAdv(nodeDescription)
                              {
                                  TagObject = node,
                                  Tag = (typeof(T) == typeof(ITeam)) ? orgTeam : orgSite,
                                  HelpText = changedInfo((IChangeInfo)node),
                                  LeftImageIndices = new[] { (typeof(T) == typeof(ITeam)) ? 2 : 1 }
                              };

            parentNode.Nodes.Add(newNode);
            return newNode;
        }

        private int getNextTeamId()
        {
            var names = (from p in _teamCollection
                         where p.Description.Name.Contains(_newTeamName)
                         select p.Description.Name
                         .Replace(lessThanChar, string.Empty)
                         .Replace(greaterThanChar, string.Empty)
                         .Replace(_newTeamName, string.Empty)
                         .Replace(spaceChar, string.Empty)).ToList();

			var sortedArray = (from q in names
                               where string.IsNullOrEmpty(q) == false && int.TryParse(q, out _)
                               select Int32.Parse(q, CultureInfo.CurrentCulture)).ToArray();

            var nextId = 1;

            if (!sortedArray.IsEmpty())
            {
                Array.Sort(sortedArray);

                // Adds 1 to last number.
                nextId = sortedArray[(sortedArray.Length - 1)] + 1;
            }
            return nextId;
        }

        private ISite createSite()
        {
            var nextCount = getNextSiteId();

            // Formats the name.
            var name = string.Format(CultureInfo.InvariantCulture, newNameFormat, _newSiteName, nextCount);

            ISite site = new Site(name);
            _siteRepository.Add(site);
            return site;
        }

        private void deleteSite(TreeNodeAdv node)
        {
            var site = (ISite)node.TagObject;

            IList<ITeam> teamCollection = new List<ITeam>(site.TeamCollection.Where(t => t.IsChoosable));
            if (teamCollection.Count == 0)
            {
                _siteCollection.Remove(site);
                _removedNodesCollection.Add(site);
                _siteRepository.Remove(site);
                node.Remove();
            }
            else
            {
                if (string.IsNullOrEmpty(_errorMessage.ToString()))
                    _errorMessage.Append(Resources.CannotDeleteSiteColon + site.Description + ". " + Resources.ThereAreTeamsAssignedUnderneath);
                else
                    _errorMessage.Append("\n" + Resources.CannotDeleteSiteColon + site.Description + ". " + Resources.ThereAreTeamsAssignedUnderneath);
            }
        }

        private void deleteTeam(TreeNodeAdv node)
        {
            var team = (ITeam)node.TagObject;
            //min date 1/1/1753-
            var period = DateOnly.Today.ToDateOnlyPeriod();
            var personRep = PersonRepository.DONT_USE_CTOR(new ThisUnitOfWork(_unitOfWork), null, null);

            if (personRep.FindPeopleBelongTeam(team, period).Count == 0)
            {
                _teamCollection.Remove(team);
                _removedNodesCollection.Add(team);
                _teamRepository.Remove(team);
                node.Remove();

            }
            else
            {
                if (string.IsNullOrEmpty(_errorMessage.ToString()))
                    _errorMessage.Append(Resources.CannotDeleteTeamColon + team.Description + ". " + Resources.ThereAreAgentsAssignedToTheTeam);
                else
                    _errorMessage.Append("\n" + Resources.CannotDeleteTeamColon + team.Description + ". " + Resources.ThereAreAgentsAssignedToTheTeam);
            }


        }

        private int getNextSiteId()
        {
            var names = (from p in _siteCollection
                         where p.Description.Name.Contains(_newSiteName)
                         select p.Description.Name
                         .Replace(lessThanChar, string.Empty)
                         .Replace(greaterThanChar, string.Empty)
                         .Replace(_newSiteName, string.Empty)
                         .Replace(spaceChar, string.Empty)).ToList();

			var sortedArray = (from q in names
                               where string.IsNullOrEmpty(q) == false && int.TryParse(q, out _)
                               select Int32.Parse(q, CultureInfo.CurrentCulture)).ToArray();
            var nextId = 1;

            if (!sortedArray.IsEmpty())
            {
                Array.Sort(sortedArray);

                // Adds 1 to last number.
                nextId = sortedArray[(sortedArray.Length - 1)] + 1;
            }
            return nextId;
        }

        private void addNewNode()
        {
            treeViewAdvHierarchyTree.BeginUpdate();
            var selectedNodes = (SelectedNodesCollection)treeViewAdvHierarchyTree.SelectedNodes.Clone();
            var currentSelectedNode = treeViewAdvHierarchyTree.SelectedNode;
            foreach (TreeNodeAdv node in selectedNodes)
            {
                switch (node.Tag.ToString())
                {
                    case orgBusinessUnit:
                        currentSelectedNode = getNewSiteNode(node);
                        break;
                    case orgSite:
                        currentSelectedNode = getNewTeamNode(node);
                        break;
                    default:
                        break;
                }

            }
            treeViewAdvHierarchyTree.EndUpdate();

            //set selected node to be in edit mode.
            treeViewAdvHierarchyTree.SelectedNode = currentSelectedNode;
            treeViewAdvHierarchyTree.BeginEdit(treeViewAdvHierarchyTree.SelectedNode);
        }

        private TreeNodeAdv getNewTeamNode(TreeNodeAdv node)
        {
            var team = createTeam((ISite)node.TagObject);
            var currentSelectedNode = addNode(node, team, team.Description.Name);
            _teamCollection.Add(team);
            _newNodesCollection.Add(team);
            return currentSelectedNode;
        }

        private TreeNodeAdv getNewSiteNode(TreeNodeAdv node)
        {
            var site = createSite();
            var currentSelectedNode = addNode(node, site, site.Description.Name);
            _siteCollection.Add(site);
            _newNodesCollection.Add(site);
            return currentSelectedNode;
        }

        private void deleteNode()
        {
            treeViewAdvHierarchyTree.BeginUpdate();
            var selectedNodes = (SelectedNodesCollection)treeViewAdvHierarchyTree.SelectedNodes.Clone();

            foreach (TreeNodeAdv node in selectedNodes)
            {
                switch (node.Tag.ToString())
                {
                    case orgSite:
                        deleteSite(node);
                        break;
                    case orgTeam:
                        deleteTeam(node);
                        break;
                    default:
                        break;
                }
            }
            treeViewAdvHierarchyTree.EndUpdate();
        }

        private void renameNodeText(TreeNodeAdv node)
        {
            if (string.IsNullOrWhiteSpace(node.Text))
            {
                ViewBase.ShowErrorMessage(Resources.NodeTextCantBeNulls, Resources.WarningMessageTitle);
                treeViewAdvHierarchyTree.BeginEdit(treeViewAdvHierarchyTree.SelectedNode);
                return;
            }

            if (node.Text.Length > nameLength)
            {
                ViewBase.ShowErrorMessage(Resources.TextTooLong, Resources.WarningMessageTitle);
                treeViewAdvHierarchyTree.BeginEdit(treeViewAdvHierarchyTree.SelectedNode);
                return;
            }

            if (node.Tag.ToString() == orgTeam) ((ITeam)node.TagObject).SetDescription(new Description(node.Text));
            else if (node.Tag.ToString() == orgSite) ((ISite)node.TagObject).SetDescription(new Description(node.Text));
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            toolTip1.SetToolTip(buttonDelete, Resources.Delete);
            toolTip1.SetToolTip(buttonNew, Resources.AddNewSite);
            _newTeamName = Resources.Team;
            _newSiteName = Resources.Site;
        }

        private void TreeViewAdvHierarchyTreeBeforeEdit(object sender, TreeNodeAdvBeforeEditEventArgs e)
        {
            if (e.Node.Tag.ToString() == orgBusinessUnit) e.Cancel = true;

        }

        private void treeViewAdvHierarchyTreeNodeEditorValidated(object sender, TreeNodeAdvEditEventArgs e)
        {
            renameNodeText(e.Node);
        }

        private void treeViewAdvHierarchyTreeAfterSelect(object sender, EventArgs e)
        {
	        var selectedNode = treeViewAdvHierarchyTree.SelectedNode;
	        if (selectedNode == null) return;

			switch (selectedNode.Tag.ToString())
            {
                case orgBusinessUnit:
                    buttonNew.Enabled = true;
                    contextMenuStrip.Items[0].Enabled = true;
                    toolTip1.SetToolTip(buttonNew, Resources.AddNewSite);
                    break;
                case orgSite:
                    buttonNew.Enabled = true;
                    contextMenuStrip.Items[0].Enabled = true;
                    toolTip1.SetToolTip(buttonNew, Resources.AddNewTeam);
                    break;
                default:
                    buttonNew.Enabled = false;
                    contextMenuStrip.Items[0].Enabled = false;
                    break;
            }
        }

        private void buttonNewClick(object sender, EventArgs e)
        {
            addNewNode();
        }

        private void buttonDeleteClick(object sender, EventArgs e)
        {
            string text = string.Format(
                CurrentCulture,
                Resources.AreYouSureYouWantToDelete);

            string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);
            DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
            if (response != DialogResult.Yes) return;

            deleteNode();

            if (!string.IsNullOrEmpty(_errorMessage.ToString()))
                ViewBase.ShowErrorMessage(_errorMessage.ToString(), Resources.WarningMessageTitle);
            _errorMessage = new StringBuilder();
        }

        private void toolStripMenuItemAddNewClick(object sender, EventArgs e)
        {
            addNewNode();
        }

        private void toolStripMenuItemDeleteClick(object sender, EventArgs e)
        {
            deleteNode();

            if (!string.IsNullOrEmpty(_errorMessage.ToString()))
                ViewBase.ShowErrorMessage(_errorMessage.ToString(), Resources.WarningMessageTitle);
            _errorMessage = new StringBuilder();
        }

        private void toolStripMenuItemRenameClick(object sender, EventArgs e)
        {
            treeViewAdvHierarchyTree.BeginEdit(treeViewAdvHierarchyTree.SelectedNode);
        }

        public void InitializeDialogControl()
        {
            setColors();
            SetTexts();
        }

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

            gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
            labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

            tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            treeViewAdvHierarchyTree.BackColor = ColorHelper.GridControlGridInteriorColor();
        }

        public void LoadControl()
        {
            if (DesignMode) return;
            loadSites();
            loadTeams();
            createTreeNodes();
            bindTreeNodeList();
        }

        public void SaveChanges()
        {

            var identity = ((ITeleoptiIdentityWithUnsafeBusinessUnit)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity);
            foreach (IAggregateRoot aggregateRoot in _newNodesCollection)
            {
                var theSite = aggregateRoot as ISite;
                if (theSite != null)
                    identity.BusinessUnit().AddSite(theSite);

                var theTeam = aggregateRoot as ITeam;
                if (theTeam == null) continue;
                var teamSite = identity.BusinessUnit().SiteCollection.
                    FirstOrDefault(s => s.Equals(theTeam.Site));
                if (teamSite != null) teamSite.AddTeam(theTeam);
            }

            foreach (IAggregateRoot aggregateRoot in _removedNodesCollection)
            {
                var theSite = aggregateRoot as ISite;
                if (theSite != null)
                    identity.BusinessUnit().RemoveSite(theSite);

                var theTeam = aggregateRoot as ITeam;
                if (theTeam == null) continue;
                var teamSite = identity.BusinessUnit().SiteCollection.
                    FirstOrDefault(s => s.Equals(theTeam.Site));
                teamSite.RemoveTeam(theTeam);
            }

            _newNodesCollection.Clear();
            _removedNodesCollection.Clear();
        }

        public TreeFamily TreeFamily()
        {
            return new TreeFamily(Resources.OrganizationHierarchy);
        }

        public string TreeNode()
        {
            return Resources.DefineOrganization;
        }

        public void OnShow()
        { }

        public void Unload()
        {
        }

        public void SetUnitOfWork(IUnitOfWork value)
        {
            _unitOfWork = value;

            _siteRepository = SiteRepository.DONT_USE_CTOR(_unitOfWork);
            _teamRepository = TeamRepository.DONT_USE_CTOR(_unitOfWork);
        }

        public void Persist()
        {
        }

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
        }

        public ViewType ViewType
        {
            get { return ViewType.OrganizationTree; }
        }
    }
}
