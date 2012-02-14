using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Common.ControlBinders;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Controls
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class BusinessUnitHierarchyPanelAdv : BaseUserControlWithUnitOfWork, ISelfDataHandling
    {
        private TreeViewAdvBinder _treeViewBinder;
        private ITreeItem<TreeNodeAdv> _rootItem;
        private List<IPerson> _allPersons = new List<IPerson>();
        private ITeam _virtualTeam;

        private bool _showPersons = true;
        private bool _showPersonsNotInHierarchy = true;
        private bool _showTeamsWithNoPersons = true;
        private bool _loading = true;
        private CommonNameDescriptionSetting _commonNameDescriptionSetting;

        private IApplicationFunction _applicationFunction =
            Domain.Security.AuthorizationEntities.ApplicationFunction.FindByPath(DefinedRaptorApplicationFunctionFactory.ApplicationFunctionList,
                DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication);

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessUnitHierarchyPanelAdv"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-02
        /// </remarks>
        public BusinessUnitHierarchyPanelAdv()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            xtreeBUStructure.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Gets the business unit structure.
        /// </summary>
        /// <value>The business unit structure.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-20
        /// </remarks>
        public ITreeItem<TreeNodeAdv> BusinessUnitStructureTreeItems
        {
            get { return _rootItem; }
        }

        /// <summary>
        /// Gets or sets the current fetch persons.
        /// </summary>
        /// <value>The current fetch persons.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-06
        /// </remarks>
        public ICollection<IPerson> CurrentFetchPersons
        {
            get { return _currentFetchPersons; }
        }

        /// <summary>
        /// Sets the unit of work.
        /// Typically calls for the ChangeObjectsUnitOfWork method in base class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-03
        /// </remarks>
        public void SetUnitOfWork(IUnitOfWork value)
        {
            ChangeObjectsUnitOfWork(this, value);
        }

        /// <summary>
        /// Persists the data instance.
        /// </summary>
        public void Persist()
        {
            // do nothing  
        }

        /// <summary>
        /// Validates the user edited data in control.
        /// </summary>
        /// <param name="direction">The direction  of dataflow.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        public bool ValidateData(ExchangeDataOption direction)
        {
            return true;
        }

        /// <summary>
        /// Exchances the data between the controls and the datasource object.
        /// </summary>
        /// <param name="direction">The direction of dataflow.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        public void ExchangeData(ExchangeDataOption direction)
        {
            if (direction == ExchangeDataOption.ServerToClient)
            {
                LoadSettings();
                xdtpDate.Value = DateTime.Now;
                DisplayBusinessUnitStructure();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to load persons in the hierarchy.
        /// </summary>
        /// <value><c>true</c> if [load persons]; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        [Description("Gets or sets a value indicating whether to show persons in the hierarchy.")]
        public bool ShowPersons
        {
            get { return _showPersons; }
            set { _showPersons = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show persons who are not agents in the hierarchy.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if show persons not in hierarchy; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(true)]
        [Description("Gets or sets a value indicating whether to show persons who are not agents in the hierarchy.")]
        public bool ShowPersonsNotInHierarchy
        {
            get { return _showPersonsNotInHierarchy; }
            set { _showPersonsNotInHierarchy = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show teams which has no persons assigned to.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show persons not in hierarchy]; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(true)]
        [Description("Gets or sets a value indicating whether to show teams which has no persons assigned to.")]
        public bool ShowTeamsWithNoPersons
        {
            get { return _showTeamsWithNoPersons; }
            set { _showTeamsWithNoPersons = value; }
        }

        /// <summary>
        /// Gets all persons.
        /// </summary>
        /// <value>The get all persons.</value>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ReadOnlyCollection<IPerson> GetAllPersons
        {
            get { return new ReadOnlyCollection<IPerson>(_allPersons); }
        }

        ///// <summary>
        ///// Gets the selected persons.
        ///// </summary>
        ///// <value>The selected persons.</value>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IList<IPerson> SelectedPersons
        {
            get { return GetSelectedItems<IPerson>(); }
        }

        /// <summary>
        /// Gets the selected tree nodes.
        /// </summary>
        /// <value>The selected tree nodes.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-03-17
        /// </remarks>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IList<ITreeItem<TreeNodeAdv>> SelectedItemCollection
        {
            get { return _treeViewBinder.SelectedItemCollection; }
        }

        /// <summary>
        /// Sets the selected tree nodes.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetTreeSelectedNodes(IList<ITreeItem<TreeNodeAdv>> selectedItems)
        {
            _treeViewBinder.SetTreeSelectedNodes(selectedItems);
        }

        /// <summary>
        /// Gets or sets the application function.
        /// </summary>
        /// <value>The application function.</value>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IApplicationFunction ApplicationFunction
        {
            get { return _applicationFunction; }
            set { _applicationFunction = value; }
        }

        /// <summary>
        /// Loads the persons structure.
        /// </summary>
        private ITreeItem<TreeNodeAdv> LoadBusinessUnitStructure()
        {
            DateTime utcDate = TimeZoneHelper.ConvertToUtc(xdtpDate.Value);
            IBusinessUnit bu = StateHolderReader.Instance.StateReader.SessionScopeData.BusinessUnit;
            BusinessUnitListBoxPresenter buPresenter = new BusinessUnitListBoxPresenter(bu);
            ITreeItem<TreeNodeAdv> rootItem = CreateTreeItem(buPresenter);
            rootItem.ImageIndex = 0;
            IAuthorizationService authorizationService = StateHolderReader.Instance.StateReader.SessionScopeData.AuthorizationService;

            try
            {
                authorizationService.StartBunchQueries();

                _allPersons = new List<IPerson>(FetchAllPersons());

                if (ShowPersons && ShowPersonsNotInHierarchy)
                {
                    ITreeItem<TreeNodeAdv> allPersonsItem = CreateVirtualSiteWithPersonsWhoAreUsers();
                    allPersonsItem.IsPermitted =
                        authorizationService.AggregatedPermittedData(_applicationFunction).PermissionToPersonNotPartOfOrganization.Value;
                    rootItem.AddChild(allPersonsItem);
                }

                foreach (Site site in bu.SiteCollection)
                {
                    SiteListBoxPresenter sitePresenter = new SiteListBoxPresenter(site);
                    ITreeItem<TreeNodeAdv> siteItem = CreateTreeItem(sitePresenter);
                    rootItem.AddChild(siteItem);
                    siteItem.IsPermitted = site.IsPermitted(_applicationFunction, utcDate);
                    siteItem.ImageIndex = 1;
                    foreach (Team team in site.TeamCollection)
                    {
                        TeamListBoxPresenter teamPresenter = new TeamListBoxPresenter(team);
                        ITreeItem<TreeNodeAdv> teamItem = CreateTreeItem(teamPresenter);
                        teamItem.IsPermitted = team.IsPermitted(_applicationFunction, utcDate);
                        teamItem.ImageIndex = 2;
                        siteItem.AddChild(teamItem);

                        if (ShowPersons)
                        {
                            IList<IPerson> personInTeamCollection =
                                _allPersons.FindAll(
                                    new PersonBelongsToTeamSpecification(
                                        new DateTimePeriod(TimeZoneInfo.ConvertTimeToUtc(xdtpDate.Value),
                                                           TimeZoneInfo.ConvertTimeToUtc(xdtpDate.Value.AddDays(2))), team)
                                        .IsSatisfiedBy);

                            foreach (IPerson person in personInTeamCollection)
                            {
                                PersonListBoxPresenter personPresenter = new PersonListBoxPresenter(person, _commonNameDescriptionSetting);
                                ITreeItem<TreeNodeAdv> personItem = CreateTreeItem(personPresenter);
                                personItem.IsPermitted = person.IsPermitted(_applicationFunction, utcDate);
                                personItem.ImageIndex = 3;
                                teamItem.AddChild(personItem);
                            }
                        }
                    }
                }
            }
            finally
            {
                authorizationService.EndBunchQueries();
            }
            return rootItem;
        }

        
        /// <summary>
        /// Creates a virtual site with persons not member of hierarchy.
        /// </summary>
        /// <returns></returns>
        private ITreeItem<TreeNodeAdv> CreateVirtualSiteWithPersonsWhoAreUsers()
        {
            if (_virtualTeam == null)
            {
                _virtualTeam = new Team();
                _virtualTeam.Description = new Description(UserTexts.Resources.NotInHierarchy); //xx NotInHierarchy
                _virtualTeam.SetId(Guid.NewGuid());
            }
            TeamListBoxPresenter teamPresenter = new TeamListBoxPresenter(_virtualTeam);
            ITreeItem<TreeNodeAdv> virtualTeamItem = CreateTreeItem(teamPresenter);
            virtualTeamItem.ImageIndex = 1;

            ICollection<IPerson> personsWhoAreUsers = FetchPersonsWhoAreUsers(new DateOnly(xdtpDate.Value));
           
            foreach (IPerson user in personsWhoAreUsers)
            {
                PersonListBoxPresenter personPresenter = new PersonListBoxPresenter(user, _commonNameDescriptionSetting);
                ITreeItem<TreeNodeAdv> personItem = CreateTreeItem(personPresenter);
                personItem.IsPermitted = user.IsPermitted(_applicationFunction, TimeZoneHelper.ConvertToUtc(xdtpDate.Value));
                personItem.ImageIndex = 3;
                virtualTeamItem.AddChild(personItem);
            }

            return virtualTeamItem;
       }

        /// <summary>
        /// Displays the persons structure.
        /// </summary>
        private void DisplayBusinessUnitStructure()
        {
            ITreeItem<TreeNodeAdv> rootItem = LoadBusinessUnitStructure();
            _rootItem = TreeViewAdvBinder.SynchronizeDisplayInformation(_rootItem, rootItem);
            _treeViewBinder = new TreeViewAdvBinder(xtreeBUStructure, _rootItem);
            _treeViewBinder.Display(1);
        }

        private ICollection<IPerson> _currentFetchPersons;

        public void SetCurrentFetchPersons(ICollection<IPerson> currentFetchPersons)
        {
            _currentFetchPersons = currentFetchPersons;
        }

        /// <summary>
        /// Fetches all the persons.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/5/2007
        /// </remarks>
        private ICollection<IPerson> FetchAllPersons()
        {
            ICollection<IPerson> personList;
            if (CurrentFetchPersons == null)
            {
                IPersonRepository rep = new PersonRepository(UnitOfWork);
                personList = rep.LoadAllPeopleWithHierarchyDataSortByName();
            }
            else personList = CurrentFetchPersons;

            return personList;
        }

        /// <summary>
        /// Fetches the persons who are users.
        /// </summary>
        /// <param name="utcDate">The UTC date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Tamas Balog
        /// Created date: 2009-06-03
        /// </remarks>
        private IList<IPerson> FetchPersonsWhoAreUsers(DateOnly utcDate)
        {
            List<IPerson> allPersons = new List<IPerson>(FetchAllPersons());
            return allPersons.FindAll(new PersonIsUserSpecification(utcDate).IsSatisfiedBy);
        }


        private static TreeItem<TreeNodeAdv> CreateTreeItem(IEntity entity)
        {
            IListBoxPresenter presenter = entity as IListBoxPresenter;
            if (presenter != null)
                return new TreeItem<TreeNodeAdv>(presenter.Id.ToString(), presenter.DataBindDescriptionText, presenter.DataBindText, presenter);
            TreeItem<TreeNodeAdv> ret = new TreeItem<TreeNodeAdv>(entity.Id.ToString());
            ret.Data = entity;
            return ret;
        }

        /// <summary>
        /// Gets the checked items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-06
        /// </remarks>
        private IList<T> GetSelectedItems<T>()
        {
            IList<T> retCollection = new List<T>();
            ReadOnlyCollection<ITreeItem<TreeNodeAdv>> checkedItems = _treeViewBinder.SelectedItemCollection;
            foreach (ITreeItem<TreeNodeAdv> item in checkedItems)
            {
                if (item.Data != null && item.Data is IListBoxPresenter)
                {
                    try
                    {
                        retCollection.Add((T)((IListBoxPresenter)item.Data).EntityObject);
                    }
                    catch (InvalidCastException)
                    { }
                }
            }
            return retCollection;
        }

        public void LoadSettings()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ISettingDataRepository settingDataRepository = new GlobalSettingDataRepository(uow);

                _commonNameDescriptionSetting =  settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
            }
        }

        private void xdtpDate_ValueChanged(object sender, EventArgs e)
        {
            if (!_loading) ExchangeData(ExchangeDataOption.ServerToClient);
            else _loading = false;
        }

        public event EventHandler OnTreeViewClicked;

        private void xtreeBUStructure_MouseClick(object sender, MouseEventArgs e)
        {
            if (OnTreeViewClicked != null) OnTreeViewClicked(sender, e);
        }
    }
}
