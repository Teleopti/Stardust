using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    public class AvailableData : VersionedAggregateRoot, IAvailableData, IDeleteTag
    {
	    private IApplicationRole _applicationRole;
        private readonly ICollection<IBusinessUnit> _availableBusinessUnits;
        private readonly ICollection<ISite> _availableSites;
        private readonly ICollection<ITeam> _availableTeams;
        private AvailableDataRangeOption _availableDataRange;
        private bool _isDeleted;

	    /// <summary>               
        /// Initializes a new instance of the <see cref="AvailableData"/> class.
        /// </summary>
        public AvailableData()
        {
            _availableTeams = new List<ITeam>();
            _availableSites = new List<ISite>();
            _availableBusinessUnits = new List<IBusinessUnit>();
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
        public virtual ReadOnlyCollection<IBusinessUnit> AvailableBusinessUnits => new ReadOnlyCollection<IBusinessUnit>(_availableBusinessUnits.Distinct().ToArray());

		/// <summary>
        /// Adds an available business unit.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        public virtual void AddAvailableBusinessUnit(IBusinessUnit businessUnit)
        {
            InParameter.NotNull(nameof(businessUnit), businessUnit);
            if (!_availableBusinessUnits.Contains(businessUnit))
                _availableBusinessUnits.Add(businessUnit);
        }

        /// <summary>
        /// Gets the available sites.
        /// </summary>
        /// <value>The available sites.</value>
        public virtual ReadOnlyCollection<ISite> AvailableSites => new ReadOnlyCollection<ISite>(_availableSites.Distinct().ToArray());

		/// <summary>
        /// Adds an available site.
        /// </summary>
        /// <param name="site">The site.</param>
        public virtual void AddAvailableSite(ISite site)
        {
            InParameter.NotNull(nameof(site), site);
            if (!_availableSites.Contains(site))
                _availableSites.Add(site);
        }

        /// <summary>
        /// Gets the available teams.
        /// </summary>
        /// <value>The available teams.</value>
        public virtual ReadOnlyCollection<ITeam> AvailableTeams => new ReadOnlyCollection<ITeam>(_availableTeams.Distinct().ToArray());

		/// <summary>
        /// Adds an available team.
        /// </summary>
        /// <param name="team">The team.</param>
        public virtual void AddAvailableTeam(ITeam team)
        {
            InParameter.NotNull(nameof(team), team);
            if (!_availableTeams.Contains(team))
                _availableTeams.Add(team);
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
            InParameter.NotNull(nameof(team), team);
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
            InParameter.NotNull(nameof(site), site);
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
            InParameter.NotNull(nameof(businessUnit), businessUnit);
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
		
        public virtual bool IsDeleted => _isDeleted;

	    public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
