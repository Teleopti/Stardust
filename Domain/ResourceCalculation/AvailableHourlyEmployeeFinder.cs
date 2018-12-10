using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

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
		private readonly IRestrictionExtractor _restrictionExtractor;
		private readonly IPerson _sourcePerson;
		private readonly DateOnly _dateOnly;
	    private readonly IScheduleDayForPerson _scheduleDayForPerson;
	    private readonly ICollection<IPerson> _filteredPersons;
		private readonly IUserTimeZone _userTimeZone;

		public AvailableHourlyEmployeeFinder(IRestrictionExtractor restrictionExtractor, IPerson sourcePerson, DateOnly dateOnly, IScheduleDayForPerson scheduleDayForPerson, ICollection<IPerson> filteredPersons, IUserTimeZone userTimeZone)
		{
			_restrictionExtractor = restrictionExtractor;
			_sourcePerson = sourcePerson;
			_dateOnly = dateOnly;
		    _scheduleDayForPerson = scheduleDayForPerson;
		    _filteredPersons = filteredPersons;
			_userTimeZone = userTimeZone;
		}

        public IList<AvailableHourlyEmployeeFinderResult> Find()
        {
	        var ret = new BlockingCollection<AvailableHourlyEmployeeFinderResult>();
            IScheduleDay source = _scheduleDayForPerson.ForPerson(_sourcePerson,_dateOnly);
			if (source.SignificantPart() != SchedulePartView.MainShift)
				return new List<AvailableHourlyEmployeeFinderResult>(ret);

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

			var targetScheduleDay = _scheduleDayForPerson.ForPerson(person,_dateOnly); 
			if (targetScheduleDay.IsScheduled())
				return;

			var result = new AvailableHourlyEmployeeFinderResult(person, hasMatchingAvailibility(targetScheduleDay),
																 availabilityTimes(targetScheduleDay), workTimesYesterday(person), workTimesTomorrow(person),
			                                                     nightRest(person));
			ret.Add(result);
		}

		private string workTimesYesterday(IPerson person)
		{
			IScheduleDay yesterday = _scheduleDayForPerson.ForPerson(person,_dateOnly.AddDays(-1));
			if (yesterday.SignificantPartForDisplay() != SchedulePartView.MainShift)
				return string.Empty;

			return yesterday.ProjectionService().CreateProjection().Period().Value.TimePeriod(_userTimeZone.TimeZone()).ToShortTimeString();
		}

		private string workTimesTomorrow(IPerson person)
		{
			IScheduleDay tomorrow = _scheduleDayForPerson.ForPerson(person,_dateOnly.AddDays(1));
			if (tomorrow.SignificantPartForDisplay() != SchedulePartView.MainShift)
				return string.Empty;

			return tomorrow.ProjectionService().CreateProjection().Period().Value.TimePeriod(_userTimeZone.TimeZone()).ToShortTimeString();
		}

		private string availabilityTimes(IScheduleDay targetScheduleDay)
		{
			var result = _restrictionExtractor.Extract(targetScheduleDay);
			if (!result.StudentAvailabilityList.Any())
				return string.Empty;

			var availability = result.StudentAvailabilityList.First().RestrictionCollection[0];
			return availability.StartTimeLimitation.StartTimeString + " - " + availability.EndTimeLimitation.EndTimeString;
		}

		private bool isHourly(IPerson person)
		{
			IPersonPeriod period = person.Period(_dateOnly);
			return period?.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff;
		}

		private bool hasMatchingAvailibility(IScheduleDay targetScheduleDay)
		{
			var result = _restrictionExtractor.Extract(targetScheduleDay);
			if (!result.StudentAvailabilityList.Any())
				return false;

            IScheduleDay sourceScheduleDay = _scheduleDayForPerson.ForPerson(_sourcePerson,_dateOnly);
			IVisualLayerCollection visualLayerCollection = sourceScheduleDay.ProjectionService().CreateProjection();
			var layerPeriod = visualLayerCollection.Period();
			if(!layerPeriod.HasValue)
				return false;

			DateTimePeriod period = layerPeriod.Value;
			TimeZoneInfo tzInfo = targetScheduleDay.Person.PermissionInformation.DefaultTimeZone();
			DateTime baseDate = _dateOnly.Date;
			TimeSpan startTime = period.StartDateTimeLocal(tzInfo).Subtract(baseDate);
			TimeSpan endTime = period.EndDateTimeLocal(tzInfo).Subtract(baseDate);

			foreach (IStudentAvailabilityDay studentAvailabilityDay in result.StudentAvailabilityList)
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
				_scheduleDayForPerson.ForPerson(_sourcePerson,_dateOnly)
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
			var scheduleDay = _scheduleDayForPerson.ForPerson(person,_dateOnly.AddDays(-1));
			if (!scheduleDay.IsScheduled())
				return true;

			var sourceStart = sourceDayPeriod.StartDateTime;
			var yesterdayEnd = scheduleDay.ProjectionService().CreateProjection().Period().Value.EndDateTime;
			
			return sourceStart.Subtract(yesterdayEnd) >= nightlyRest;
		}

		private bool nightRestTillTomorrow(IPerson person, TimeSpan nightlyRest, DateTimePeriod sourceDayPeriod)
		{
			var scheduleDay = _scheduleDayForPerson.ForPerson(person,_dateOnly.AddDays(1));
			if (!scheduleDay.IsScheduled())
				return true;

			var sourceEnd = sourceDayPeriod.EndDateTime;
			var tomorrowStart = scheduleDay.ProjectionService().CreateProjection().Period().Value.StartDateTime;
			
			return tomorrowStart.Subtract(sourceEnd) >= nightlyRest;
		}

	}
}