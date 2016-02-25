using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ShiftWithConfidentialAbsence : IUserDataSetup
	{
		private static readonly CultureInfo SwedishCultureInfo = CultureInfo.GetCultureInfo(1053);
		public IScenario Scenario = DefaultScenario.Scenario;
		public string Date { get; set; }
		public string Absence { get; set; }

		public ShiftWithConfidentialAbsence()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(SwedishCultureInfo);
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			new AssignedShift {Date = Date}.Apply(currentUnitOfWork, user, cultureInfo);

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var date = TimeZoneHelper.ConvertToUtc(DateTime.Parse(Date,SwedishCultureInfo), timeZone);

			IAbsence confidentialAbsence;
			if (Absence == null)
			{
				confidentialAbsence = AbsenceFactory.CreateAbsence(RandomName.Make());
				confidentialAbsence.Confidential = true;
				confidentialAbsence.DisplayColor = Color.GreenYellow;
				new AbsenceRepository(currentUnitOfWork).Add(confidentialAbsence);
			}
			else
			{
				confidentialAbsence = new AbsenceRepository(currentUnitOfWork).LoadAll().Single(x => x.Name == Absence);
			}

			var absenseLayer = new AbsenceLayer(confidentialAbsence, new DateTimePeriod(date.AddHours(8), date.AddHours(17)));
			var personAbsence = new PersonAbsence(user, Scenario, absenseLayer);
			var absRepository = new PersonAbsenceRepository(currentUnitOfWork);
			absRepository.Add(personAbsence);
		}
	}
}