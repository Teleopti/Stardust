using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

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
		private IList<ISiteOpenHour> _openHourCollection;

		/// <summary>
		/// Initializes a new instance of the <see cref="Site"/> class.
		/// </summary>
		protected Site()
		{
			_teamCollection = new List<ITeam>();
			_openHourCollection = new List<ISiteOpenHour>();
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
		/// Gets or sets the descrition of the site.
		/// </summary>
		/// <value>The site.</value>
		public virtual Description Description => _description;

		public virtual void SetDescription(Description value)
		{
			if (_description != value)
				AddEvent(() => new SiteNameChangedEvent
				{
					SiteId = Id.GetValueOrDefault(),
					Name = value.Name
				});
			_description = value;
		}

		/// <summary>
		/// Gets the teams.
		/// Read only wrapper around the actual list.
		/// </summary>
		/// <value>The teams.</value>
		public virtual ReadOnlyCollection<ITeam> TeamCollection => new ReadOnlyCollection<ITeam>(_teamCollection);

		public virtual ReadOnlyCollection<ITeam> SortedTeamCollection
		{
			get
			{
				var sortedList = _teamCollection.OrderBy(team => team.Description.Name).ToList();
				return new ReadOnlyCollection<ITeam>(sortedList);
			}
		}

		public virtual bool IsDeleted => _isDeleted;

		public virtual int? MaxSeats
		{
			get
			{
				return _maxSeats;
			}
			set
			{
				_maxSeats = value;
			}
		}

		/// <summary>
		/// Adds a Team.
		/// </summary>
		/// <param name="team">The team.</param>
		public virtual void AddTeam(ITeam team)
		{
			InParameter.NotNull(nameof(team), team);
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
			InParameter.NotNull(nameof(team), team);
			_teamCollection.Remove(team);

		}

		public virtual IEnumerable<ISiteOpenHour> OpenHourCollection => new ReadOnlyCollection<ISiteOpenHour>(_openHourCollection);

		public virtual void ClearOpenHourCollection()
		{
			_openHourCollection.Clear();
		}

		public virtual bool AddOpenHour(ISiteOpenHour siteOpenHour)
		{
			if (_openHourCollection.All(openHour => openHour.WeekDay != siteOpenHour.WeekDay))
			{
				_openHourCollection.Add(siteOpenHour);
				siteOpenHour.Parent = this;
				return true;
			}
			return false;
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}
	}
}
