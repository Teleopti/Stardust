using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[DomainTest]
	public class AnalyticsScheduleChangeUpdaterUnnecessaryLoadTest : IIsolateSystem
	{
		public AnalyticsScheduleChangeUpdater Target;
		public FakeAnalyticsDateRepository AnalyticsDates;
		public IScheduleStorage Schedules;
		public IBusinessUnitRepository BusinessUnits;
		public IScenarioRepository Scenarios;
		public IPersonRepository Persons;
		public FakeAnalyticsScenarioRepository AnalyticsScenarios;
		public FakeAnalyticsPersonPeriodRepository AnalyticsPersonPeriods;
		
		[Test]
		public void ShouldNotLoadUnnecessaryData()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2017, 12, 31), new DateTime(2019, 12, 31));
			var date = new DateOnly(2018,10,22);
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId();
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId();
			var person = PersonFactory.CreatePerson().WithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(date.AddDays(-10)).WithId();
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, scenario,date.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()));
			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenario.Id.Value, ScenarioId = 6
			};
			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);
			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 1,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriod.Id.Value
			});

			Assert.DoesNotThrow(() => Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = date.Date.ToUniversalTime().AddDays(-1),
				EndDateTime = date.Date.ToUniversalTime().AddDays(1),
				ScenarioId = scenario.Id.Value,
				LogOnBusinessUnitId = businessUnit.Id.Value,
				PersonId = person.Id.Value
			}));
		}

		public void Isolate(IIsolate isolate)
		{
			SetupThrowingTestDoubles.For(isolate, typeof(INoteRepository));
			SetupThrowingTestDoubles.For(isolate, typeof(IPublicNoteRepository));
		}
	}
}