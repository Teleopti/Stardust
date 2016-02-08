using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Tracking
{
    public class PersonAccountProjectionService : IPersonAccountProjectionService
    {
        private readonly ISchedule _schedule;
        private DateTimePeriod _accountPeriod;
        private readonly IAccount _account;
        private readonly IPerson _person;
        
        public PersonAccountProjectionService(IAccount account, ISchedule loadedSchedule)
        {
           if (loadedSchedule!=null) InParameter.MustBeTrue("Person must be the same on schedule and PersonAccount",account.Owner.Person.Equals(loadedSchedule.Person));

            _account = account;
            _schedule = loadedSchedule;
            _person = account.Owner.Person;
            _accountPeriod = account.Period().ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());
        }


		// todo: tamasb> can be removed, not used, we never send a schedule as parameter
        public IList<DateTimePeriod> PeriodsToLoad()
        {

            IList<DateTimePeriod> retList = new List<DateTimePeriod>();
			if (noSchedule() || noIntersection())
            {
                retList.Add(_accountPeriod);
                return retList;
            }

            if (_schedule.Period.StartDateTime > _accountPeriod.StartDateTime)
                retList.Add(new DateTimePeriod(_accountPeriod.StartDateTime, _schedule.Period.StartDateTime));
            if (_schedule.Period.EndDateTime < _accountPeriod.EndDateTime)
                retList.Add(new DateTimePeriod(_schedule.Period.EndDateTime, _accountPeriod.EndDateTime));

            return retList;
        }

	    private bool noIntersection()
	    {
		    return _accountPeriod.Intersection(_schedule.Period) == null;
	    }

	    private bool noSchedule()
	    {
		    return _schedule == null;
	    }

	    public DateTimePeriod? PeriodToReadFromSchedule()
        {
            return (_schedule != null) ? _accountPeriod.Intersection(_schedule.Period) : null;
        }

        public IList<IScheduleDay> CreateProjection(IScheduleStorage storage, IScenario scenario)
        {
            var scheduleDays  = new SortedList<DateOnly, IScheduleDay>();
	        var timeZone = _person.PermissionInformation.DefaultTimeZone();

	        foreach(DateTimePeriod period in PeriodsToLoad())
            {
	            var intersection = _accountPeriod.Intersection(period);
	            if (!intersection.HasValue) continue;

	            var dateOnlyPeriod = intersection.Value.ToDateOnlyPeriod(timeZone);
	            var range = storage.ScheduleRangeBasedOnAbsence(intersection.Value, scenario, _person, _account.Owner.Absence);
	            range.ScheduledDayCollection(dateOnlyPeriod)
	                 .ForEach(d => scheduleDays.Add(d.DateOnlyAsPeriod.DateOnly, d));
            }

	        var periodToReadFromSchedule = PeriodToReadFromSchedule();
            if (_schedule != null && periodToReadFromSchedule.HasValue)
            {
                var range = _schedule as IScheduleRange ?? _schedule.Owner[_person];
                var dateOnlyPeriod = periodToReadFromSchedule.Value.ToDateOnlyPeriod(timeZone);
				range.ScheduledDayCollection(dateOnlyPeriod)
						 .ForEach(d => scheduleDays.Add(d.DateOnlyAsPeriod.DateOnly,d));
            }
            return scheduleDays.Values;
        }
    }
}
