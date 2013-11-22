using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    public class AvailableData : VersionedAggregateRoot, IAvailableData, IDeleteTag
    {
        #region Variables

        private IApplicationRole _applicationRole;
        private readonly ICollection<IBusinessUnit> _availableBusinessUnits;
        private readonly ICollection<ISite> _availableSites;
        private readonly ICollection<ITeam> _availableTeams;
        private readonly ICollection<IPerson> _availablePersons;
        private AvailableDataRangeOption _availableDataRange;
        private bool? _permissionToPersonNotPartOfOrganization;
        private bool _isDeleted;

        #endregion

        #region Constructor

        /// <summary>               
        /// Initializes a new instance of the <see cref="AvailableData"/> class.
        /// </summary>
        public AvailableData()
        {
            _availablePersons = new List<IPerson>();
            _availableTeams = new List<ITeam>();
            _availableSites = new List<ISite>();
            _availableBusinessUnits = new List<IBusinessUnit>();
        }

        #endregion

        #region Interface

        #region Static

        /// <summary>
        /// Finds the available data in the available data list by the contained application role.
        /// </summary>
        /// <param name="availableDataList">The available data list.</param>
        /// <param name="applicationRole">The application role.</param>
        /// <returns></returns>
        public static IAvailableData FindByApplicationRole(IEnumerable<IAvailableData> availableDataList, IApplicationRole applicationRole)
        {
            foreach (IAvailableData availableData in availableDataList)
            {
                if (availableData.ApplicationRole.Equals(applicationRole))
                    return availableData;
            }
            return null;
        }

        /// <summary>
        /// Combines the available datas in the list and returns the combined list.
        /// </summary>
        /// <param name="collection">The list.</param>
        /// <returns></returns>
        public static IAvailableData CombineAvailableData(IEnumerable<IAvailableData> collection)
        {
            InParameter.NotNull("collection", collection);
            List<IAvailableData> availableDataWorkingList = new List<IAvailableData>(collection);
            IAvailableData result = new AvailableData();
            if (availableDataWorkingList.Count == 0)
            {
                return result;
            }
            for (int counter = 0; counter < availableDataWorkingList.Count; counter++)
            {
                IAvailableData currentAvailableDataItem = availableDataWorkingList[counter];
                foreach (IBusinessUnit unit in currentAvailableDataItem.AvailableBusinessUnits)
                {
                    result.AddAvailableBusinessUnit(unit);
                }
                foreach (ISite site in currentAvailableDataItem.AvailableSites)
                {
                    result.AddAvailableSite(site);
                }
                foreach (ITeam team in currentAvailableDataItem.AvailableTeams)
                {
                    result.AddAvailableTeam(team);
                }
                foreach (IPerson person in currentAvailableDataItem.AvailablePersons)
                {
                    result.AddAvailablePerson(person);
                }
                if (currentAvailableDataItem.PermissionToPersonNotPartOfOrganization.HasValue || result.PermissionToPersonNotPartOfOrganization.HasValue)
                {
                    result.PermissionToPersonNotPartOfOrganization =
                        (result.PermissionToPersonNotPartOfOrganization ?? false) || (currentAvailableDataItem.PermissionToPersonNotPartOfOrganization ?? false);
                }
                result.AvailableDataRange = (AvailableDataRangeOption)Math.Max((int)result.AvailableDataRange, (int)currentAvailableDataItem.AvailableDataRange);
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Converts the permitted data lists (AvailableBusinessUnit, AvailableSites, AvailableTeams) to team collection.
        /// </summary>
        /// <returns></returns>
        public virtual IList<ITeam> ConvertToPermittedTeamCollection()
        {
            IList<ITeam> returnList = new List<ITeam>();

            foreach (IBusinessUnit businessUnit in AvailableBusinessUnits)
            {
                foreach (ISite site in businessUnit.SiteCollection)
                {
                    foreach (ITeam team in site.TeamCollection)
                    {
                        //if(!returnList.Contains(team))
                            returnList.Add(team);
                    }
                }
            }
            foreach (ISite site in AvailableSites)
            {
                foreach (ITeam team in site.TeamCollection)
                {
                    //if (!returnList.Contains(team))
                        returnList.Add(team);
                }
            }
            foreach (ITeam team in AvailableTeams)
            {
                //if (!returnList.Contains(team))
                    returnList.Add(team);
            }
            return returnList;
        }

        /// <summary>
        /// Converts the permitted data lists (AvailableBusinessUnit, AvailableSites) to site collection.
        /// </summary>
        /// <returns></returns>
        public virtual IList<ISite> ConvertToPermittedSiteCollection()
        {
            IList<ISite> returnList = new List<ISite>();

            foreach (IBusinessUnit businessUnit in AvailableBusinessUnits)
            {
                foreach (ISite site in businessUnit.SiteCollection)
                {
                    returnList.Add(site);
                }
            }
            foreach (ISite site in AvailableSites)
            {
                    returnList.Add(site);
            }
            return returnList;
        }

        /// <summary>
        /// Converts the available data to permitted person collection.
        /// </summary>
        /// <param name="candidates">The candidates.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public virtual IList<IPerson> ConvertToPermittedPersonCollection(IList<IPerson> candidates, DateOnlyPeriod period)
        {
            IList<IPerson> returnList = new List<IPerson>();

            foreach (IBusinessUnit businessUnit in AvailableBusinessUnits)
            {
                foreach (IPerson person in businessUnit.PersonsInHierarchy(candidates, period))
                {
                    returnList.Add(person);
                }
            }
            foreach (ISite site in AvailableSites)
            {
                foreach (IPerson person in site.PersonsInHierarchy(candidates, period))
                {
                    returnList.Add(person);
                }
            }
            foreach (ITeam team in AvailableTeams)
            {
                foreach (IPerson person in team.PersonsInHierarchy(candidates, period))
                {
                    returnList.Add(person);
                }
            }
            foreach (IPerson person in AvailablePersons)
            {
                 returnList.Add(person);
            }
            return returnList;
        }

        /// <summary>
        /// Converts the available data to permitted data entry collection.
        /// </summary>
        /// <returns></returns>
        public virtual IList<IAvailableDataEntry> ConvertToPermittedDataEntryCollection()
        {
            IList<IAvailableDataEntry> returnList = new List<IAvailableDataEntry>();

            foreach (IBusinessUnit businessUnit in AvailableBusinessUnits)
            {
                BusinessUnitAuthorizationEntityConverter holder = new BusinessUnitAuthorizationEntityConverter(businessUnit, ApplicationRole);
                AvailableDataEntry newEntry = new AvailableDataEntry(holder);
                returnList.Add(newEntry);
            }
            foreach (ISite site in AvailableSites)
            {
                SiteAuthorizationEntityConverter holder = new SiteAuthorizationEntityConverter(site, ApplicationRole);

                AvailableDataEntry newEntry = new AvailableDataEntry(holder);
                returnList.Add(newEntry);
            }
            foreach (ITeam team in AvailableTeams)
            {
                TeamAuthorizationEntityConverter holder = new TeamAuthorizationEntityConverter(team, ApplicationRole);

                AvailableDataEntry newEntry = new AvailableDataEntry(holder);
                returnList.Add(newEntry);
            }
            foreach (IPerson person in AvailablePersons)
            {
                PersonAuthorizationEntityConverter holder = new PersonAuthorizationEntityConverter(person, ApplicationRole);

                AvailableDataEntry newEntry = new AvailableDataEntry(holder);
                returnList.Add(newEntry);
            }
            return returnList;
        }

        /// <summary>
        /// Gets or sets the application role.
        /// </summary>
        /// <value>The application role.</value>
        public virtual IApplicationRole ApplicationRole
        {
            get { return _applicationRole; }
            set { _applicationRole = value; }
        }

        /// <summary>
        /// Gets the available business units.
        /// </summary>
        /// <value>The available business units.</value>
        public virtual ReadOnlyCollection<IBusinessUnit> AvailableBusinessUnits
        {
            get
            {
                IList<IBusinessUnit> returnList = new List<IBusinessUnit>();
                foreach (IBusinessUnit businessUnit in _availableBusinessUnits)
                {
                    if (businessUnit != null && !returnList.Contains(businessUnit))
                    {
                        returnList.Add(businessUnit);
                    }
                }
                return new ReadOnlyCollection<IBusinessUnit>(returnList);
            }
        }

        /// <summary>
        /// Adds an available business unit.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        public virtual void AddAvailableBusinessUnit(IBusinessUnit businessUnit)
        {
            InParameter.NotNull("businessUnit", businessUnit);
            if (!_availableBusinessUnits.Contains(businessUnit))
                _availableBusinessUnits.Add(businessUnit);
        }

        /// <summary>
        /// Gets the available sites.
        /// </summary>
        /// <value>The available sites.</value>
        public virtual ReadOnlyCollection<ISite> AvailableSites
        {
            get
            {
                IList<ISite> returnList = new List<ISite>();
                foreach (ISite site in _availableSites)
                {
                    if (site != null && !returnList.Contains(site))
                    {
                        returnList.Add(site);
                    }
                }
                return new ReadOnlyCollection<ISite>(returnList);
            }
        }

        /// <summary>
        /// Adds an available site.
        /// </summary>
        /// <param name="site">The site.</param>
        public virtual void AddAvailableSite(ISite site)
        {
            InParameter.NotNull("site", site);
            if (!_availableSites.Contains(site))
                _availableSites.Add(site);
        }

        /// <summary>
        /// Gets the available teams.
        /// </summary>
        /// <value>The available teams.</value>
        public virtual ReadOnlyCollection<ITeam> AvailableTeams
        {
            get
            {
                IList<ITeam> returnList = new List<ITeam>();
                foreach (ITeam team in _availableTeams)
                {
                    if (team != null && !returnList.Contains(team))
                    {
                        returnList.Add(team);
                    }
                }
                return new ReadOnlyCollection<ITeam>(returnList);
            }
        }

        /// <summary>
        /// Adds an available team.
        /// </summary>
        /// <param name="team">The team.</param>
        public virtual void AddAvailableTeam(ITeam team)
        {
            InParameter.NotNull("team", team);
            if (!_availableTeams.Contains(team))
                _availableTeams.Add(team);
        }

        /// <summary>
        /// Gets the available agents.
        /// </summary>
        /// <value>The available agents.</value>
        public virtual ReadOnlyCollection<IPerson> AvailablePersons
        {
            get
            {
                IList<IPerson> returnList = new List<IPerson>();
                foreach (IPerson person in _availablePersons)
                {
                    if (person != null && !returnList.Contains(person))
                    {
                        returnList.Add(person);
                    }
                }
                return new ReadOnlyCollection<IPerson>(returnList);
            }
        }

        /// <summary>
        /// Adds an available person.
        /// </summary>
        /// <param name="person">The person.</param>
        public virtual void AddAvailablePerson(IPerson person)
        {
            InParameter.NotNull("person", person);
            if (!_availablePersons.Contains(person))
                _availablePersons.Add(person);
        }

        /// <summary>
        /// Deletes an available person.
        /// </summary>
        /// <param name="person">The person.</param>
        public virtual void DeleteAvailablePerson(IPerson person)
        {
            InParameter.NotNull("person", person);
            if (_availablePersons.Contains(person))
                _availablePersons.Remove(person);
        }

        /// <summary>
        /// Deletes the availableteam.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/2/2008
        /// </remarks>
        public virtual void DeleteAvailableTeam(ITeam team)
        {
            InParameter.NotNull("team", team);
            if (_availableTeams.Contains(team))
                _availableTeams.Remove(team);
        }

        /// <summary>
        /// Deletes the available site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/2/2008
        /// </remarks>
        public virtual void DeleteAvailableSite(ISite site)
        {
            InParameter.NotNull("site", site);
            if (_availableSites.Contains(site))
                _availableSites.Remove(site);

        }

        /// <summary>
        /// Deletes the available business unit.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/2/2008
        /// </remarks>
        public virtual void DeleteAvailableBusinessUnit(IBusinessUnit businessUnit)
        {
            InParameter.NotNull("businessUnit", businessUnit);
            if (_availableBusinessUnits.Contains(businessUnit))
                _availableBusinessUnits.Remove(businessUnit);

        }

        /// <summary>
        /// Gets or sets the available data range.
        /// </summary>
        /// <value>The available data range.</value>
        public virtual AvailableDataRangeOption AvailableDataRange
        {
            get { return _availableDataRange; }
            set { _availableDataRange = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the user has permission to persons who are not part of the organization.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if has permission to person who not part of organization; otherwise, <c>false</c>.
        /// </value>
        public virtual bool? PermissionToPersonNotPartOfOrganization
        {
            get
            {
                return _permissionToPersonNotPartOfOrganization;
            }
            set
            {
                _permissionToPersonNotPartOfOrganization = value;
            }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        #endregion

    }
}
