using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class AbsencePreference : BasePreference
	{
		
		public string Absence { get; set; }

		protected override PreferenceRestriction ApplyRestriction(IUnitOfWork uow)
		{
			IAbsence absence;
			if (Absence != null)
			{
				absence = new AbsenceRepository(uow).LoadAll().Single(a => a.Name == Absence);
			}
			else
			{
				absence = AbsenceFactory.CreateAbsence(DefaultName.Make(), DefaultName.Make(), Color.FromArgb(210, 150, 150));
				var absenceRepository = new AbsenceRepository(uow);
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