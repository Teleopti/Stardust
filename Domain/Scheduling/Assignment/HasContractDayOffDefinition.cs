using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class HasContractDayOffDefinition : IHasContractDayOffDefinition
	{
		public bool IsDayOff(IScheduleDay scheduleDay)
		{
			var dateOnlyAsPeriod = scheduleDay.DateOnlyAsPeriod;
			if (dateOnlyAsPeriod == null)
				return false;

			var dateOnly = dateOnlyAsPeriod.DateOnly;

			var person = scheduleDay.Person;

			var personPeriod = person?.Period(dateOnly);

			var personContract = personPeriod?.PersonContract;

			var contract = personContract?.Contract;
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

			return !contractSchedule.IsWorkday(periodstartDate.Value, dateOnly, person.FirstDayOfWeek);
		}
	}
}