using System;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class AssignedAbsence : IUserDataSetup
	{
		private static readonly CultureInfo SwedishCultureInfo = CultureInfo.GetCultureInfo(1053);
		public string Date { get; set; }
		public IAbsence Absence;
		public IScenario Scenario = DefaultScenario.Scenario;
		public string AbsenceColor { get; set; }

		public AssignedAbsence()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(SwedishCultureInfo);
		}

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var date = DateTime.Parse(Date,SwedishCultureInfo);
			var startTime = person.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(date);
			var endTime = startTime.AddHours(24);
			var period = new DateTimePeriod(startTime, endTime);

			Absence = new Absence { Description = new Description("Absence") };
			AbsenceRepository.DONT_USE_CTOR(unitOfWork).Add(Absence);
			Absence.DisplayColor = AbsenceColor != null ? Color.FromName(AbsenceColor) : Color.FromArgb(210, 150, 150);

			var absence = new PersonAbsence(person, Scenario, new AbsenceLayer(Absence, period));

			var absenceRepository = new PersonAbsenceRepository(unitOfWork);

			absenceRepository.Add(absence);

		}
	}
}