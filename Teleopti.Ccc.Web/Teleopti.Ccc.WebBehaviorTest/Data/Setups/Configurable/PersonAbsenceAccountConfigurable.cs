using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonAbsenceAccountConfigurable : IUserDataSetup
	{
		public string Absence { get; set; }
		public string FromDate { get; set; }

		public string Accrued { get; set; }
		public string BalanceIn { get; set; }
		public string BalanceOut { get; set; }
		public string Extra { get; set; }

		public PersonAbsenceAccountConfigurable()
		{
			Accrued = "0";
			BalanceIn = "0";
			BalanceOut = "0";
			Extra = "0";
		}

		public PersonAbsenceAccountConfigurable(PersonAbsenceAccountConfigurable config)
		{
			Absence = config.Absence;
			FromDate = config.FromDate;
			Accrued = !string.IsNullOrEmpty(config.Accrued) ? config.Accrued : "0";
			BalanceIn = !string.IsNullOrEmpty(config.BalanceIn) ? config.BalanceIn : "0";
			BalanceOut = !string.IsNullOrEmpty(config.BalanceOut) ? config.BalanceOut : "0";
			Extra = !string.IsNullOrEmpty(config.Extra) ? config.Extra : "0";
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var absence = new AbsenceRepository(uow).LoadAll().Single(x => x.Description.Name == Absence);
			var personAbsenceAccount = new PersonAbsenceAccount(user, absence);

			var trackerType = absence.Tracker != null ? absence.Tracker.GetType() : null; 
			if (trackerType == Tracker.CreateDayTracker().GetType())
			{
				personAbsenceAccount.Add(new AccountDay(FromDate)
					{
						Accrued = TimeSpan.Parse(Accrued),
						BalanceIn = TimeSpan.Parse(BalanceIn),
						BalanceOut = TimeSpan.Parse(BalanceOut),
						Extra = TimeSpan.Parse(Extra)
					});
			}
			else if (trackerType == Tracker.CreateTimeTracker().GetType())
			{
				personAbsenceAccount.Add(new AccountTime(FromDate)
					{
						Accrued = getTimeSpanInMinute(Accrued),
						BalanceIn = getTimeSpanInMinute(BalanceIn),
						BalanceOut = getTimeSpanInMinute(BalanceOut),
						Extra = getTimeSpanInMinute(Extra)
					});
			}

			var repository = new PersonAbsenceAccountRepository(uow);
			repository.Add(personAbsenceAccount);
		}

		private static TimeSpan getTimeSpanInMinute(string ts)
		{
			var timeInMinutes = 0;
			var formatIsCorrect = false;

			if (ts == "0")
			{
				formatIsCorrect = true;
				timeInMinutes = 0;
			}
			else if (ts.Contains(':'))
			{
				var values = ts.Split(':');
				if (values.Length == 2)
				{
					int hourCount, minuteCount;
					if (int.TryParse(values[0], out hourCount) && int.TryParse(values[1], out minuteCount) && (minuteCount >= 0 && minuteCount < 60))
					{
						formatIsCorrect = true;
						timeInMinutes = hourCount * 60 + minuteCount;
					}
				}
			}

			if (!formatIsCorrect)
			{
				throw new ArgumentException(string.Format("TimeSpan value {0} should match \"HH:mm\" format.", ts));
			}

			return new TimeSpan(timeInMinutes, 0, 0);
		}
	}
}
