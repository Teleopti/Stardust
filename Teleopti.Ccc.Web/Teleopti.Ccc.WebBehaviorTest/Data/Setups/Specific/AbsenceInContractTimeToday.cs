using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class AbsenceInContractTimeToday : IUserDataSetup
	{
		private static readonly CultureInfo swedishCulture = new CultureInfo("sv-SE");

		public AbsenceInContractTimeToday()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(swedishCulture);
		}

		public string Date { get; set; }
		public IAbsence Absence = TestData.AbsenceInContractTime;
		public IScenario Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var date = new DateOnly(DateTime.Parse(Date, swedishCulture));
			var startTime = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(date);
			var endTime = startTime.AddHours(24);
			var period = new DateTimePeriod(startTime, endTime);
			var absence = new PersonAbsence(user, Scenario, new AbsenceLayer(Absence, period));
			var absenceRepository = new PersonAbsenceRepository(uow);
			absenceRepository.Add(absence);
		}
	}
}