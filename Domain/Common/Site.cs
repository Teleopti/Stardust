using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public class Site : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, ISite, IDeleteTag, IAggregateRoot_Events
	{
		private readonly IList<ITeam> _teamCollection;
		private Description _description;
		private bool _isDeleted;
		private int? _maxSeats;
		private IList<ISiteOpenHour> _openHourCollection;
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

		protected Site()
		{
			_teamCollection = new List<ITeam>();
			_openHourCollection = new List<ISiteOpenHour>();
		}

		public Site(string name)
			: this()
		{
			_description = new Description(name);
		}

		public override IEnumerable<IEvent> PopAllEvents(IPopEventsContext context) =>
			base.PopAllEvents(context)
				.KeepLastOfType<SiteNameChangedEvent>();

		public virtual Description Description => _description;

		public virtual void SetDescription(Description value)
		{
			AddEvent(() => new SiteNameChangedEvent
			{
				SiteId = Id.GetValueOrDefault(),
				Name = value.Name
			});
			_description = value;
		}

		public virtual void SetBusinessUnit(IBusinessUnit value)
		{
			BusinessUnit = value;
		}

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
			get { return _maxSeats; }
			set { _maxSeats = value; }
		}

		public virtual void AddTeam(ITeam team)
		{
			InParameter.NotNull(nameof(team), team);
			if (!_teamCollection.Contains(team))
			{
				_teamCollection.Add(team);
				team.Site = this;
			}
		}

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

		public virtual string UpdatedTimeInUserPerspective => _localizer.UpdatedTimeInUserPerspective(this);

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}
	}
}