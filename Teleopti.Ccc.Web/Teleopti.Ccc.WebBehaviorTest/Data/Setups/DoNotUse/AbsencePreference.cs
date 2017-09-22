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
	public class AbsencePreference : BasePreference
	{
		
		public string Absence { get; set; }

		protected override PreferenceRestriction ApplyRestriction(ICurrentUnitOfWork currentUnitOfWork)
		{
			IAbsence absence;
			if (Absence != null)
			{
				absence = new AbsenceRepository(currentUnitOfWork).LoadAll().Single(a => a.Name == Absence);
			}
			else
			{
				absence = AbsenceFactory.CreateAbsence(RandomName.Make(), RandomName.Make(), Color.FromArgb(210, 150, 150));
				var absenceRepository = new AbsenceRepository(currentUnitOfWork);
				absenceRepository.Add(absence);
			}
			return new PreferenceRestriction { Absence = absence };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
            return DateTime.Parse(Date,SwedishCultureInfo);
		}
	}
}