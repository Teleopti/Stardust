using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class AbsenceAccountFactory
	{
		/// <param name="periodLength">Length of absence account period, set to 0 or negative number make it no period end.</param>
		public static AccountDay CreateAbsenceAccountDays(IPerson person, IAbsence absence, DateOnly startDate,
			TimeSpan periodLength, TimeSpan balanceIn, TimeSpan balanceOut, TimeSpan accrued, TimeSpan extra, TimeSpan used)
		{
			var absenceAccount = new PersonAbsenceAccount(person, absence);
			absenceAccount.Absence.Tracker = Tracker.CreateDayTracker();

			absenceAccount.Add(new AccountDay(startDate)
			{
				BalanceIn = balanceIn,
				BalanceOut = balanceOut,
				Accrued = accrued,
				LatestCalculatedBalance = used
			});

			if (periodLength.TotalSeconds > 0)
			{
				absenceAccount.Add(new AccountDay(new DateOnly(startDate.Date.Add(periodLength)))
					{
						BalanceIn = balanceIn,
						BalanceOut = balanceOut,
						Accrued = accrued,
						LatestCalculatedBalance = used
					});
			}

			var result = (AccountDay)absenceAccount.AccountCollection().Last();
			return result;
		}


		/// <param name="periodLength">Length of absence account period, set to 0 or negative number make it no period end.</param>
		public static AccountTime CreateAbsenceAccountHours(IPerson person, IAbsence absence, DateOnly startDate,
			TimeSpan periodLength, TimeSpan balanceIn, TimeSpan balanceOut, TimeSpan accrued, TimeSpan extra, TimeSpan used)
		{
			var absenceAccount = new PersonAbsenceAccount(person, absence);
			absenceAccount.Absence.Tracker = Tracker.CreateTimeTracker();

			absenceAccount.Add(new AccountTime(startDate)
			{
				BalanceIn = balanceIn,
				BalanceOut = balanceOut,
				Accrued = accrued,
				LatestCalculatedBalance = used
			});

			if (periodLength.TotalSeconds > 0)
			{
				absenceAccount.Add(new AccountTime(new DateOnly(startDate.Date.Add(periodLength)))
				{
					BalanceIn = balanceIn,
					BalanceOut = balanceOut,
					Accrued = accrued,
					LatestCalculatedBalance = used
				});
			}

			var result = (AccountTime) absenceAccount.AccountCollection().Last();
			return result;
		}
	}
}
