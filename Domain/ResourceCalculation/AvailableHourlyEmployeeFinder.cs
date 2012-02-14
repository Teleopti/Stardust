using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class AvailableHourlyEmployeeFinderResult
	{
		private readonly IPerson _person;
		private readonly bool _matching;
		private readonly IStudentAvailabilityDay _studentAvailabilityDay;

		public AvailableHourlyEmployeeFinderResult(IPerson person, bool matching, IStudentAvailabilityDay studentAvailabilityDay)
		{
			_person = person;
			_matching = matching;
			_studentAvailabilityDay = studentAvailabilityDay;
		}

		public IPerson Person
		{
			get { return _person; }
		}

		public bool Matching
		{
			get { return _matching; }
		}

		public IStudentAvailabilityDay StudentAvailabilityDay
		{
			get { return _studentAvailabilityDay; }
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
			IList<AvailableHourlyEmployeeFinderResult> ret = new List<AvailableHourlyEmployeeFinderResult>();
            IScheduleDay source = _resultStateHolder.Schedules[_sourcePerson].ScheduledDay(_dateOnly);
			if (source.SignificantPart() != SchedulePartView.MainShift)
				return ret;

			foreach (var person in _filteredPersons)
			{
				if(person.Equals(_sourcePerson))
					continue;
				if(!isHourly(person))
					continue;
				if(isScheduled(person))
					continue;
				IStudentAvailabilityDay prefernece = hasAvailibility(person);
				if (prefernece != null)
				{
					AvailableHourlyEmployeeFinderResult result;
					if (hasMatchingAvailibility(person))
					{
						result = new AvailableHourlyEmployeeFinderResult(person, true, prefernece);
					}
					else
					{
						result = new AvailableHourlyEmployeeFinderResult(person, false, prefernece);
					}

					ret.Add(result);
				}
			}

			return ret;
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

		private IStudentAvailabilityDay hasAvailibility(IPerson person)
		{
            IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_resultStateHolder);
			restrictionExtractor.Extract(person, _dateOnly);
			if (restrictionExtractor.StudentAvailabilityList.Count() == 0)
				return null;

			return restrictionExtractor.StudentAvailabilityList.FirstOrDefault();
		}

		private bool hasMatchingAvailibility(IPerson person)
		{
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_resultStateHolder);
			restrictionExtractor.Extract(person, _dateOnly);
			if (restrictionExtractor.StudentAvailabilityList.Count() == 0)
				return false;

            IScheduleDay sourceScheduleDay = _resultStateHolder.Schedules[_sourcePerson].ScheduledDay(_dateOnly);
			IVisualLayerCollection visualLayerCollection = sourceScheduleDay.ProjectionService().CreateProjection();
			if(!visualLayerCollection.Period().HasValue)
				return false;

			DateTimePeriod period = visualLayerCollection.Period().Value;
			ICccTimeZoneInfo tzInfo = person.PermissionInformation.DefaultTimeZone();
			DateTime baseDate = _dateOnly.Date;
			TimeSpan startTime = period.StartDateTimeLocal(tzInfo).Subtract(baseDate);
			TimeSpan endTime = period.EndDateTimeLocal(tzInfo).Subtract(baseDate);

			foreach (IStudentAvailabilityDay studentAvailabilityDay in restrictionExtractor.StudentAvailabilityList)
			{
				foreach (IStudentAvailabilityRestriction restriction in studentAvailabilityDay.RestrictionCollection)
				{
					
					if (!restriction.StartTimeLimitation.ValidPeriod().Contains(startTime))
						return false;
					if (!restriction.EndTimeLimitation.ValidPeriod().Contains(endTime))
						return false;
					if (!restriction.WorkTimeLimitation.ValidPeriod().Contains(visualLayerCollection.ContractTime()))
						return false;
				}
			}
			
			return true;
		}
	}
}