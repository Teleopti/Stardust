using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IHasDayOffDefinition
	{
		bool IsDayOff();
	}

	public class HasDayOffDefinition : IHasDayOffDefinition
	{
        private readonly IScheduleDay _scheduleDay;

        public HasDayOffDefinition(IScheduleDay scheduleDay)
        {
            InParameter.NotNull("scheduleDay", scheduleDay);
            _scheduleDay = scheduleDay;
        }

        public bool IsDayOff()
        {
            var dateOnly = _scheduleDay.DateOnlyAsPeriod.DateOnly;
            var personPeriod = _scheduleDay.Person.Period(dateOnly);

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

            return !contractSchedule.IsWorkday(personPeriod.StartDate, dateOnly);
        }
    }
}