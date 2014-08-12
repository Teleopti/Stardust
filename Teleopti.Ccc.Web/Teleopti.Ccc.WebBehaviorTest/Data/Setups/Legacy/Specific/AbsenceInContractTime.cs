using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class AbsenceInContractTime : IUserDataSetup
	{
		private static readonly CultureInfo swedishCulture = CultureInfo.GetCultureInfo("sv-SE");
		public IAbsence Absence = TestData.AbsenceInContractTime;
		public IScenario Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;

		public AbsenceInContractTime()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(swedishCulture);
		}

		public string Date { get; set; }

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