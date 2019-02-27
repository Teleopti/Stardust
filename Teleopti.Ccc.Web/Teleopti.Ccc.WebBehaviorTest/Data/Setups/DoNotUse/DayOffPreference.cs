using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class DayOffPreference : BasePreference
	{
		public string DayOffTemplate { get; set; }

		protected override PreferenceRestriction ApplyRestriction(ICurrentUnitOfWork currentUnitOfWork)
		{
			IDayOffTemplate dayOffTemplate;
			if (DayOffTemplate != null)
			{
				dayOffTemplate = DayOffTemplateRepository.DONT_USE_CTOR2(currentUnitOfWork).LoadAll().Single(sCat => sCat.Description.Name.Equals(DayOffTemplate));
			}
			else
			{
				dayOffTemplate = DayOffFactory.CreateDayOff(new Description(RandomName.Make(), RandomName.Make()));
				var repository = DayOffTemplateRepository.DONT_USE_CTOR2(currentUnitOfWork);
				repository.Add(dayOffTemplate);
				DayOffTemplate = dayOffTemplate.Description.Name;
			}

			return new PreferenceRestriction { DayOffTemplate = dayOffTemplate };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateTime.Parse(Date,SwedishCultureInfo);
		}
	}
}