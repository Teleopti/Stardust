using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


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

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var absence = AbsenceRepository.DONT_USE_CTOR(unitOfWork).LoadAll().Single(x => x.Description.Name == Absence);
			var repository = new PersonAbsenceAccountRepository(unitOfWork);
			var result = repository.Find(person);
			IPersonAbsenceAccount personAbsenceAccount = null;
			if (result.Any())
			{
				personAbsenceAccount = result.FirstOrDefault(x => x.Absence.Id == absence.Id);
			}
			if (personAbsenceAccount == null)
			{
				personAbsenceAccount = new PersonAbsenceAccount(person, absence);
				repository.Add(personAbsenceAccount);
			}

			var trackerType = absence.Tracker != null ? absence.Tracker.GetType() : null; 
			if (trackerType == Tracker.CreateDayTracker().GetType())
			{
				personAbsenceAccount.Add(new AccountDay(new DateOnly(DateTime.Parse(FromDate)))
					{
						Accrued = TimeSpan.Parse(Accrued),
						BalanceIn = TimeSpan.Parse(BalanceIn),
						BalanceOut = TimeSpan.Parse(BalanceOut),
						Extra = TimeSpan.Parse(Extra)
					});
			}
			else if (trackerType == Tracker.CreateTimeTracker().GetType())
			{
				personAbsenceAccount.Add(new AccountTime(new DateOnly(DateTime.Parse(FromDate)))
					{
						Accrued = getTimeSpanInMinute(Accrued),
						BalanceIn = getTimeSpanInMinute(BalanceIn),
						BalanceOut = getTimeSpanInMinute(BalanceOut),
						Extra = getTimeSpanInMinute(Extra)
					});
			}


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

			return TimeSpan.FromMinutes(timeInMinutes);
		}
	}
}
