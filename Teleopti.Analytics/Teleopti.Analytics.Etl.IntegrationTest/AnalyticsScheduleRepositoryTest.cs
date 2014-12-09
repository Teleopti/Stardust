using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class AnalyticsScheduleRepositoryTest //: DatabaseTest
	{
		private IAnalyticsScheduleRepository _target;
		private const string datasourceName = "Teleopti CCC Agg: Default log object";
		[SetUp]
		public void Setup()
		{
			_target = StatisticRepositoryFactory.CreateAnalytics();
			SetupFixtureForAssembly.BeginTest();
			AnalyticsRunner.DropAndCreateTestProcedures();
		}

		[TearDown]
		public void TearDown()
		{
			SetupFixtureForAssembly.EndTest();
		}

		[Test]
		public void ShouldLoadActivities()
		{
			var timeZones = new UtcAndCetTimeZones();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, datasource) { BusinessUnitId = 12 };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			
			Data.Apply(activityPhone);

			var act = new Activity(activityPhone.Activity, datasource, businessUnit.BusinessUnitId) { ActivityId = 22 };

			analyticsDataFactory.Setup(act);
			analyticsDataFactory.Persist();

			var acts = _target.Activities();
			acts.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadAbsences()
		{
			var timeZones = new UtcAndCetTimeZones();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, datasource) { BusinessUnitId = 12 };
			var absenceFree = new AbsenceConfigurable { Name = "Freee", Color = "LightGreen" };

			Data.Apply(absenceFree);

			var abs = new Absence(absenceFree.Absence, datasource, businessUnit.BusinessUnitId) { AbsenceId = 22 };

			analyticsDataFactory.Setup(abs);
			analyticsDataFactory.Persist();

			var acts = _target.Absences();
			acts.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadScenariosAndCategories()
		{
			var timeZones = new UtcAndCetTimeZones();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var datasource = new ExistingDatasources(timeZones);
			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var scen = new ScenarioConfigurable {BusinessUnit = "BusinessUnit", EnableReporting = true,Name = "Default"};
			Data.Apply(cat);
			Data.Apply(scen);

			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(12,TestState.BusinessUnit.Id.GetValueOrDefault()));
			analyticsDataFactory.Setup(new ShiftCategory(cat.ShiftCategory, datasource,12));
			analyticsDataFactory.Persist();

			var scens = _target.Scenarios();
			scens.Count.Should().Be.EqualTo(1); 

			var cats = _target.ShiftCategories();
			cats.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadDates()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var weekDates = new CurrentWeekDates();
			analyticsDataFactory.Setup(weekDates);
			analyticsDataFactory.Persist();
			var dates = _target.LoadDimDates();
			dates.Count.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldLoadPerson()
		{
			var timeZones = new UtcAndCetTimeZones();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var datasource = new ExistingDatasources(timeZones);
			var personPeriod = Guid.NewGuid();
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, datasource) { BusinessUnitId = 12 };
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;

			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(new Person(person, datasource, 10, new DateTime(2010, 1, 1),
										new DateTime(2059, 12, 31), 0, -2, 20, TestState.BusinessUnit.Id.GetValueOrDefault(),
										false, timeZones.UtcTimeZoneId, personPeriod));
			analyticsDataFactory.Persist();
			var pers = _target.PersonAndBusinessUnit(personPeriod);
			pers.Should().Not.Be.Null();
			pers.PersonId.Should().Be.EqualTo(10);
			pers.BusinessUnitId.Should().Be.EqualTo(20);
		}

		[Test]
		public void ShouldInsertFactSchedule()
		{
			var timeZones = new UtcAndCetTimeZones();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var dates = new CurrentWeekDates();
			var intervals = new QuarterOfAnHourInterval();
			var datasource = new ExistingDatasources(timeZones);
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;
			analyticsDataFactory.Setup(new Person(person, datasource, 0, new DateTime(2010, 1, 1),
										new DateTime(2059, 12, 31), 0, -2, 0, TestState.BusinessUnit.Id.GetValueOrDefault(),
										false, timeZones.UtcTimeZoneId));
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, datasource) {BusinessUnitId = 12};
			var activityEmpty = new ActivityConfigurable { Name = "Empty" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var scenario = new ScenarioConfigurable() { BusinessUnit = TestState.BusinessUnit.Name, Name = "Deff" };
			Data.Apply(scenario);
			var martScenario = Scenario.DefaultScenarioFor(10, TestState.BusinessUnit.Id.GetValueOrDefault());
			analyticsDataFactory.Setup(martScenario);
			Data.Apply(activityEmpty);
			Data.Apply(activityPhone);

			var actEmpty = new Activity(activityEmpty.Activity, datasource,businessUnit.BusinessUnitId) {ActivityId = -1};
			var act = new Activity(activityPhone.Activity, datasource,businessUnit.BusinessUnitId){ActivityId = 22};

			var absenceEmpty = new AbsenceConfigurable { Name = "Empty" };
			var absenceFree = new AbsenceConfigurable { Name = "Free", Color = "LightGreen" };
			Data.Apply(absenceEmpty);
			Data.Apply(absenceFree);

			var absEmpty = new Absence(absenceEmpty.Absence, datasource, businessUnit.BusinessUnitId) {AbsenceId = -1};
			var abs = new Absence(absenceFree.Absence, datasource, businessUnit.BusinessUnitId);

			 
			analyticsDataFactory.Setup(act);
			analyticsDataFactory.Setup(actEmpty);
			analyticsDataFactory.Setup(abs);
			analyticsDataFactory.Setup(absEmpty);
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(businessUnit);

			analyticsDataFactory.Persist();
			var datePart = new AnalyticsFactScheduleDate
			{
				ScheduleStartDateLocalId = 1,
				ScheduleDateId = 1,
				IntervalId = 32,
				ActivityStartTime = DateTime.Today.AddHours(8),
				ActivityStartDateId = 1,
				ActivityEndDateId = 1,
				ActivityEndTime = DateTime.Today.AddHours(8).AddMinutes(15),
				ShiftStartIntervalId = 32,
				ShiftEndIntervalId = 68,
				DatasourceUpdateDate = DateTime.Now,
				ShiftStartTime = DateTime.Today.AddHours(8),
				ShiftEndTime = DateTime.Today.AddHours(17),
				ShiftStartDateId = 1,
				ShiftEndDateId = 1
			};
			var timePart = new AnalyticsFactScheduleTime
			{
				AbsenceId = -1,
				ActivityId = 22,
				ContractTimeAbsenceMinutes = 0,
				ContractTimeActivityMinutes = 15,
				ContractTimeMinutes = 15,
				OverTimeId = -1,
				OverTimeMinutes = 0,
				PaidTimeMinutes = 15,
				PaidTimeAbsenceMinutes = 0,
				PaidTimeActivityMinutes = 15,
				ReadyTimeMinues = 15,
				ScheduledMinutes = 15,
				ScheduledAbsenceMinutes = 0,
				ScheduledActivityMinutes = 15,
				ScenarioId = 10,
				ShiftLength = (int) TimeSpan.FromHours(8).TotalMinutes,
				WorkTimeMinutes = 15,
				WorkTimeAbsenceMinutes = 0,
				WorkTimeActivityMinutes = 15
			};

			var personPart = new AnalyticsFactSchedulePerson
			{
				BusinessUnitId = 12,
				PersonId = 0
			};


			_target.PersistFactScheduleRow(timePart,datePart,personPart);

		}

		[Test]
		public void ScholdBeAbleToDeleteADay()
		{
			_target.DeleteFactSchedule(1);
		}

	}
}