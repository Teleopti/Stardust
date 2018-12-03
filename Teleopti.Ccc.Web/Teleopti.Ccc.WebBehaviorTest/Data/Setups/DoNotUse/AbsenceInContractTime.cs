using System;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class AbsenceInContractTime : IUserDataSetup
	{
		private static readonly CultureInfo swedishCulture = CultureInfo.GetCultureInfo("sv-SE");
		public IScenario Scenario = DefaultScenario.Scenario;

		public AbsenceInContractTime()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(swedishCulture);
		}

		public string Date { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var absenceInContractTime = AbsenceFactory.CreateAbsence(RandomName.Make(), RandomName.Make(), Color.FromArgb(200, 150, 150));
			absenceInContractTime.InContractTime = true;
			new AbsenceRepository(unitOfWork).Add(absenceInContractTime);

			var date = DateTime.Parse(Date, swedishCulture);
			var startTime = person.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(date);
			var endTime = startTime.AddHours(24);
			var period = new DateTimePeriod(startTime, endTime);
			var absence = new PersonAbsence(person, Scenario, new AbsenceLayer(absenceInContractTime, period));
			var absenceRepository = new PersonAbsenceRepository(unitOfWork);
			absenceRepository.Add(absence);
		}
	}
}