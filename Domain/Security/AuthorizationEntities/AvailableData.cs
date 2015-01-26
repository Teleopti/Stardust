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
	    private IApplicationRole _applicationRole;
        private readonly ICollection<IBusinessUnit> _availableBusinessUnits;
        private readonly ICollection<ISite> _availableSites;
        private readonly ICollection<ITeam> _availableTeams;
        private readonly ICollection<IPerson> _availablePersons;
        private AvailableDataRangeOption _availableDataRange;
        private bool? _permissionToPersonNotPartOfOrganization;
        private bool _isDeleted;

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
    }
}
