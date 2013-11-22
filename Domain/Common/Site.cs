using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{

    /// <summary>
    /// Class for UnitCollection
    /// </summary>
    public class Site : VersionedAggregateRootWithBusinessUnit, ISite, IDeleteTag
    {
        private readonly IList<ITeam> _teamCollection;
        private Description _description;
        private bool _isDeleted;
    	private int? _maxSeats;
    	private ISkill _maxSeatSkill;
    	
        /// <summary>
        /// Initializes a new instance of the <see cref="Site"/> class.
        /// </summary>
        protected Site()
        {
            _teamCollection = new List<ITeam>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Site"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Site(string name)
            : this()
        {
            _description = new Description(name);
        }

        /// <summary>
        /// Collects all the persons in the candidates that are on the Site.
        /// </summary>
        /// <value></value>
        /// <returns>All persons on the site.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.CompareTo(System.String)")]
        public virtual ReadOnlyCollection<IPerson> PersonsInHierarchy(IEnumerable<IPerson> candidates, DateOnlyPeriod period)
        {
            List<IPerson> personInTeamCollection = new List<IPerson>(candidates).FindAll(new PersonBelongsToTeamSpecification(period, _teamCollection).IsSatisfiedBy);
            personInTeamCollection.Sort(delegate(IPerson p1, IPerson p2) { return p1.Name.ToString(NameOrderOption.LastNameFirstName).CompareTo(p2.Name.ToString(NameOrderOption.LastNameFirstName)); });
            return new ReadOnlyCollection<IPerson>(personInTeamCollection);
        }

		public virtual ISkill MaxSeatSkill
		{
			get { return _maxSeatSkill; }
			set { _maxSeatSkill = value; }
		}

    	/// <summary>
        /// Gets or sets the descrition of the site.
        /// </summary>
        /// <value>The site.</value>
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }
        
        /// <summary>
        /// Gets the teams.
        /// Read only wrapper around the actual list.
        /// </summary>
        /// <value>The teams.</value>
        public virtual ReadOnlyCollection<ITeam> TeamCollection
        {
            get { return new ReadOnlyCollection<ITeam>(_teamCollection); }
        }

    	public virtual ReadOnlyCollection<ITeam> SortedTeamCollection
    	{
    		get
    		{
				var sortedList = (from team in _teamCollection
								  orderby team.Description.Name ascending
								  select team).ToList();

				return new ReadOnlyCollection<ITeam>(sortedList);
    		}
    	}

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

    	public virtual int? MaxSeats
    	{
    		get {
    			return _maxSeats;
    		}
    		set {
    			_maxSeats = value;
    		}
    	}

    	/// <summary>
        /// Adds a Team.
        /// </summary>
        /// <param name="team">The team.</param>
        public virtual void AddTeam(ITeam team)
        {
            InParameter.NotNull("team", team);
            if (!_teamCollection.Contains(team))
            {
                _teamCollection.Add(team);
                team.Site = this;
            }
        }

        /// <summary>
        /// Removes the team.
        /// </summary>
        /// <param name="team">The team.</param>
        public virtual void RemoveTeam(ITeam team)
        {
            InParameter.NotNull("team", team);
            _teamCollection.Remove(team);

        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
