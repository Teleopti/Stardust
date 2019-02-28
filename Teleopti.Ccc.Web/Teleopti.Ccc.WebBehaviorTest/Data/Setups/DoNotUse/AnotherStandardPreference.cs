using System;
using System.Drawing;
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
	public class AnotherStandardPreference : BasePreference
	{
		public string Preference;

		protected override PreferenceRestriction ApplyRestriction(ICurrentUnitOfWork currentUnitOfWork)
		{
			IAbsence absence;
			if (Preference != null)
			{
				absence = AbsenceRepository.DONT_USE_CTOR(currentUnitOfWork).LoadAll().Single(a => a.Name == Preference);
			}
			else
			{
				absence = AbsenceFactory.CreateAbsence(RandomName.Make(), RandomName.Make(), Color.FromArgb(210, 150, 150));
				var absenceRepository = AbsenceRepository.DONT_USE_CTOR(currentUnitOfWork);
				absenceRepository.Add(absence);
				Preference = absence.Description.Name;
			}
			
			return new PreferenceRestriction { Absence = absence };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateHelper.GetFirstDateInWeek(DateTime.Parse(Date,SwedishCultureInfo), cultureInfo).AddDays(1);
		}
	}
}