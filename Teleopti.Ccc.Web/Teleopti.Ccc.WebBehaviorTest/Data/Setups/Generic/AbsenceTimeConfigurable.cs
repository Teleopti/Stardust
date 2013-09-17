using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class AbsenceTimeConfigurable : IPostSetup
	{
		public DateTime Date { get; set; }
		public int Hours { get; set; }
		public string Absence { get; set; }

		public void Apply(IPerson user, IUnitOfWork uow)
		{
			var scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;
			var scenarioId = scenario.Id.GetValueOrDefault();

			var absenceRepository = new AbsenceRepository(uow);
			var absence = absenceRepository.LoadAll().Single(a=>a.Name==Absence);

			var scheduleProjectionReadOnlyRepository = new ScheduleProjectionReadOnlyRepository(new FixedCurrentUnitOfWork(uow));

			var period =
				new DateOnlyPeriod(new DateOnly(Date), new DateOnly(Date)).ToDateTimePeriod(user.PermissionInformation.DefaultTimeZone());

			var layer = new ProjectionChangedEventLayer
			{
				ContractTime = TimeSpan.FromHours(Hours),
				WorkTime = TimeSpan.FromHours(Hours),
				DisplayColor = Color.Bisque.ToArgb(),
				Name = absence.Name,
				ShortName = "xx",
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime,
				PayloadId = absence.Id.GetValueOrDefault()
			};

			scheduleProjectionReadOnlyRepository.AddProjectedLayer(new DateOnly(Date), scenarioId, user.Id.GetValueOrDefault(), layer);

		}
	}
}