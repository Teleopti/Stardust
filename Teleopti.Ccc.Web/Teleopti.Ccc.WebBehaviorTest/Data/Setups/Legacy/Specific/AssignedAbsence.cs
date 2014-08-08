using System;
using System.Drawing;
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
	public class AssignedAbsence : IUserDataSetup
	{
		private static readonly CultureInfo SwedishCultureInfo = CultureInfo.GetCultureInfo(1053);
		public string Date { get; set; }
		public IAbsence Absence = TestData.Absence;
		public IScenario Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;
		public string AbsenceColor { get; set; }

		public AssignedAbsence()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(SwedishCultureInfo);
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var date = new DateOnly(DateTime.Parse(Date,SwedishCultureInfo));
			var startTime = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(date);
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