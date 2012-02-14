using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class PersonAccountBalanceCalculator : IPersonAccountBalanceCalculator
    {
        private readonly IEnumerable<IAccount> _accounts;

        public PersonAccountBalanceCalculator(IEnumerable<IAccount> accounts)
        {
            _accounts = accounts;
        }

        public bool CheckBalance(IScheduleRange scheduleRange, DateOnlyPeriod period)
        {
            var rangePeriod = scheduleRange.Period.ToDateOnlyPeriod(scheduleRange.Person.PermissionInformation.DefaultTimeZone());

            foreach (IAccount account in _accounts)
            {
                var intersectingPeriod = account.Period().Intersection(rangePeriod);
                if (intersectingPeriod.HasValue)
                {
                    IList<IScheduleDay> scheduleDays =
                        new List<IScheduleDay>(scheduleRange.ScheduledDayCollection(intersectingPeriod.Value));

                    account.Owner.Absence.Tracker.Track(account, account.Owner.Absence, scheduleDays);
                    bool check1 = account.IsExceeded;
                    bool check2 = account.IsExceeded;
                    if (check1 || check2) return false; //Micke kollar på varför vi måste göra så här...
                }
            }
            return true;
        }
    }
}