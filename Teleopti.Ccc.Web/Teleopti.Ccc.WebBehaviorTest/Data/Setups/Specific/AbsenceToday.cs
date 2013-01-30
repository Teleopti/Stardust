using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class AbsenceToday : IUserDataSetup
	{
		public DateOnly Date = DateOnly.Today;
		public IAbsence Absence = TestData.Absence;
		public IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;
		public string AbsenceColor { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var startTime = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(Date);
			var endTime = startTime.AddHours(24);
			var period = new DateTimePeriod(startTime, endTime);

			PersonAbsence absence;

			if (AbsenceColor != null)
			{
				var newAbsence = new Absence();
				newAbsence.Description = new Description("Absence");
				new AbsenceRepository(uow).Add(newAbsence);
				newAbsence.DisplayColor = Color.FromName(AbsenceColor);
				absence = new PersonAbsence(user, Scenario, new AbsenceLayer(newAbsence, period));
			}
			else
			{
				absence = new PersonAbsence(user, Scenario, new AbsenceLayer(TestData.Absence, period));
			}

			var absenceRepository = new PersonAbsenceRepository(uow);

			absenceRepository.Add(absence);

		}
	}
}