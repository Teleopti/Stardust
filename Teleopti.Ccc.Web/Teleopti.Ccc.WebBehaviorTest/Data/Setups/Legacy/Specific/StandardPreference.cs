using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class StandardPreference : BasePreference
	{
		public string Preference = TestData.DayOffTemplate.Description.Name;

		protected override PreferenceRestriction ApplyRestriction(IUnitOfWork uow)
		{
			var rep = new DayOffTemplateRepository(uow);
			return new PreferenceRestriction { DayOffTemplate = rep.LoadAll().FirstOrDefault(d => d.Description.Name == Preference) };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateHelper.GetFirstDateInWeek(DateTime.Parse(Date,SwedishCultureInfo), cultureInfo);
		}
	}
}