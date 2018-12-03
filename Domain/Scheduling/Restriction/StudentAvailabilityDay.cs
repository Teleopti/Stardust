using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
	public class StudentAvailabilityDay : VersionedAggregateRootWithBusinessUnit, IRestrictionOwner, IStudentAvailabilityDay, IAggregateRootWithEvents
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

		public virtual ReadOnlyCollection<IStudentAvailabilityRestriction> RestrictionCollection => new ReadOnlyCollection<IStudentAvailabilityRestriction>(_restrictionCollection);

		public virtual IEnumerable<IRestrictionBase> RestrictionBaseCollection => _restrictionCollection.OfType<IRestrictionBase>().ToList();

		public virtual IPerson Person => _person;

		public virtual IScenario Scenario => null;

		public virtual DateOnly RestrictionDate => _restrictionDate;

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

		public virtual DateTimePeriod Period => TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_restrictionDate.Date,
			_restrictionDate.Date.AddDays(1), _person.PermissionInformation.DefaultTimeZone());

		public virtual object Clone()
		{
			var clone = (StudentAvailabilityDay)MemberwiseClone();

			clone._restrictionCollection = _restrictionCollection.Select(studentAvailabilityRestriction =>
			{
				var cloneRestriction = (IStudentAvailabilityRestriction) studentAvailabilityRestriction.Clone();
				cloneRestriction.SetParent(clone);
				return cloneRestriction;
			}).ToList();
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

		public virtual IAggregateRoot MainRoot => Person;

		public virtual string FunctionPath => DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction;

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

		public override void NotifyTransactionComplete(DomainUpdateType operation)
		{
			base.NotifyTransactionComplete(operation);
			switch (operation)
			{
				case DomainUpdateType.Insert:
				case DomainUpdateType.Update:
				case DomainUpdateType.Delete:
					AddEvent(new AvailabilityChangedEvent
					{
						Dates = new List<DateOnly> { RestrictionDate },
						PersonId = Person.Id.GetValueOrDefault()
					});
					break;
			}
		}
	}
}
