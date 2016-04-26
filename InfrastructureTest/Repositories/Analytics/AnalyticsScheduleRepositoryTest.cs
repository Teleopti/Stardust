using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure.Analytics;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;
using Scenario = Teleopti.Ccc.TestCommon.TestData.Analytics.Scenario;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsScheduleRepositoryTest
	{
		public IAnalyticsScheduleRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private AnalyticsDataFactory analyticsDataFactory;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
		}

		[Test]
		public void ShouldLoadActivities()
		{
			var act = new Activity(22, Guid.NewGuid(), "Phone", Color.LightGreen, _datasource, businessUnitId);

			analyticsDataFactory.Setup(act);
			analyticsDataFactory.Persist();

			
			var acts = WithAnalyticsUnitOfWork.Get(() => Target.Activities());
			acts.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadOvertimes()
		{
			var overtime = new DimOvertime(22, Guid.NewGuid(), "Overtime1", _datasource, businessUnitId);

			analyticsDataFactory.Setup(overtime);
			analyticsDataFactory.Persist();

			var overtimes = WithAnalyticsUnitOfWork.Get(() => Target.Overtimes());
			overtimes.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadAbsences()
		{
			var abs = new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId);

			analyticsDataFactory.Setup(abs);
			analyticsDataFactory.Persist();

			var absences = WithAnalyticsUnitOfWork.Get(() => Target.Absences());
			absences.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadShiftLengths()
		{
			var sl = new ShiftLength(33, 240, _datasource);

			analyticsDataFactory.Setup(sl);
			analyticsDataFactory.Persist();

			var shiftLengths = WithAnalyticsUnitOfWork.Get(() => Target.ShiftLengths());
			shiftLengths.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadScenariosAndCategories()
		{
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(1, Guid.NewGuid()));
			analyticsDataFactory.Setup(new ShiftCategory(1, Guid.NewGuid(), "Kattegat", Color.Green, _datasource, businessUnitId));
			analyticsDataFactory.Persist();

			var scens = WithAnalyticsUnitOfWork.Get(() => Target.Scenarios());
			scens.Count.Should().Be.EqualTo(1);

			var cats = WithAnalyticsUnitOfWork.Get(() => Target.ShiftCategories());
			cats.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldLoadPerson()
		{
			var personPeriodCode = Guid.NewGuid();
			analyticsDataFactory.Setup(new Person(10, Guid.NewGuid(), personPeriodCode, "Ashley", "Andeen", new DateTime(2010, 1, 1),
										new DateTime(2059, 12, 31), 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

			analyticsDataFactory.Persist();
			var pers = WithAnalyticsUnitOfWork.Get(() => Target.PersonAndBusinessUnit(personPeriodCode));
			pers.Should().Not.Be.Null();
			pers.PersonId.Should().Be.EqualTo(10);
			pers.BusinessUnitId.Should().Be.EqualTo(businessUnitId);
		}

		[Test]
		public void ShouldInsertFactSchedule()
		{
			analyticsDataFactory.Setup(new Person(10, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", new DateTime(2010, 1, 1),
										new DateTime(2059, 12, 31), 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(10, Guid.NewGuid()));

			var actEmpty = new Activity(-1, Guid.NewGuid(), "Empty", Color.Black, _datasource, businessUnitId);
			var act = new Activity(22, Guid.NewGuid(), "Phone", Color.LightGreen, _datasource, businessUnitId);
			
			var absEmpty = new Absence(-1, Guid.NewGuid(), "Empty", Color.Black, _datasource, businessUnitId);
			var abs = new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId);

			var shiftLength = new ShiftLength(4, 480, _datasource);
			 
			analyticsDataFactory.Setup(act);
			analyticsDataFactory.Setup(actEmpty);
			analyticsDataFactory.Setup(abs);
			analyticsDataFactory.Setup(absEmpty);
			analyticsDataFactory.Setup(new CurrentWeekDates());
			analyticsDataFactory.Setup(new QuarterOfAnHourInterval());
			analyticsDataFactory.Setup(shiftLength);

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
				ShiftLengthId = 4,
				WorkTimeMinutes = 15,
				WorkTimeAbsenceMinutes = 0,
				WorkTimeActivityMinutes = 15
			};

			var personPart = new AnalyticsFactSchedulePerson
			{
				BusinessUnitId = businessUnitId,
				PersonId = 10
			};
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.PersistFactScheduleBatch(new List<IFactScheduleRow>
				{
					new FactScheduleRow
					{
						PersonPart = personPart,
						DatePart = datePart,
						TimePart = timePart
					}
				});
			});
		}

		[Test]
		public void ShouldInsertFactScheduleDayCount()
		{
			analyticsDataFactory.Setup(new CurrentWeekDates());
			analyticsDataFactory.Setup(new Person(5, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", new DateTime(2010, 1, 1),
										new DateTime(2059, 12, 31), 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(10, Guid.NewGuid()));
			analyticsDataFactory.Setup(new ShiftCategory(-1, Guid.NewGuid(), "Kattegat", Color.Green, _datasource, businessUnitId));
			analyticsDataFactory.Setup(new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId));
			analyticsDataFactory.Setup(new DimDayOff(-1, Guid.NewGuid(), "DayOff", _datasource, businessUnitId));
			analyticsDataFactory.Persist();

			var dayCount = new AnalyticsFactScheduleDayCount
			{
				ShiftStartDateLocalId = 1,
				PersonId = 5,
				ScenarioId = 10,
				StartTime = DateTime.Now,
				ShiftCategoryId = -1,
				DayOffName = null,
				DayOffShortName = null,
				AbsenceId = 22,
				BusinessUnitId = businessUnitId
			};
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.PersistFactScheduleDayCountRow(dayCount);
			});
		}

		[Test]
		public void ShouldBeAbleToDeleteADay()
		{
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.DeleteFactSchedule(1, 1, 1);
			});
		}

		[Test]
		public void ShouldCreateNewShiftLengthIfNotExist()
		{
			var shiftLengthId = WithAnalyticsUnitOfWork.Get(() => Target.ShiftLengthId(120));
			shiftLengthId.Should().Be.GreaterThan(0);
		}

	}
}