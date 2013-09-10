using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Tracking
{
    public class PersonAccountProjectionService : IPersonAccountProjectionService
    {
        private readonly ISchedule _schedule;
        private DateTimePeriod _accountPeriod;
        private readonly IAccount _account;
        private readonly IPerson _person;
        private DateTimePeriod _period;
        
        public PersonAccountProjectionService(IAccount account, ISchedule loadedSchedule)
        {
           if (loadedSchedule!=null) InParameter.MustBeTrue("Person must be the same on schedule and PersonAccount",account.Owner.Person.Equals(loadedSchedule.Person));

            _account = account;
            _schedule = loadedSchedule;
            _person = account.Owner.Person;
            _accountPeriod = account.Period().ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());
            _period = _accountPeriod;
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

        public IList<IScheduleDay> CreateProjection(IScheduleRepository repository, IScenario scenario)
        {
            var scheduleDays  = new SortedList<DateOnly, IScheduleDay>();

            foreach(DateTimePeriod period in PeriodsToLoad())
            {
                
                if (_period.Intersect(period))
                {
                    DateTimePeriod intersection = (DateTimePeriod)_period.Intersection(period);
                    var dateOnlyPeriod = intersection.ToDateOnlyPeriod(_person.PermissionInformation.DefaultTimeZone());
                    IScheduleRange range = repository.ScheduleRangeBasedOnAbsence(intersection, scenario, _person, _account.Owner.Absence);
                    foreach (DateOnly dateOnly in dateOnlyPeriod.DayCollection())
                    {
                        scheduleDays.Add(dateOnly, range.ScheduledDay(dateOnly));
                    }
                }
            }

            if (_schedule != null && PeriodToReadFromSchedule() != null)
            {
                var sched = _schedule as IScheduleRange ?? _schedule.Owner[_person];
                var dateOnlyPeriod = PeriodToReadFromSchedule().Value.ToDateOnlyPeriod(_person.PermissionInformation.DefaultTimeZone());
                foreach (DateOnly dateOnly in dateOnlyPeriod.DayCollection())
                {
                    scheduleDays.Add(dateOnly, sched.ScheduledDay(dateOnly));
                }
            }
            return scheduleDays.Values;
        }
    }
}
