using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class AbsenceToday : IUserDataSetup
	{
		public DateOnly Date = DateOnly.Today;
		public IAbsence Absence = TestData.Absence;
		public IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var startTime = user.PermissionInformation.DefaultTimeZone().ConvertTimeToUtc(Date);
			var endTime = startTime.AddHours(24);
			var period = new DateTimePeriod(startTime, endTime);

			var absence = new PersonAbsence(user, Scenario, new AbsenceLayer(Absence, period));

			var absenceRepository = new PersonAbsenceRepository(uow);

			absenceRepository.Add(absence);

		}
	}
}