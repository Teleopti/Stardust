using System;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class AbsenceInContractTime : IUserDataSetup
	{
		private static readonly CultureInfo swedishCulture = CultureInfo.GetCultureInfo("sv-SE");
		public IScenario Scenario = GlobalDataMaker.Data().Data<DefaultScenario>().Scenario;

		public AbsenceInContractTime()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(swedishCulture);
		}

		public string Date { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var absenceInContractTime = AbsenceFactory.CreateAbsence(RandomName.Make(), RandomName.Make(), Color.FromArgb(200, 150, 150));
			absenceInContractTime.InContractTime = true;
			new AbsenceRepository(uow).Add(absenceInContractTime);

			var date = new DateOnly(DateTime.Parse(Date, swedishCulture));
			var startTime = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(date);
			var endTime = startTime.AddHours(24);
			var period = new DateTimePeriod(startTime, endTime);
			var absence = new PersonAbsence(user, Scenario, new AbsenceLayer(absenceInContractTime, period));
			var absenceRepository = new PersonAbsenceRepository(uow);
			absenceRepository.Add(absence);
		}
	}
}