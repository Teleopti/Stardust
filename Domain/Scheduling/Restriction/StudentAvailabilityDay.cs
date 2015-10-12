using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
	public class StudentAvailabilityDay : VersionedAggregateRootWithBusinessUnit, IRestrictionOwner, IStudentAvailabilityDay
	{
		private IList<IStudentAvailabilityRestriction> _restrictionCollection;
		private readonly IPerson _person;
		private readonly DateOnly _restrictionDate;
		private bool _notAvailable;


		public StudentAvailabilityDay(IPerson person, DateOnly restrictionDate, IList<IStudentAvailabilityRestriction> restrictionCollection)
		{
			_person = person;
			_restrictionDate = restrictionDate;
			foreach (var studentAvailabilityRestriction in restrictionCollection)
			{
				((StudentAvailabilityRestriction)studentAvailabilityRestriction).SetParent(this);
			}
			_restrictionCollection = restrictionCollection;
		}

		public virtual int IndexInCollection(IStudentAvailabilityRestriction restriction)
		{
			return _restrictionCollection.IndexOf(restriction);
		}

		public virtual bool NotAvailable
		{
			get { return _notAvailable; }
			set { _notAvailable = value; }
		}

		protected StudentAvailabilityDay() { }

		public virtual ReadOnlyCollection<IStudentAvailabilityRestriction> RestrictionCollection
		{
			get
			{
				return new ReadOnlyCollection<IStudentAvailabilityRestriction>(_restrictionCollection);
			}
		}

		public virtual IEnumerable<IRestrictionBase> RestrictionBaseCollection
		{
			get
			{
				var ret = new List<IRestrictionBase>();
				_restrictionCollection.ForEach(ret.Add);
				return ret;
			}
		}

		public virtual IPerson Person
		{
			get { return _person; }
		}

		public virtual IScenario Scenario
		{
			get { return null; }
		}

		public virtual DateOnly RestrictionDate
		{
			get { return _restrictionDate; }
		}

		public virtual void Change(TimePeriod range)
		{
			_restrictionCollection.Clear();
			var restriction = new StudentAvailabilityRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(range.StartTime, null),
				EndTimeLimitation = new EndTimeLimitation(null, range.EndTime)
			};
			restriction.SetParent(this);
			_restrictionCollection.Add(restriction);
		}

		public virtual DateTimePeriod Period
		{
			get { return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_restrictionDate.Date, _restrictionDate.Date.AddDays(1), _person.PermissionInformation.DefaultTimeZone()); }
		}

		public virtual object Clone()
		{
			var clone = (StudentAvailabilityDay)MemberwiseClone();

			clone._restrictionCollection = new List<IStudentAvailabilityRestriction>();
			foreach (var studentAvailabilityRestriction in _restrictionCollection)
			{
				var cloneRestriction = (IStudentAvailabilityRestriction)studentAvailabilityRestriction.Clone();
				cloneRestriction.SetParent(clone);
				clone._restrictionCollection.Add(cloneRestriction);
			}
			return clone;
		}

		public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
		{
			return dateAndPeriod.DateOnly == _restrictionDate;
		}

		public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
		{
			return dateOnlyPeriod.Contains(_restrictionDate);
		}

		public virtual bool BelongsToScenario(IScenario scenario)
		{
			return true;
		}

		public virtual IAggregateRoot MainRoot
		{
			get { return Person; }
		}

		public virtual string FunctionPath
		{
			get { return DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction; }
		}

		public virtual IPersistableScheduleData CreateTransient()
		{
			var ret = (IStudentAvailabilityDay)Clone();
			ret.SetId(null);
			foreach (var restriction in ret.RestrictionCollection)
			{
				restriction.SetId(null);
			}
			return ret;
		}

		public override int GetHashCode()
		{
			return Person.GetHashCode() ^ RestrictionDate.GetHashCode();
		}

		public override bool Equals(IEntity other)
		{
			if (other == null) return false;
			return GetHashCode().Equals(other.GetHashCode());
		}
	}
}
