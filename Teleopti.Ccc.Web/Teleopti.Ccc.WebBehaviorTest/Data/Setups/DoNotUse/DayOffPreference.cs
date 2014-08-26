using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class DayOffPreference : BasePreference
	{
		public string DayOffTemplate { get; set; }

		protected override PreferenceRestriction ApplyRestriction(IUnitOfWork uow)
		{
			IDayOffTemplate dayOffTemplate;
			if (DayOffTemplate != null)
			{
				dayOffTemplate = new DayOffTemplateRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(DayOffTemplate));
			}
			else
			{
				dayOffTemplate = DayOffFactory.CreateDayOff(new Description(RandomName.Make(), RandomName.Make()));
				var activityRepository = new ActivityRepository(uow);
				activityRepository.Add(dayOffTemplate);
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