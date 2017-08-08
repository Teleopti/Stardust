using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ScheduleRange : Schedule, IScheduleRange, IValidateScheduleRange, IUnvalidatedScheduleRangeUpdate
	{
		private readonly IPersistableScheduleDataPermissionChecker _permissionChecker;
		private IList<IScheduleData> _scheduleObjectsWithNoPermissions;
		private ScheduleRange _snapshot;
		private TargetScheduleSummary _targetScheduleSummary;
		private CurrentScheduleSummary _currentScheduleSummary;
		private readonly Lazy<IEnumerable<DateOnlyPeriod>> _availablePeriods;
		private IShiftCategoryFairnessHolder _shiftCategoryFairnessHolder;


		public ScheduleRange(IScheduleDictionary owner, IScheduleParameters parameters, IPersistableScheduleDataPermissionChecker permissionChecker)
			: base(owner, parameters)
		{
			_permissionChecker = permissionChecker;
			_scheduleObjectsWithNoPermissions = new List<IScheduleData>();
			_availablePeriods = new Lazy<IEnumerable<DateOnlyPeriod>>(() =>
				{
					var timeZone = Person.PermissionInformation.DefaultTimeZone();
					var dop = Period.ToDateOnlyPeriod(timeZone);
					return PrincipalAuthorization.Current()
												 .PermittedPeriods(DefinedRaptorApplicationFunctionPaths.ViewSchedules, dop,
																   Person);
				});
		}

		public ScheduleRange Snapshot
		{
			get { return _snapshot ?? (_snapshot = new ScheduleRange(Owner, this, _permissionChecker)); }
		}

		public IEnumerable<DateOnlyPeriod> AvailablePeriods()
		{
			return _availablePeriods.Value;
		}

		public bool Contains(IScheduleData scheduleData, bool includeNonPermitted)
		{
			if (!includeNonPermitted)
				return Contains(scheduleData);
			return Contains(scheduleData) || _scheduleObjectsWithNoPermissions.Contains(scheduleData);
		}

		public void ExtractAllScheduleData(IScheduleExtractor extractor, DateTimePeriod period)
		{
			var zone = Person.PermissionInformation.DefaultTimeZone();
			var days = period.ToDateOnlyPeriod(zone).Inflate(1).DayCollection();
			foreach (var day in days)
			{
				var dayAndPeriod = new DateOnlyAsDateTimePeriod(day, zone);
				var part = ScheduleDay(dayAndPeriod, true, AvailablePeriods());
				(from data in _scheduleObjectsWithNoPermissions
				 where data.BelongsToPeriod(dayAndPeriod)
				 select data).ForEach(data => part.Add((IScheduleData)data.Clone()));
				extractor.AddSchedulePart(part);
			}
		}

		public IScheduleDay ReFetch(IScheduleDay schedulePart)
		{
			return ScheduledDay(schedulePart.DateOnlyAsPeriod.DateOnly);
		}

		public IScheduleDay ScheduledDay(DateOnly day, bool includeUnpublished)
		{
			var dayAndPeriod = new DateOnlyAsDateTimePeriod(day, Person.PermissionInformation.DefaultTimeZone());
			return ScheduleDay(dayAndPeriod, includeUnpublished, AvailablePeriods());
		}

		public IScheduleDay ScheduledDay(DateOnly day)
		{
			var dayAndPeriod = new DateOnlyAsDateTimePeriod(day, Person.PermissionInformation.DefaultTimeZone());
			return ScheduleDay(dayAndPeriod, PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules), AvailablePeriods());
		}

		public void ValidateBusinessRules(INewBusinessRuleCollection newBusinessRuleCollection)
		{
			var period = Owner.Period.VisiblePeriod.ToDateOnlyPeriod(Person.PermissionInformation.DefaultTimeZone());

			BusinessRuleResponseInternalCollection.Clear();
			IList<IScheduleDay> scheduleDays = ScheduledDayCollection(period).ToList();

			if (newBusinessRuleCollection != null)
			{
				IDictionary<IPerson, IScheduleRange> ranges = new Dictionary<IPerson, IScheduleRange>();
				ranges.Add(Person, this);
				newBusinessRuleCollection.CheckRules(ranges, scheduleDays);
			}
		}

		protected override bool CheckPermission(IScheduleData persistableScheduleData)
		{
			var hasPermission = false;
			foreach (var availablePeriod in AvailablePeriods())
			{
				if (persistableScheduleData.BelongsToPeriod(availablePeriod))
				{
					hasPermission = true;
					break;
				}
			}

			if (!hasPermission)
			{
				if (Owner.PermissionsEnabled)
				{
					throw new PermissionException("Cannot add " + persistableScheduleData + " to the collection.");
				}
				_scheduleObjectsWithNoPermissions.Add(persistableScheduleData);
			}
			return hasPermission;
		}

		internal IList<IPersonMeeting> DeleteMeetingFromDataSource(Guid id)
		{
			IList<IPersonMeeting> personMeetings = new List<IPersonMeeting>();
			foreach (IScheduleData scheduleData in ScheduleDataInternalCollection())
			{
				IPersonMeeting casted = scheduleData as IPersonMeeting;
				if (casted != null && casted.BelongsToMeeting.Id == id)
				{
					personMeetings.Add(casted);

					Remove(casted);
					Snapshot.Remove(casted);
				}
			}
			return personMeetings;
		}

		public IDifferenceCollection<IPersistableScheduleData> DifferenceSinceSnapshot(IDifferenceCollectionService<IPersistableScheduleData> differenceService)
		{
			var org = new List<IPersistableScheduleData>(Snapshot.PersistableScheduleDataInternalCollection());
			var current = new List<IPersistableScheduleData>(PersistableScheduleDataInternalCollection());

			return differenceService.Difference(org, current);
		}

		public IDifferenceCollection<IPersistableScheduleData> DifferenceSinceSnapshot(IDifferenceCollectionService<IPersistableScheduleData> differenceService, DateOnlyPeriod period)
		{
			var org = new List<IPersistableScheduleData>(Snapshot.PersistableScheduleDataInternalCollection().Where(x => x.BelongsToPeriod(period)));
			var current = new List<IPersistableScheduleData>(PersistableScheduleDataInternalCollection().Where(x => x.BelongsToPeriod(period)));

			return differenceService.Difference(org, current);
		}

	//don't use this from client. use scheduledictionary.modify instead!
	public void ModifyInternal(IScheduleDay part)
		{
			var periodData = PersistableScheduleDataInternalCollection().Where(d => d.BelongsToPeriod(part.DateOnlyAsPeriod));
			var permittedData = _permissionChecker.GetPermittedData(periodData);
			permittedData
				.ForEach(Remove);

			AddRange(_permissionChecker.GetPermittedData(part.PersistableScheduleDataCollection()));

			_currentScheduleSummary = null;
			_targetScheduleSummary = null;
			_shiftCategoryFairnessHolder = null;
		}

		public TimeSpan CalculatedContractTimeHolderOnPeriod(DateOnlyPeriod periodToCheck)
		{
			if (_currentScheduleSummary == null)
				_currentScheduleSummary = new CurrentScheduleSummaryCalculator().GetCurrent(this, periodToCheck);

			return _currentScheduleSummary.ContractTime;
		}

		public TimeSpan? CalculatedTargetTimeHolder(DateOnlyPeriod periodToCheck)
		{
			if (_targetScheduleSummary?.TargetTime == null)
				_targetScheduleSummary = new TargetScheduleSummaryCalculator().GetTargets(this, periodToCheck);

			return _targetScheduleSummary.TargetTime;
		}

		public CurrentScheduleSummary CalculatedCurrentScheduleSummary(DateOnlyPeriod periodToCheck)
		{
			if (_currentScheduleSummary == null)
				_currentScheduleSummary = new CurrentScheduleSummaryCalculator().GetCurrent(this, periodToCheck);

			return _currentScheduleSummary;
		}

		public TargetScheduleSummary CalculatedTargetTimeSummary(DateOnlyPeriod periodToCheck)
		{
			if (_targetScheduleSummary?.TargetTime == null)
				_targetScheduleSummary = new TargetScheduleSummaryCalculator().GetTargets(this, periodToCheck);

			return _targetScheduleSummary;
		}

		public int CalculatedScheduleDaysOffOnPeriod(DateOnlyPeriod periodToCheck)
		{
			if (_currentScheduleSummary == null)
				_currentScheduleSummary = new CurrentScheduleSummaryCalculator().GetCurrent(this, periodToCheck);

			return _currentScheduleSummary.NumberOfDaysOff;
		}

		public int? CalculatedTargetScheduleDaysOff(DateOnlyPeriod periodToCheck)
		{
			if (_targetScheduleSummary?.TargetDaysOff == null)
			{
				_targetScheduleSummary = new TargetScheduleSummaryCalculator().GetTargets(this, periodToCheck);
			}

			return _targetScheduleSummary.TargetDaysOff;
		}

		public IEnumerable<IScheduleDay> ScheduledDayCollection(DateOnlyPeriod dateOnlyPeriod)
		{
			var canSeeUnpublished =
				PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			return getScheduledDayCollection(dateOnlyPeriod, canSeeUnpublished);
		}

		/// <summary>
		/// Get scheduleds for the day colletion to get student availability
		/// This method will ignore published date and view unpublished schdule permission setting to get 
		/// student availability after published date even if current user has no permission to view unpublished schedule.
		/// It should only be used to get schedule to retrieve student availability.
		/// Refer to bug #33327: Agents can no longer see Availability they entered for dates that have not been published.
		/// </summary>
		public IEnumerable<IScheduleDay> ScheduledDayCollectionForStudentAvailability(DateOnlyPeriod dateOnlyPeriod)
		{
			return getScheduledDayCollection(dateOnlyPeriod, true);
		}

		public void TakeSnapshot()
		{
			_snapshot = (ScheduleRange)Clone();
			clearSnapshotsSnapshot();
		}

		private void clearSnapshotsSnapshot()
		{
			if (_snapshot != null)
			{
				_snapshot._snapshot = null;
			}
		}

		//TODO only here temporarly, will be solved when refact validations
		public IList<IBusinessRuleResponse> ExposedBusinessRuleResponseCollection()
		{
			return BusinessRuleResponseInternalCollection;
		}

		//use this one only if you know what you're doing!
		//not part of IScheduleRange. Use with care!
		public virtual void SolveConflictBecauseOfExternalInsert(IScheduleData databaseVersion, bool discardMyChanges)
		{
			if (discardMyChanges)
			{
				Snapshot.Add(databaseVersion);
				var myVersion = find(databaseVersion);
				Remove(myVersion);
				Add(databaseVersion);
			}
			else
			{
				Snapshot.Add(databaseVersion);
				var myVersion = find(databaseVersion);
				var databaseVersionEntity = databaseVersion as IEntity;
				if (databaseVersionEntity != null)
					((IEntity)myVersion).SetId(databaseVersionEntity.Id);
				var databaseVersionVersioned = databaseVersion as IVersioned;
				if (databaseVersionVersioned != null)
					((IVersioned)myVersion).SetVersion(databaseVersionVersioned.Version.Value);
			}
		}

		//use this one only if you know what you're doing!
		//not part of IScheduleRange. Use with care!
		public virtual void SolveConflictBecauseOfExternalUpdate(IScheduleData databaseVersion, bool discardMyChanges)
		{
			// replace version in snapshot with database version
			Snapshot.Remove(databaseVersion);
			Snapshot.Add(databaseVersion);

			if (discardMyChanges)
			{
				// get my version of this thing, and remove if it exists
				var myVersion = find(databaseVersion);
				if (myVersion != null)
					Remove(myVersion);
				// put database version as my version
				Add(databaseVersion);
			}
			else
			{
				// get my version of this thing
				var myVersion = find(databaseVersion);
				// update version number of my data to databases version
				var databaseVersioned = databaseVersion as IVersioned;
				if (databaseVersioned != null)
				{
					var myVersioned = myVersion as IVersioned;
					if (myVersioned != null)
						myVersioned.SetVersion(databaseVersioned.Version.Value);
				}
			}
		}

		//use this one only if you know what you're doing!
		//not part of IScheduleRange. Use with care!
		public IPersistableScheduleData SolveConflictBecauseOfExternalDeletion(Guid id, bool discardMyChanges)
		{
			foreach (var scheduleData in Snapshot.ScheduleDataInternalCollection())
			{
				var casted = scheduleData as IPersistableScheduleData;
				if (casted != null && casted.Id == id)
				{
					var current = ((IPersistableScheduleData)find(casted));
					Snapshot.Remove(casted);
					if (current != null)
					{
						Remove(casted);

						// if overwrite other's deletion, mimic an add
						if (!discardMyChanges)
						{
							var transientCurrent = current.CreateTransient();
							Add(transientCurrent);
						}
					}
					return casted;
				}
			}
			return null;
		}

		private IScheduleData find(IScheduleData scheduleData)
		{
			foreach (var data in ScheduleDataInternalCollection())
			{
				if (data.Equals(scheduleData))
					return data;
			}
			return null;
		}

		public DateTimePeriod VisiblePeriodMinusFourWeeksPeriod()
		{
			return ((ISchedule)this).Owner.Period.VisiblePeriodMinusFourWeeksPeriod();
		}

		public IShiftCategoryFairnessHolder CachedShiftCategoryFairness()
		{
			if (_shiftCategoryFairnessHolder == null)
			{
				ShiftCategoryFairnessCreator creator = new ShiftCategoryFairnessCreator();
				TimeZoneInfo timeZoneInfo = this.Person.PermissionInformation.DefaultTimeZone();
				DateOnlyPeriod period = VisiblePeriodMinusFourWeeksPeriod().ToDateOnlyPeriod(timeZoneInfo);
				_shiftCategoryFairnessHolder = creator.CreatePersonShiftCategoryFairness(this, period);
			}
			return _shiftCategoryFairnessHolder;
		}

		public void ForceRecalculationOfTargetTimeContractTimeAndDaysOff()
		{
			_currentScheduleSummary = null;
			_targetScheduleSummary = null;
		}

		public bool IsEmpty()
		{
			return !PersistableScheduleDataInternalCollection().Any();
		}

		public void Reassociate(IUnitOfWork unitOfWork)
		{
			unitOfWork.Reassociate(PersistableScheduleDataInternalCollection());
		}

		protected override void CloneDerived(Schedule clone)
		{
			var typedClone = (ScheduleRange)clone;
			typedClone._snapshot = null;
			typedClone._scheduleObjectsWithNoPermissions = new List<IScheduleData>();
			typedClone._shiftCategoryFairnessHolder = null;
		}

		private IEnumerable<IScheduleDay> getScheduledDayCollection(DateOnlyPeriod dateOnlyPeriod, bool canSeeUnpublished)
		{
			var timeZone = Person.PermissionInformation.DefaultTimeZone();
			var availablePeriods = AvailablePeriods();

			var retList = new List<IScheduleDay>();
			foreach (var date in dateOnlyPeriod.DayCollection())
			{
				var dayAndPeriod = new DateOnlyAsDateTimePeriod(date, timeZone);
				retList.Add(ScheduleDay(dayAndPeriod, canSeeUnpublished, availablePeriods));
			}
			return retList;
		}
	}
	public class PersistableScheduleDataForAuthorization : IPersistableScheduleDataAuthorizer
	{
		private readonly IPersistableScheduleData _persistableScheduleData;

		public PersistableScheduleDataForAuthorization(IPersistableScheduleData persistableScheduleData)
		{
			_persistableScheduleData = persistableScheduleData;
		}

		public string FunctionPath { get { return _persistableScheduleData.FunctionPath; } }

		public IPerson Person { get { return _persistableScheduleData.Person; } }

		public DateOnly DateOnly
		{
			get
			{
				return
					new DateOnly(
						_persistableScheduleData.Period.StartDateTimeLocal(_persistableScheduleData.Person.PermissionInformation.DefaultTimeZone()));
			}
		}
	}

	public interface IPersistableScheduleDataAuthorizer
	{
	}

}
