using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
				var intersectionBetweenRequestedPeriodAndAccountPeriod = account.Period().Intersection(period);
				if (!intersectionBetweenRequestedPeriodAndAccountPeriod.HasValue) continue;	

				var intersectingPeriod = account.Period().Intersection(rangePeriod);
				if (intersectingPeriod.HasValue)
				{
					IList<IScheduleDay> scheduleDays =
						new List<IScheduleDay>(scheduleRange.ScheduledDayCollection(intersectingPeriod.Value));

					account.Owner.Absence.Tracker.Track(account, account.Owner.Absence, scheduleDays);
					return !account.IsExceeded;
				}
			}
			return false;
		}
	}
}