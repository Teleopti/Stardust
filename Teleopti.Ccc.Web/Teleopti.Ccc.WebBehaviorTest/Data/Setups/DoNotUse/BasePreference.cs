using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;


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

		protected abstract PreferenceRestriction ApplyRestriction(ICurrentUnitOfWork currentUnitOfWork);
		protected abstract DateTime ApplyDate(CultureInfo cultureInfo);

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var date = ApplyDate(cultureInfo);

			var preferenceDay = new PreferenceDay(person, new DateOnly(date), ApplyRestriction(unitOfWork));

			var preferenceDayRepository = new PreferenceDayRepository(unitOfWork);
			preferenceDayRepository.Add(preferenceDay);
		}
	}
}