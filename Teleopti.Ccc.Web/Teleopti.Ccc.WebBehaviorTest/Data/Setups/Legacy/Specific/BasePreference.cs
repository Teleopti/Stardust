using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public abstract class BasePreference : IUserDataSetup
	{
		public DateTime Date;

		protected abstract PreferenceRestriction ApplyRestriction();
		protected abstract DateTime ApplyDate(CultureInfo cultureInfo);

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			Date = ApplyDate(cultureInfo);

			var preferenceDay = new PreferenceDay(user, new DateOnly(Date), ApplyRestriction());

			var preferenceDayRepository = new PreferenceDayRepository(uow);
			preferenceDayRepository.Add(preferenceDay);
		}
	}
}