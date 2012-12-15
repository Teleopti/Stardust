using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IHasDayOffDefinition
	{
		bool IsDayOff();
		bool IsDayOff(IScheduleDay scheduleDay);
	}

	public class HasDayOffDefinition : IHasDayOffDefinition
	{
		private readonly IScheduleDay _scheduleDay;

		public HasDayOffDefinition()
		{}

		public HasDayOffDefinition(IScheduleDay scheduleDay)
		{
			InParameter.NotNull("scheduleDay", scheduleDay);
			_scheduleDay = scheduleDay;
		}

		public bool IsDayOff()
		{
			return IsDayOff(_scheduleDay);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool IsDayOff(IScheduleDay scheduleDay)
		{
			var dateOnlyAsPeriod = scheduleDay.DateOnlyAsPeriod;
			if (dateOnlyAsPeriod == null)
				return false;

			var dateOnly = dateOnlyAsPeriod.DateOnly;

			var person = scheduleDay.Person;
			if (person == null)
				return false;

			var personPeriod = person.Period(dateOnly);

			if (personPeriod == null)
				return false;

			var personContract = personPeriod.PersonContract;
			if (personContract == null)
				return false;

			var contract = personContract.Contract;
			if (contract == null)
				return false;

			if (contract.EmploymentType == EmploymentType.HourlyStaff)
				return false;

			var contractSchedule = personContract.ContractSchedule;
			if (contractSchedule == null)
				return false;

			var periodstartDate = person.SchedulePeriodStartDate(dateOnly);
			if (!periodstartDate.HasValue)
				return false;

			return !contractSchedule.IsWorkday(periodstartDate.Value, dateOnly);
		}
	}
}