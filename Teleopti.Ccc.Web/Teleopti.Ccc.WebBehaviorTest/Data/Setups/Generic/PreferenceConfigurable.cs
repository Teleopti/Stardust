using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PreferenceConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public bool IsExtended { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var restriction = new PreferenceRestriction();

			if (IsExtended)
				restriction.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(8));
			
			var preferenceDay = new PreferenceDay(user, new DateOnly(Date), restriction);

			var preferenceDayRepository = new PreferenceDayRepository(uow);
			preferenceDayRepository.Add(preferenceDay);
		}
	}
}