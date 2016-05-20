using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.ReadModel.PerformanceTest
{
	[TestFixture]
	[PerformanceTest]
	[Toggle(Toggles.RTA_ScheduleProjectionReadOnlyHangfire_35703)]
	[Category("SaveSchedulesTest")]
	public class SaveSchedulesTest
	{
		public TestConfiguration Configuration;
		public Database Database;
		public IPersonRepository Persons;
		public IBusinessUnitRepository BusinessUnits;
		public WithUnitOfWork WithUnitOfWork;
		public Http Http;
		public MutableNow Now;
		public IHangfireUtilities Hangfire;
		public IDataSourceScope DataSource;
		public ImpersonateSystem Impersonate;
		public IScenarioRepository Scenarios;
		public IActivityRepository Activities;
		public IPersonAssignmentRepository Assignments;

		[Test]
		public void MeasurePerformance()
		{
			Guid businessUnitId;
			using (DataSource.OnThisThreadUse("TestData"))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.Value;
			Impersonate.Impersonate("TestData", businessUnitId);

			Now.Is("2016-06-01".Utc());
			Http.Get("/Test/SetCurrentTime?ticks=" + Now.UtcDateTime().Ticks);
			var dates = Enumerable.Range(1, Configuration.NumberOfDays)
				.Select(i => new DateOnly(Now.UtcDateTime().AddDays(i)));

			WithUnitOfWork.Do(() =>
			{
				var scenario = Scenarios.LoadDefaultScenario();
				var phone = Activities.LoadAll().Single(x => x.Name == "Phone");
				var persons = Persons.LoadAll();
				persons.ForEach(person =>
				{
					dates.ForEach(date =>
					{
						var assignment = new PersonAssignment(person, scenario, date);
						var startTime = DateTime.SpecifyKind(date.Date.AddHours(8), DateTimeKind.Utc);
						var endTime = DateTime.SpecifyKind(date.Date.AddHours(17), DateTimeKind.Utc);
						assignment.AddActivity(phone, startTime, endTime);
						Assignments.Add(assignment);
					});
				});
			});

			Hangfire.WaitForQueue();
		}
	}
	
}