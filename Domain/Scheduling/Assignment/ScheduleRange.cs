using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class ScheduleRange : Schedule, IScheduleRange, IValidateScheduleRange, IUnvalidatedScheduleRangeUpdate
    {
        private IList<IScheduleData> _scheduleObjectsWithNoPermissions;
        private ScheduleRange _snapshot;
        private TimeSpan? _calculatedContractTimeHolder;
        private TimeSpan? _calculatedTargettTimeHolder;
        private int? _calculatedTargetScheduleDaysOff;
        private int? _calculatedScheduleDaysOff;
		private readonly Lazy<IEnumerable<DateOnlyPeriod>> _availablePeriods;
        private IShiftCategoryFairnessHolder _shiftCategoryFairnessHolder;

        public ScheduleRange(IScheduleDictionary owner, IScheduleParameters parameters)
            : base(owner, parameters)
        {
            _scheduleObjectsWithNoPermissions = new List<IScheduleData>();
	        _availablePeriods = new Lazy<IEnumerable<DateOnlyPeriod>>(() =>
		        {
			        var timeZone = Person.PermissionInformation.DefaultTimeZone();
			        var dop = Period.ToDateOnlyPeriod(timeZone);
			        return PrincipalAuthorization.Instance()
			                                     .PermittedPeriods(DefinedRaptorApplicationFunctionPaths.ViewSchedules, dop,
			                                                       Person);
		        });
        }

        public ScheduleRange Snapshot
        {
            get
            {
                if (_snapshot == null)
                    _snapshot = new ScheduleRange(Owner, this);
                return _snapshot;
            }
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
			foreach (var day in period.ToDateOnlyPeriod(zone).DayCollection())
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
			return ScheduleDay(dayAndPeriod, PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules), AvailablePeriods());
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

		//don't use this from client. use scheduledictionary.modify instead!
		public void ModifyInternal(IScheduleDay part)
		{
		    var authorization = PrincipalAuthorization.Instance();

			ICollection<IPersistableScheduleData> permittedData = new List<IPersistableScheduleData>();
			IEnumerable<IPersistableScheduleData> fullData = PersistableScheduleDataInternalCollection();

			foreach (IPersistableScheduleData persistableScheduleData in fullData)
		    {
                var forAuthorization = new PersistableScheduleDataForAuthorization(persistableScheduleData);
		        if(persistableScheduleData.BelongsToPeriod(part.DateOnlyAsPeriod)
                    && 
                   authorization.IsPermitted(forAuthorization.FunctionPath, forAuthorization.DateOnly, forAuthorization.Person))
                    permittedData.Add(persistableScheduleData);
                    
		    }

			permittedData.ForEach(Remove);

		    AddRange(part.PersistableScheduleDataCollection().Where(d =>
		                                                                {
		                                                                    var forAuthorization =
		                                                                        new PersistableScheduleDataForAuthorization(d);
		                                                                    return
		                                                                        authorization.IsPermitted(
		                                                                            forAuthorization.FunctionPath,
		                                                                            forAuthorization.DateOnly,
		                                                                            forAuthorization.Person);
		                                                                }));

            _calculatedContractTimeHolder = null;
            _calculatedTargettTimeHolder = null;
            _calculatedTargetScheduleDaysOff = null;
            _calculatedScheduleDaysOff = null;
            _shiftCategoryFairnessHolder = null;
        }

        public class PersistableScheduleDataForAuthorization
        {
			private readonly IPersistableScheduleData _persistableScheduleData;

			public PersistableScheduleDataForAuthorization(IPersistableScheduleData persistableScheduleData)
            {
                _persistableScheduleData = persistableScheduleData;
            }

            public string FunctionPath { get { return _persistableScheduleData.FunctionPath; } }

            public IPerson Person { get { return _persistableScheduleData.Person; } }

            public DateOnly DateOnly { get
            {
                return
                    new DateOnly(
                        _persistableScheduleData.Period.StartDateTimeLocal(TeleoptiPrincipal.Current.Regional.TimeZone));
            } }
        }

        private IFairnessValueResult FairnessPoints(DateTimePeriod period)
        {
            IFairnessValueResult ret = new FairnessValueResult();
            //using(PerformanceOutput.ForOperation("Calculating JusicePoints for " + Person.Name))
            //{
            foreach (var scheduleData in ScheduleDataInternalCollection())
            {
                if (!(scheduleData is PersonAssignment))
                    continue;

                if (!period.Contains(scheduleData.Period.StartDateTime))
                    continue;

                IPersonAssignment assignment = (IPersonAssignment)scheduleData;
                if (assignment.ShiftCategory == null)
                    continue;

                DayOfWeek dow =
                    TimeZoneHelper.ConvertFromUtc(assignment.Period.StartDateTime,
                                                  Person.PermissionInformation.DefaultTimeZone()).DayOfWeek;
                ret.FairnessPoints += assignment.ShiftCategory.DayOfWeekJusticeValues[dow];
                ret.TotalNumberOfShifts += 1;
            }
            //}
            return ret;
        }

        public IFairnessValueResult FairnessPoints()
        {
            DateTimePeriod period = VisiblePeriodMinusFourWeeksPeriod();
            return FairnessPoints(period);
        }

		public TimeSpan? CalculatedContractTimeHolder
		{
			get { return _calculatedContractTimeHolder; }
			set { _calculatedContractTimeHolder = value; }
		}

		public TimeSpan? CalculatedTargetTimeHolder
		{
			get { return _calculatedTargettTimeHolder; }
			set { _calculatedTargettTimeHolder = value; }
		}

        public int? CalculatedScheduleDaysOff
        {
            get { return _calculatedScheduleDaysOff; }
            set { _calculatedScheduleDaysOff = value; }
        }

		public int? CalculatedTargetScheduleDaysOff
		{
			get { return _calculatedTargetScheduleDaysOff; }
			set { _calculatedTargetScheduleDaysOff = value; }
		}

		public IEnumerable<IScheduleDay> ScheduledDayCollection(DateOnlyPeriod dateOnlyPeriod)
		{
			var canSeeUnpublished = PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
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
					((IEntity) myVersion).SetId(databaseVersionEntity.Id);
				var databaseVersionVersioned = databaseVersion as IVersioned;
				if (databaseVersionVersioned != null)
					((IVersioned) myVersion).SetVersion(databaseVersionVersioned.Version.Value);
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
				    var current = ((IPersistableScheduleData) find(casted));
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
        	return ((ISchedule) this).Owner.Period.VisiblePeriodMinusFourWeeksPeriod();
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

		public void ForceRecalculationOfContractTimeAndDaysOff()
		{
			CalculatedContractTimeHolder = null;
			CalculatedScheduleDaysOff = null;
		}

						public bool IsEmpty()
			{
				return !PersistableScheduleDataInternalCollection().Any();
			}

        protected override void CloneDerived(Schedule clone)
        {
            var typedClone = (ScheduleRange)clone;
            typedClone._snapshot = null;
            typedClone._scheduleObjectsWithNoPermissions = new List<IScheduleData>();
            typedClone._shiftCategoryFairnessHolder = null;
        }
    }
}
