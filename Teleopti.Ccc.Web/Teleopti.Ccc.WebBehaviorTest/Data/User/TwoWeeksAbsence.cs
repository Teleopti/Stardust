using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class TwoWeeksAbsence : IUserDataSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var date = DateTime.Now.Date.AddDays(-7);

			var startTime = user.PermissionInformation.DefaultTimeZone().ConvertTimeToUtc(date);
			var endTime = startTime.AddDays(14);
			var period = new DateTimePeriod(startTime, endTime);

			var absence = new PersonAbsence(user, TestData.Scenario, new AbsenceLayer(TestData.Absence, period));

			var absenceRepository = new PersonAbsenceRepository(uow);

			absenceRepository.Add(absence);

		}
	}
}