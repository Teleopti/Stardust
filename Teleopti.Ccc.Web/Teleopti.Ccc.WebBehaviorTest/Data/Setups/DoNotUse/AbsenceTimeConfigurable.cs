using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class AbsenceTimeConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public int Hours { get; set; }
		public string Absence { get; set; }
		
		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var scenario = DefaultScenario.Scenario;
			var scenarioId = scenario.Id.GetValueOrDefault();

			var absenceRepository = new AbsenceRepository(currentUnitOfWork);
			var absence = absenceRepository.LoadAll().Single(a=>a.Name==Absence);

			var scheduleProjectionReadOnlyRepository = new ScheduleProjectionReadOnlyPersister(currentUnitOfWork);

			var period =
				new DateOnlyPeriod(new DateOnly(Date), new DateOnly(Date)).ToDateTimePeriod(user.PermissionInformation.DefaultTimeZone());
			
			var model = new ScheduleProjectionReadOnlyModel
			{
				PersonId = user.Id.GetValueOrDefault(),
				ScenarioId = scenarioId,
				BelongsToDate = new DateOnly(Date),
				PayloadId = absence.Id.GetValueOrDefault(),
				WorkTime = TimeSpan.FromHours(Hours),
				ContractTime = TimeSpan.FromHours(Hours),
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime,
				Name = absence.Name,
				ShortName = "xx",
				DisplayColor = Color.Bisque.ToArgb()
			};
			scheduleProjectionReadOnlyRepository.AddActivity(model);
		}
	}
}