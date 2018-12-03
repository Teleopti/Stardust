using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class VirtualSchedulePeriodSplitChecker : IVirtualSchedulePeriodSplitChecker
    {
        private DateOnlyPeriod _schedulePeriod;
        private readonly IPerson _person;

        public VirtualSchedulePeriodSplitChecker(IPerson person)
        {
            _person = person;
        }

        public DateOnlyPeriod? Check(DateOnlyPeriod schedulePeriod, DateOnly dateOnly)
        {
            var personPeriod = _person.Period(dateOnly);

            if (personPeriod == null)
                return null;

            _schedulePeriod = schedulePeriod;

            var startDate = CheckNonBreakingPersonPeriodStartDate(personPeriod);
            var endDate = CheckNonBreakingPersonPeriodEndDate(personPeriod);

            if (startDate < _schedulePeriod.StartDate)
                startDate = _schedulePeriod.StartDate;

            if (endDate > _schedulePeriod.EndDate)
                endDate = _schedulePeriod.EndDate;

            endDate = CheckTerminalDate(endDate);
            endDate = CheckConflictNextSchedulePeriod(endDate);

            return new DateOnlyPeriod(startDate, endDate);
        }

        private DateOnly CheckConflictNextSchedulePeriod(DateOnly endDate)
        {
            foreach (var schedulePeriod in _person.PersonSchedulePeriodCollection)
            {
                if(schedulePeriod.DateFrom > _schedulePeriod.StartDate && schedulePeriod.DateFrom <= endDate)
                {
                    return schedulePeriod.DateFrom.AddDays(-1);
                }
            }

            return endDate;
        }

        private DateOnly CheckTerminalDate(DateOnly endDate)
        {
            if (_person.TerminalDate.HasValue && _person.TerminalDate.Value < endDate)
                return _person.TerminalDate.Value;

            return endDate;
        }

        private DateOnly CheckNonBreakingPersonPeriodStartDate(IPersonPeriod personPeriod)
        {
            var startDate = personPeriod.StartDate;

            var previousPeriod = _person.PreviousPeriod(personPeriod);
            
            if(previousPeriod == null)
            {
                return startDate;
            }

	        if (previousPeriod.PersonContract == null || personPeriod.PersonContract == null)
	        {
		        return startDate;
	        }

            if(previousPeriod.PersonContract.ContractSchedule == personPeriod.PersonContract.ContractSchedule && 
                previousPeriod.PersonContract.Contract == personPeriod.PersonContract.Contract &&
                previousPeriod.PersonContract.PartTimePercentage == personPeriod.PersonContract.PartTimePercentage)
            {
                startDate = CheckNonBreakingPersonPeriodStartDate(previousPeriod);
            }

            return startDate;
        }

        private DateOnly CheckNonBreakingPersonPeriodEndDate(IPersonPeriod personPeriod)
        {
            var nextPeriod = _person.NextPeriod(personPeriod);

            if (nextPeriod == null || nextPeriod.StartDate.AddDays(-1) > _schedulePeriod.EndDate)
                return _schedulePeriod.EndDate;

            var endDate = nextPeriod.StartDate.AddDays(-1);

            if (nextPeriod.PersonContract.ContractSchedule == personPeriod.PersonContract.ContractSchedule &&
                nextPeriod.PersonContract.Contract == personPeriod.PersonContract.Contract &&
                nextPeriod.PersonContract.PartTimePercentage == personPeriod.PersonContract.PartTimePercentage)
            {
                endDate = CheckNonBreakingPersonPeriodEndDate(nextPeriod);
            }

            return endDate;
        }
    }
}
