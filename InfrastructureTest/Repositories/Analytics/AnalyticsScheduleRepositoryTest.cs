using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
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
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsScheduleRepositoryTest
	{
		public IAnalyticsScheduleRepository Target;
		public IAnalyticsActivityRepository TargetActivityRepository;
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


			var acts = WithAnalyticsUnitOfWork.Get(() => TargetActivityRepository.Activities());
			acts.Count.Should().Be.EqualTo(1);
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
		public void ShouldLoadPerson()
		{
			var personPeriodCode = Guid.NewGuid();
			analyticsDataFactory.Setup(new Person(10, Guid.NewGuid(), personPeriodCode, "Ashley", "Andeen", new DateTime(2010, 1, 1),
										AnalyticsDate.Eternity.DateDate, 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

			analyticsDataFactory.Persist();
			var pers = WithAnalyticsUnitOfWork.Get(() => Target.PersonAndBusinessUnit(personPeriodCode));
			pers.Should().Not.Be.Null();
			pers.PersonId.Should().Be.EqualTo(10);
			pers.BusinessUnitId.Should().Be.EqualTo(businessUnitId);
		}

		[Test]
		public void ShouldInsertFactSchedule()
		{
			insertPerson(10, new DateTime(2010, 1, 1), AnalyticsDate.Eternity.DateDate);
			setupThingsForFactSchedule();
			analyticsDataFactory.Persist();
			insertFactSchedule(10, DateTime.Today.AddHours(8));
		}

		private void insertPerson(int personId, DateTime validFrom, DateTime validTo, bool toBeDeleted=false)
		{
			analyticsDataFactory.Setup(new Person(personId, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", validFrom, validTo, 0, -2, businessUnitId, Guid.NewGuid(), _datasource, toBeDeleted, _timeZones.UtcTimeZoneId));
		}

		private void insertFactSchedule(int personId, DateTime activityStartTime)
		{
			var datePart = new AnalyticsFactScheduleDate
			{
				ScheduleStartDateLocalId = 1,
				ScheduleDateId = 1,
				IntervalId = 32,
				ActivityStartTime = activityStartTime,
				ActivityStartDateId = 1,
				ActivityEndDateId = 1,
				ActivityEndTime = activityStartTime.AddMinutes(15),
				ShiftStartIntervalId = 32,
				ShiftEndIntervalId = 68,
				DatasourceUpdateDate = DateTime.Now,
				ShiftStartTime = activityStartTime,
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
				PersonId = personId
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

		private void setupThingsForFactSchedule()
		{
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
		}

		[Test]
		public void ShouldInsertFactScheduleDayCount()
		{
			analyticsDataFactory.Setup(new Person(5, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", new DateTime(2010, 1, 1), AnalyticsDate.Eternity.DateDate, 0, AnalyticsDate.Eternity.DateId, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));
			setupThingsForFactScheduleDayCount();
			analyticsDataFactory.Persist();
			insertFactScheduleDayCount(5, 1, DateTime.Now);
		}

		private void insertFactScheduleDayCount(int personId,int shiftStartDateLocalId, DateTime startTime)
		{
			var dayCount = new AnalyticsFactScheduleDayCount
			{
				ShiftStartDateLocalId = shiftStartDateLocalId,
				PersonId = personId,
				ScenarioId = 10,
				StartTime = startTime,
				ShiftCategoryId = -1,
				DayOffName = null,
				DayOffShortName = null,
				AbsenceId = 22,
				BusinessUnitId = businessUnitId
			};
			WithAnalyticsUnitOfWork.Do(() => { Target.PersistFactScheduleDayCountRow(dayCount); });
		}

		private void setupThingsForFactScheduleDayCount()
		{
			analyticsDataFactory.Setup(new CurrentWeekDates());
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(10, Guid.NewGuid()));
			analyticsDataFactory.Setup(new ShiftCategory(-1, Guid.NewGuid(), "Kattegat", Color.Green, _datasource, businessUnitId));
			analyticsDataFactory.Setup(new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId));
			analyticsDataFactory.Setup(new DimDayOff(-1, Guid.NewGuid(), "DayOff", _datasource, businessUnitId));
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

		[Test]
		public void ShouldUpdateFactScheduleWithUnlinkedPersonidsWhenAddNewPersonPeriod()
		{
			insertPerson(10, new DateTime(2015, 1, 10), new DateTime(2015, 1, 24));
			insertPerson(11, new DateTime(2015, 1, 25), AnalyticsDate.Eternity.DateDate);
			setupThingsForFactSchedule();
			analyticsDataFactory.Persist();
			insertFactSchedule(10, new DateTime(2015, 1, 20).AddHours(8));
			insertFactSchedule(10, new DateTime(2015, 1, 26).AddHours(8));

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleRowCount(10));
			rowCount.Should().Be.EqualTo(2);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleRowCount(11));
			rowCount.Should().Be.EqualTo(0);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { 10, 11 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleRowCount(10));
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleRowCount(11));
			rowCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldUpdateFactScheduleWithUnlinkedPersonidsWhenDeleteExistingPersonPeriod()
		{
			insertPerson(10, new DateTime(2015, 1, 10), AnalyticsDate.Eternity.DateDate, true);
			insertPerson(11, new DateTime(2015, 1, 5), AnalyticsDate.Eternity.DateDate);
			setupThingsForFactSchedule();
			analyticsDataFactory.Persist();
			insertFactSchedule(10, new DateTime(2015, 1, 20).AddHours(8));
			insertFactSchedule(11, new DateTime(2015, 1, 26).AddHours(8));

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleRowCount(10));
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleRowCount(11));
			rowCount.Should().Be.EqualTo(1);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { 10, 11 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleRowCount(10));
			rowCount.Should().Be.EqualTo(0);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleRowCount(11));
			rowCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldUpdateFactScheduleDayCountWithUnlinkedPersonidsWhenAddNewPersonPeriod()
		{
			insertPerson(10, new DateTime(2015, 1, 10), new DateTime(2015, 1, 24));
			insertPerson(11, new DateTime(2015, 1, 25), AnalyticsDate.Eternity.DateDate);
			setupThingsForFactScheduleDayCount();
			analyticsDataFactory.Persist();
			insertFactScheduleDayCount(10, 1, new DateTime(2015, 1, 20).AddHours(8));
			insertFactScheduleDayCount(10, 2, new DateTime(2015, 1, 26).AddHours(8));

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleDayCountRowCount(10));
			rowCount.Should().Be.EqualTo(2);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleDayCountRowCount(11));
			rowCount.Should().Be.EqualTo(0);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { 10, 11 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleDayCountRowCount(10));
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleDayCountRowCount(11));
			rowCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldUpdateFactScheduleDayCountWithUnlinkedPersonidsWhenDeleteExistingPersonPeriod()
		{
			insertPerson(10, new DateTime(2015, 1, 10), AnalyticsDate.Eternity.DateDate, true);
			insertPerson(11, new DateTime(2015, 1, 5), AnalyticsDate.Eternity.DateDate);
			setupThingsForFactScheduleDayCount();
			analyticsDataFactory.Persist();
			insertFactScheduleDayCount(10, 1, new DateTime(2015, 1, 20).AddHours(8));
			insertFactScheduleDayCount(11, 2, new DateTime(2015, 1, 26).AddHours(8));

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleDayCountRowCount(10));
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleDayCountRowCount(11));
			rowCount.Should().Be.EqualTo(1);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { 10, 11 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleDayCountRowCount(10));
			rowCount.Should().Be.EqualTo(0);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactScheduleDayCountRowCount(11));
			rowCount.Should().Be.EqualTo(2);
		}
	}
}