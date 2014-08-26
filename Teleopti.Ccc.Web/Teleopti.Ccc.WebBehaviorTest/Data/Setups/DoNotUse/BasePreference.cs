using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public abstract class BasePreference : IUserDataSetup
	{
		protected static readonly CultureInfo SwedishCultureInfo = CultureInfo.GetCultureInfo(1053);

		protected BasePreference()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(SwedishCultureInfo);
		}

		public string Date { get; set; }

		protected abstract PreferenceRestriction ApplyRestriction(IUnitOfWork uow);
		protected abstract DateTime ApplyDate(CultureInfo cultureInfo);

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var date = ApplyDate(cultureInfo);

			var preferenceDay = new PreferenceDay(user, new DateOnly(date), ApplyRestriction(uow));

			var preferenceDayRepository = new PreferenceDayRepository(uow);
			preferenceDayRepository.Add(preferenceDay);
		}
	}
}