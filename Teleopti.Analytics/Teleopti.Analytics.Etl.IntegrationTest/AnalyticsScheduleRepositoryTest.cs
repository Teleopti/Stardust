using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class AnalyticsScheduleRepositoryTest //: DatabaseTest
	{
		private IAnalyticsScheduleRepository _target;
		private AnalyticsDataFactory analyticsDataFactory;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;

		[SetUp]
		public void Setup()
		{
			
			_target = StatisticRepositoryFactory.CreateAnalytics();
			SetupFixtureForAssembly.BeginTest();
			AnalyticsRunner.DropAndCreateTestProcedures();

			analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
		}

		[TearDown]
		public void TearDown()
		{
			SetupFixtureForAssembly.EndTest();
		}

		[Test]
		public void ShouldLoadActivities()
		{
			var act = new Activity(22, Guid.NewGuid(), "Phone", Color.LightGreen, _datasource, businessUnitId);

			analyticsDataFactory.Setup(act);
			analyticsDataFactory.Persist();

			var acts = _target.Activities();
			acts.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadAbsences()
		{
			var abs = new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId);

			analyticsDataFactory.Setup(abs);
			analyticsDataFactory.Persist();

			var acts = _target.Absences();
			acts.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadScenariosAndCategories()
		{
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(1, Guid.NewGuid()));
			analyticsDataFactory.Setup(new ShiftCategory(1, Guid.NewGuid(), "Kattegat", Color.Green, _datasource, businessUnitId));
			analyticsDataFactory.Persist();

			var scens = _target.Scenarios();
			scens.Count.Should().Be.EqualTo(1); 

			var cats = _target.ShiftCategories();
			cats.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadDates()
		{
			var weekDates = new CurrentWeekDates();
			analyticsDataFactory.Setup(weekDates);
			analyticsDataFactory.Persist();
			var dates = _target.Dates();
			dates.Count.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldLoadPerson()
		{
			var personPeriodCode = Guid.NewGuid();
			analyticsDataFactory.Setup(new Person(10, Guid.NewGuid(), personPeriodCode, "Ashley", "Andeen", new DateTime(2010, 1, 1),
										new DateTime(2059, 12, 31), 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

			analyticsDataFactory.Persist();
			var pers = _target.PersonAndBusinessUnit(personPeriodCode);
			pers.Should().Not.Be.Null();
			pers.PersonId.Should().Be.EqualTo(10);
			pers.BusinessUnitId.Should().Be.EqualTo(businessUnitId);
		}

		[Test]
		public void ShouldInsertFactSchedule()
		{
			var dates = new CurrentWeekDates();
			var intervals = new QuarterOfAnHourInterval();

			analyticsDataFactory.Setup(new Person(10, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", new DateTime(2010, 1, 1),
										new DateTime(2059, 12, 31), 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(10, Guid.NewGuid()));

			var actEmpty = new Activity(-1, Guid.NewGuid(), "Empty", Color.Black, _datasource, businessUnitId);
			var act = new Activity(22, Guid.NewGuid(), "Phone", Color.LightGreen, _datasource, businessUnitId);
			
			var absEmpty = new Absence(-1, Guid.NewGuid(), "Empty", Color.Black, _datasource, businessUnitId);
			var abs = new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId);
			 
			analyticsDataFactory.Setup(act);
			analyticsDataFactory.Setup(actEmpty);
			analyticsDataFactory.Setup(abs);
			analyticsDataFactory.Setup(absEmpty);
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(intervals);

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
				BusinessUnitId = businessUnitId,
				PersonId = 10
			};


			_target.PersistFactScheduleRow(timePart,datePart,personPart);

		}

		[Test]
		public void ShouldInsertFactScheduleDayCount()
		{
			analyticsDataFactory.Setup(new CurrentWeekDates());
			analyticsDataFactory.Setup(new Person(5, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", new DateTime(2010, 1, 1),
										new DateTime(2059, 12, 31), 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(10, Guid.NewGuid()));

			

			var dayCount = new AnalyticsFactScheduleDayCount
			{
				ShiftStartDateLocalId = 1,
				PersonId = 5,
				ScenarioId = 10,
				StartTime = DateTime.Now,
				ShiftCategoryId = -1,
				DayOffName = 
			};
			_target.PersistFactScheduleDayCountRow(dayCount);
		}

		[Test]
		public void ScholdBeAbleToDeleteADay()
		{
			_target.DeleteFactSchedule(1, 1, 1);
		}

	}
}