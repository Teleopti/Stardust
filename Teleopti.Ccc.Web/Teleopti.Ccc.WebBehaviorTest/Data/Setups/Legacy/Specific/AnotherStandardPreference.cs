using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class AnotherStandardPreference : BasePreference
	{
		public string Preference = TestData.Absence.Description.Name;

		protected override PreferenceRestriction ApplyRestriction(IUnitOfWork uow)
		{
			var rep = new AbsenceRepository(uow);
			return new PreferenceRestriction { Absence = rep.LoadAll().FirstOrDefault(a => a.Description.Name == Preference) };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateHelper.GetFirstDateInWeek(DateTime.Parse(Date,SwedishCultureInfo), cultureInfo).AddDays(1);
		}
	}
}