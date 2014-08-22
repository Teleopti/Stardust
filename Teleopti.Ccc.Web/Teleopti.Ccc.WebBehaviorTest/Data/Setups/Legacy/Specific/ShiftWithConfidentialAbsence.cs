using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class ShiftWithConfidentialAbsence : IUserDataSetup
	{
		private static readonly CultureInfo SwedishCultureInfo = CultureInfo.GetCultureInfo(1053);
		public IScenario Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;
		public string Date { get; set; }
		public string Absence { get; set; }

		public ShiftWithConfidentialAbsence()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(SwedishCultureInfo);
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			new AssignedShift {Date = Date}.Apply(uow, user, cultureInfo);

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var date = TimeZoneHelper.ConvertToUtc(DateTime.Parse(Date,SwedishCultureInfo), timeZone);

			IAbsence confidentialAbsence;
			if (Absence == null)
			{
				confidentialAbsence = AbsenceFactory.CreateAbsence(DefaultName.Make());
				confidentialAbsence.Confidential = true;
				confidentialAbsence.DisplayColor = Color.GreenYellow;
			}
			else
			{
				confidentialAbsence = new AbsenceRepository(uow).LoadAll().Single(x => x.Name == Absence);
			}

			var absenseLayer = new AbsenceLayer(confidentialAbsence, new DateTimePeriod(date.AddHours(8), date.AddHours(17)));
			var personAbsence = new PersonAbsence(user, Scenario, absenseLayer);
			var absRepository = new PersonAbsenceRepository(uow);
			absRepository.Add(personAbsence);
		}
	}
}