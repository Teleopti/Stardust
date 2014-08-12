using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class AbsencePreference : BasePreference
	{
		public AbsencePreference()
		{
			Absence = TestData.Absence.Description.Name;
		}

		public string Absence { get; set; }

		protected override PreferenceRestriction ApplyRestriction(IUnitOfWork uow)
		{
			var rep = new AbsenceRepository(uow);
			return new PreferenceRestriction { Absence = rep.LoadAll().FirstOrDefault(a => a.Description.Name == Absence) };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
            return DateTime.Parse(Date,SwedishCultureInfo);
		}
	}
}