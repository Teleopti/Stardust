using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Repositories;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Grouping
{
    /// <summary>
    /// Created Business Hierarchy.
    /// </summary>
    public class BusinessHierarchyCreator
    {
        private readonly IGroupPageDataProvider _groupPageDataProvider;
        private ITeam _virtualTeam;
        private List<IPerson> _personList;
        private IEnumerable<IPerson> _selectedPersons;
        private bool _usesCheckBoxes;
        private CommonNameDescriptionSetting _commonNameDescriptionSetting;

        public BusinessHierarchyCreator(IGroupPageDataProvider groupPageDataProvider)
        {
            _groupPageDataProvider = groupPageDataProvider;
        }

        /// <summary>
        /// Creates the tree nodes.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        /// <param name="selectedPersons">The selected persons.</param>
        /// <param name="usesCheckBoxes">if set to <c>true</c> [uses checkboxes].</param>
        /// <param name="hideUsers">if set to <c>true</c> [hide users].</param>
        /// <param name="hideAgents">if set to <c>true</c> [hide agents].</param>
        /// <param name="applicationFunction">The application function.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Updated by: tamasb
        /// Created date: 2008-07-05
        /// </remarks>
        public ITreeItem<TreeNodeAdv> LoadBusinessUnitStructure(IBusinessUnit businessUnit, IEnumerable<IPerson> selectedPersons, bool usesCheckBoxes, bool hideUsers, bool hideAgents, IApplicationFunction applicationFunction)
        {
            _personList = new List<IPerson>(_groupPageDataProvider.PersonCollection);
            _selectedPersons = selectedPersons;
            _usesCheckBoxes = usesCheckBoxes;

            var period = _groupPageDataProvider.SelectedPeriod;

            BusinessUnitListBoxPresenter buPresenter = new BusinessUnitListBoxPresenter(businessUnit);
            ITreeItem<TreeNodeAdv> rootItem = CreateTreeItem(buPresenter);
            rootItem.Data = businessUnit;
            rootItem.StoredDataDictionary["Grouping"] = GroupingConstants.NodeTypeGroup;

            var functionPath = applicationFunction.FunctionPath;
            try
            {
                if (!hideUsers)
                {
                    ITreeItem<TreeNodeAdv> usersItem = CreateVirtualSiteWithPersonsWhoAreUsers(functionPath, period.StartDate);
                    usersItem.IsPermitted =
                        PrincipalAuthorization.Instance().EvaluateSpecification(new AllowedToSeeUsersNotInOrganizationSpecification(functionPath));
                    rootItem.AddChild(usersItem);
                }
                foreach (ISite site in NotDeletedSitesInAlphabeticOrderInBusinessUnit(businessUnit))
                {
                    ITreeItem<TreeNodeAdv> siteItem =
                        GetSiteListBoxPresenterItem(rootItem, functionPath, site, period.StartDate);

                    foreach (ITeam team in NotDeletedTeamsInAlphabeticOrderInSite(site))
                    {
                        ITreeItem<TreeNodeAdv> teamItem =
                            GetTeamListBoxPresenterItem(functionPath, siteItem, team, period.StartDate);

                        if (!hideAgents)
                            CreateTreeNodesPerson(team, teamItem, functionPath, period);
                    }
                }
            }
            finally
            {
            }
            return rootItem;
        }

        /// <summary>
        /// Gets the not deleted sites in alphabetic order in <paramref name="businessUnit"/> parameter.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        /// <returns></returns>
        private static IEnumerable<ISite> NotDeletedSitesInAlphabeticOrderInBusinessUnit(IBusinessUnit businessUnit)
        {
            foreach (ISite site in businessUnit.SiteCollection.OrderBy(s => s.Description.Name))
            {
                //Ignore deleted sites.
                if (!((IDeleteTag)site).IsDeleted)
                    yield return site;
            }
        }

        /// <summary>
        /// Gets the not deleted teams in alphabetic order in the <paramref name="site"/> parameter.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns></returns>
        private static IEnumerable<ITeam> NotDeletedTeamsInAlphabeticOrderInSite(ISite site)
        {
            foreach (ITeam team in site.TeamCollection.OrderBy(t => t.Description.Name))
            {
                //Ignore deleted sites.
                if (team.IsChoosable)
                    yield return team;
            }
        }

        /// <summary>
        /// Create nodes for persons
        /// </summary>
        private void CreateTreeNodesPerson(ITeam team, ITreeItem<TreeNodeAdv> teamItem, string functionPath, DateOnlyPeriod period)
        {
            _commonNameDescriptionSetting = GetCommonNameDescription();
            IList<IPerson> personInTeamCollection = _personList.FindAll(new PersonBelongsToTeamSpecification(period,team).IsSatisfiedBy);
            foreach (IPerson person in personInTeamCollection.OrderBy(t => _commonNameDescriptionSetting.BuildCommonNameDescription(t)))
            {
                CreatePersonListBoxPresenter(functionPath, teamItem, person, period.StartDate);
            }
        }

        private static ITreeItem<TreeNodeAdv> GetSiteListBoxPresenterItem(ITreeItem<TreeNodeAdv> rootItem,
            string functionPath, ISite site, DateOnly dateOnly)
        {
            SiteListBoxPresenter sitePresenter = new SiteListBoxPresenter(site);
            ITreeItem<TreeNodeAdv> siteItem = CreateTreeItem(sitePresenter);
            rootItem.AddChild(siteItem);
            siteItem.IsPermitted = PrincipalAuthorization.Instance().IsPermitted(functionPath, dateOnly, site);
            siteItem.ImageIndex = 1;
            siteItem.StoredDataDictionary["Grouping"] = GroupingConstants.NodeTypeGroup;
            return siteItem;
        }

        private static ITreeItem<TreeNodeAdv> GetTeamListBoxPresenterItem(string functionPath,
            ITreeItem<TreeNodeAdv> siteItem, ITeam team, DateOnly dateOnly)
        {
            TeamListBoxPresenter teamPresenter = new TeamListBoxPresenter(team);
            ITreeItem<TreeNodeAdv> teamItem = CreateTreeItem(teamPresenter);
            teamItem.IsPermitted = PrincipalAuthorization.Instance().IsPermitted(functionPath, dateOnly, team);
            teamItem.ImageIndex = 2;
            teamItem.StoredDataDictionary["Grouping"] = GroupingConstants.NodeTypeGroup;

            siteItem.AddChild(teamItem);
            return teamItem;
        }

        private void CreatePersonListBoxPresenter(string functionPath,
            ITreeItem<TreeNodeAdv> teamItem, IPerson person, DateOnly dateOnly)
        {
            PersonListBoxPresenter personPresenter = new PersonListBoxPresenter(person, GetCommonNameDescription());
            ITreeItem<TreeNodeAdv> personItem = CreateTreeItem(personPresenter);
            personItem.IsPermitted = PrincipalAuthorization.Instance().IsPermitted(functionPath, dateOnly, person);
            personItem.ImageIndex = 3;
            personItem.StoredDataDictionary["Grouping"] = GroupingConstants.NodeTypePerson;
            if (_usesCheckBoxes)
            {
                if (_selectedPersons.Contains(person))
                {
                    personItem.Selected = true;
                }
            }
            teamItem.AddChild(personItem);
        }

        private ITreeItem<TreeNodeAdv> CreateVirtualSiteWithPersonsWhoAreUsers(string functionPath, DateOnly dateOnly)
        {
            if (_virtualTeam == null)
            {
                _virtualTeam = new Team();
                _virtualTeam.Description = new Description(UserTexts.Resources.NotInHierarchy);
                _virtualTeam.SetId(Guid.NewGuid());
            }
            //Create NotInHierarchy team
            TeamListBoxPresenter teamPresenter = new TeamListBoxPresenter(_virtualTeam);
            ITreeItem<TreeNodeAdv> virtualTeamItem = CreateTreeItem(teamPresenter);
            virtualTeamItem.ImageIndex = 1;
            virtualTeamItem.StoredDataDictionary["Grouping"] = GroupingConstants.NodeTypeGroup;

            ICollection<IPerson> personsWhoAreUsers = FetchPersonsWhoAreUsers();

            //Add non agents to the above created node. 
            foreach (IPerson user in personsWhoAreUsers)
            {
                PersonListBoxPresenter personPresenter = new PersonListBoxPresenter(user, GetCommonNameDescription());
                ITreeItem<TreeNodeAdv> personItem = CreateTreeItem(personPresenter);
                personItem.IsPermitted = PrincipalAuthorization.Instance().IsPermitted(functionPath, dateOnly, user);
                personItem.ImageIndex = 3;
                personItem.StoredDataDictionary["Grouping"] = GroupingConstants.NodeTypePerson;
                virtualTeamItem.AddChild(personItem);
            }
            return virtualTeamItem;
        }

        /// <summary>
        /// Fetches the persons who are users.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Tamas Balog
        /// Created date: 2009-06-03
        /// </remarks>
        private IList<IPerson> FetchPersonsWhoAreUsers()
        {
            return _personList.FindAll(new PersonIsUserSpecification(_groupPageDataProvider.SelectedPeriod).IsSatisfiedBy);
        }

        /// <summary>
        /// Creates the tree item.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-10-26
        /// </remarks>
        private static TreeItem<TreeNodeAdv> CreateTreeItem(IEntity entity)
        {
            IListBoxPresenter presenter = entity as IListBoxPresenter;
            if (presenter != null)
            {
                return new TreeItem<TreeNodeAdv>(presenter.Id.ToString(), presenter.DataBindDescriptionText, presenter.DataBindText, presenter);
            }
            TreeItem<TreeNodeAdv> ret = new TreeItem<TreeNodeAdv>(entity.Id.ToString());
            ret.Data = entity;

            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        private CommonNameDescriptionSetting GetCommonNameDescription()
        {
            if (_commonNameDescriptionSetting == null)
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    ISettingDataRepository settingDataRepository = new GlobalSettingDataRepository(uow);
                    _commonNameDescriptionSetting = settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
                } 
            }
            return _commonNameDescriptionSetting;
        }
    }
}