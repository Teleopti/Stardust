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
			Extra = "0";
			BalanceIn = "0";
			BalanceOut = "0";
			Extra = "0";
		}

		public PersonAbsenceAccountConfigurable(PersonAbsenceAccountConfigurable config)
		{
			Absence = config.Absence;
			FromDate = config.FromDate;
			Accrued = config.Accrued;
			BalanceIn = config.BalanceIn;
			BalanceOut = config.BalanceOut;
			Extra = config.Extra;
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
						Accrued = TimeSpan.Parse(Accrued),
						BalanceIn = TimeSpan.Parse(BalanceIn),
						BalanceOut = TimeSpan.Parse(BalanceOut),
						Extra = TimeSpan.Parse(Extra)
					});
			}

			var repository = new PersonAbsenceAccountRepository(uow);
			repository.Add(personAbsenceAccount);
		}
	}
}
