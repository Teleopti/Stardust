using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class TwoWeeksAbsence : IUserDataSetup
	{
		public IScenario Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var date = DateOnlyForBehaviorTests.TestToday.Date.AddDays(-7);

			var startTime = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(date);
			var endTime = startTime.AddDays(14);
			var period = new DateTimePeriod(startTime, endTime);

			var absence = new PersonAbsence(user, Scenario, new AbsenceLayer(TestData.Absence, period));

			var absenceRepository = new PersonAbsenceRepository(uow);

			absenceRepository.Add(absence);

		}
	}
}