using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class DayOffPreference : BasePreference
	{
		public DayOffPreference()
		{
			DayOffTemplate = TestData.DayOffTemplate.Description.Name;
		}

		public string DayOffTemplate { get; set; }

		protected override PreferenceRestriction ApplyRestriction(IUnitOfWork uow)
		{
			var rep = new DayOffTemplateRepository(uow);
			return new PreferenceRestriction {DayOffTemplate = rep.LoadAll().FirstOrDefault(d => d.Description.Name == DayOffTemplate)};
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateTime.Parse(Date,SwedishCultureInfo);
		}
	}
}