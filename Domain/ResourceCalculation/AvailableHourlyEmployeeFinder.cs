using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class AvailableHourlyEmployeeFinderResult
	{
		private readonly IPerson _person;
		private readonly bool _matching;
		private readonly string _availability;
		private readonly string _workTimesYesterday;
		private readonly string _workTimesTomorrow;
		private readonly bool _nightRestOk;

		public AvailableHourlyEmployeeFinderResult(IPerson person, bool matching, string availability, string workTimesYesterday, string workTimesTomorrow, bool nightRestOk)
		{
			_person = person;
			_matching = matching;
			_availability = availability;
			_workTimesYesterday = workTimesYesterday;
			_workTimesTomorrow = workTimesTomorrow;
			_nightRestOk = nightRestOk;
		}

		public IPerson Person
		{
			get { return _person; }
		}

		public bool Matching
		{
			get { return _matching; }
		}

		public string WorkTimesYesterday
		{
			get { return _workTimesYesterday; }
		}

		public string WorkTimesTomorrow
		{
			get { return _workTimesTomorrow; }
		}

		public bool NightRestOk
		{
			get { return _nightRestOk; }
		}

		public string Availability
		{
			get { return _availability; }
		}
	}

	public interface IAvailableHourlyEmployeeFinder
	{
		IList<AvailableHourlyEmployeeFinderResult> Find();
	}

	public class AvailableHourlyEmployeeFinder :IAvailableHourlyEmployeeFinder
	{
		private readonly IPerson _sourcePerson;
		private readonly DateOnly _dateOnly;
	    private readonly ISchedulingResultStateHolder _resultStateHolder;
	    private readonly ICollection<IPerson> _filteredPersons;

		public AvailableHourlyEmployeeFinder(IPerson sourcePerson, DateOnly dateOnly, ISchedulingResultStateHolder resultStateHolder, ICollection<IPerson> filteredPersons)
		{
			_sourcePerson = sourcePerson;
			_dateOnly = dateOnly;
		    _resultStateHolder = resultStateHolder;
		    _filteredPersons = filteredPersons;
		}

        public IList<AvailableHourlyEmployeeFinderResult> Find()
        {
	        var ret = new BlockingCollection<AvailableHourlyEmployeeFinderResult>();
            IScheduleDay source = _resultStateHolder.Schedules[_sourcePerson].ScheduledDay(_dateOnly);
			if (source.SignificantPart() != SchedulePartView.MainShift)
				return new List<AvailableHourlyEmployeeFinderResult>(ret);

	        //Parallel.ForEach(_filteredPersons, person => runOnePerson(person, ret));
	        foreach (var person in _filteredPersons)
	        {
		        runOnePerson(person, ret);
	        }

			return new List<AvailableHourlyEmployeeFinderResult>(ret);
		}

		private void runOnePerson(IPerson person, BlockingCollection<AvailableHourlyEmployeeFinderResult> ret)
		{
			if (person.Equals(_sourcePerson))
				return;
			if (!isHourly(person))
				return;
			if (isScheduled(person))
				return;

			var result = new AvailableHourlyEmployeeFinderResult(person, hasMatchingAvailibility(person),
																 availabilityTimes(person), workTimesYesterday(person), workTimesTomorrow(person),
			                                                     nightRest(person));
			ret.Add(result);
		}

		private string workTimesYesterday(IPerson person)
		{
			IScheduleDay yesterday = _resultStateHolder.Schedules[person].ScheduledDay(_dateOnly.AddDays(-1));
			if (yesterday.SignificantPartForDisplay() != SchedulePartView.MainShift)
				return string.Empty;

			return yesterday.ProjectionService().CreateProjection().Period().Value.TimePeriodLocal().ToShortTimeString();
		}

		private string workTimesTomorrow(IPerson person)
		{
			IScheduleDay tomorrow = _resultStateHolder.Schedules[person].ScheduledDay(_dateOnly.AddDays(1));
			if (tomorrow.SignificantPartForDisplay() != SchedulePartView.MainShift)
				return string.Empty;

			return tomorrow.ProjectionService().CreateProjection().Period().Value.TimePeriodLocal().ToShortTimeString();
		}

		private string availabilityTimes(IPerson person)
		{
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_resultStateHolder);
			restrictionExtractor.Extract(person, _dateOnly);
			if (!restrictionExtractor.StudentAvailabilityList.Any())
				return string.Empty;

			var availability = restrictionExtractor.StudentAvailabilityList.First().RestrictionCollection[0];
			return availability.StartTimeLimitation.StartTimeString + " - " + availability.EndTimeLimitation.EndTimeString;
		}

		private bool isScheduled(IPerson person)
		{
            return _resultStateHolder.Schedules[person].ScheduledDay(_dateOnly).IsScheduled();
		}

		private bool isHourly(IPerson person)
		{
			IPersonPeriod period = person.Period(_dateOnly);
			if (period == null)
				return false;
			if (period.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)
				return false;

			return true;
		}

		private bool hasMatchingAvailibility(IPerson person)
		{
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_resultStateHolder);
			restrictionExtractor.Extract(person, _dateOnly);
			if (!restrictionExtractor.StudentAvailabilityList.Any())
				return false;

            IScheduleDay sourceScheduleDay = _resultStateHolder.Schedules[_sourcePerson].ScheduledDay(_dateOnly);
			IVisualLayerCollection visualLayerCollection = sourceScheduleDay.ProjectionService().CreateProjection();
			if(!visualLayerCollection.Period().HasValue)
				return false;

			DateTimePeriod period = visualLayerCollection.Period().Value;
			TimeZoneInfo tzInfo = person.PermissionInformation.DefaultTimeZone();
			DateTime baseDate = _dateOnly.Date;
			TimeSpan startTime = period.StartDateTimeLocal(tzInfo).Subtract(baseDate);
			TimeSpan endTime = period.EndDateTimeLocal(tzInfo).Subtract(baseDate);

			foreach (IStudentAvailabilityDay studentAvailabilityDay in restrictionExtractor.StudentAvailabilityList)
			{
				foreach (IStudentAvailabilityRestriction restriction in studentAvailabilityDay.RestrictionCollection)
				{
					
					if (!restriction.StartTimeLimitation.IsValidFor(startTime))
						return false;
					if (!restriction.EndTimeLimitation.IsValidFor(endTime))
						return false;
					if (!restriction.WorkTimeLimitation.IsValidFor(visualLayerCollection.ContractTime()))
						return false;
				}
			}
			
			return true;
		}

		private bool nightRest(IPerson person)
		{
			var nightlyRest = person.Period(_dateOnly).PersonContract.Contract.WorkTimeDirective.NightlyRest;
			var sourceDayPeriod =
				_resultStateHolder.Schedules[_sourcePerson].ScheduledDay(_dateOnly)
				                                           .ProjectionService()
				                                           .CreateProjection()
				                                           .Period()
				                                           .Value;
			if (!nightRestFromYesterday(person, nightlyRest, sourceDayPeriod))
				return false;

			if (!nightRestTillTomorrow(person, nightlyRest, sourceDayPeriod))
				return false;

			return true;
		}

		private bool nightRestFromYesterday(IPerson person, TimeSpan nightlyRest, DateTimePeriod sourceDayPeriod)
		{
			//IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_resultStateHolder);
			//restrictionExtractor.Extract(person, _dateOnly);
			//if (!restrictionExtractor.StudentAvailabilityList.Any())
			//	return true;

			var scheduleDay = _resultStateHolder.Schedules[person].ScheduledDay(_dateOnly.AddDays(-1));
			if (!scheduleDay.IsScheduled())
				return true;

			var sourceStart = sourceDayPeriod.StartDateTime;
			var yesterdayEnd = scheduleDay.ProjectionService().CreateProjection().Period().Value.EndDateTime;
			if (sourceStart.Subtract(yesterdayEnd) < nightlyRest)
				return false;

			return true;
		}

		private bool nightRestTillTomorrow(IPerson person, TimeSpan nightlyRest, DateTimePeriod sourceDayPeriod)
		{
			var scheduleDay = _resultStateHolder.Schedules[person].ScheduledDay(_dateOnly.AddDays(1));
			if (!scheduleDay.IsScheduled())
				return true;

			var sourceEnd = sourceDayPeriod.EndDateTime;
			var tomorrowStart = scheduleDay.ProjectionService().CreateProjection().Period().Value.StartDateTime;
			if (tomorrowStart.Subtract(sourceEnd) < nightlyRest)
				return false;

			return true;
		}

	}
}