using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

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
			var code = Guid.NewGuid();
			var act = new Activity(22, code, "Phone", Color.LightGreen, _datasource, businessUnitId);

			analyticsDataFactory.Setup(act);
			analyticsDataFactory.Persist();


			var acts = WithAnalyticsUnitOfWork.Get(() => TargetActivityRepository.Activity(code));
			acts.ActivityId.Should().Be.EqualTo(22);
		}
		
		[Test]
		public void ShouldInsertFactSchedule()
		{
			insertPerson(10, new DateTime(2010, 1, 1), AnalyticsDate.Eternity.DateDate);
			setupThingsForFactSchedule();
			analyticsDataFactory.Persist();
			insertFactSchedule(10, DateTime.Today.AddHours(8));
		}

		[Test]
		public void ShouldHandleNotSetDatasourceUpdateDate()
		{
			insertPerson(10, new DateTime(2010, 1, 1), AnalyticsDate.Eternity.DateDate);
			setupThingsForFactSchedule();
			analyticsDataFactory.Persist();
			var activityStartTime = DateTime.Today.AddHours(8);
			var shiftStartDateLocalId = 1;
			var personId = 10;
			var datePart = new AnalyticsFactScheduleDate
			{
				ScheduleStartDateLocalId = shiftStartDateLocalId,
				ScheduleDateId = shiftStartDateLocalId,
				IntervalId = 32,
				ActivityStartTime = activityStartTime,
				ActivityStartDateId = shiftStartDateLocalId,
				ActivityEndDateId = shiftStartDateLocalId,
				ActivityEndTime = activityStartTime.AddMinutes(15),
				ShiftStartIntervalId = 32,
				ShiftEndIntervalId = 68,
				ShiftStartTime = activityStartTime,
				ShiftEndTime = DateTime.Today.AddHours(17),
				ShiftStartDateId = shiftStartDateLocalId,
				ShiftEndDateId = shiftStartDateLocalId
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
				ReadyTimeMinutes = 15,
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

		[Test]
		public void ShouldThrowWhenDatesAreNotSqlDateTimeCompatible()
		{
			insertPerson(10, new DateTime(2010, 1, 1), AnalyticsDate.Eternity.DateDate);
			setupThingsForFactSchedule();
			analyticsDataFactory.Persist();
			var shiftStartDateLocalId = 1;
			var personId = 10;
			var datePart = new AnalyticsFactScheduleDate
			{
				ScheduleStartDateLocalId = shiftStartDateLocalId,
				ScheduleDateId = shiftStartDateLocalId,
				IntervalId = 32,
				ActivityStartDateId = shiftStartDateLocalId,
				ActivityEndDateId = shiftStartDateLocalId,
				ShiftStartIntervalId = 32,
				ShiftEndIntervalId = 68,
				ShiftStartDateId = shiftStartDateLocalId,
				ShiftEndDateId = shiftStartDateLocalId,
				DatasourceUpdateDate = DateTime.Now
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
				ReadyTimeMinutes = 15,
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
				Assert.Throws<ArgumentOutOfRangeException>(() => Target.PersistFactScheduleBatch(new List<IFactScheduleRow>
				{
					new FactScheduleRow
					{
						PersonPart = personPart,
						DatePart = datePart,
						TimePart = timePart
					}
				}));
			});
		}

		private void insertPerson(int personId, DateTime validFrom, DateTime validTo, bool toBeDeleted = false,
			int validateFromDateId = 0, int validateToDateId = -2)
		{
			analyticsDataFactory.Setup(new Person(personId, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", validFrom,
				validTo, validateFromDateId, validateToDateId, businessUnitId, Guid.NewGuid(), _datasource, toBeDeleted,
				_timeZones.UtcTimeZoneId));
		}

		private void insertFactSchedule(int personId, DateTime activityStartTime,int shiftStartDateLocalId=1)
		{
			var datePart = new AnalyticsFactScheduleDate
			{
				ScheduleStartDateLocalId = shiftStartDateLocalId,
				ScheduleDateId = shiftStartDateLocalId,
				IntervalId = 32,
				ActivityStartTime = activityStartTime,
				ActivityStartDateId = shiftStartDateLocalId,
				ActivityEndDateId = shiftStartDateLocalId,
				ActivityEndTime = activityStartTime.AddMinutes(15),
				ShiftStartIntervalId = 32,
				ShiftEndIntervalId = 68,
				DatasourceUpdateDate = DateTime.Now,
				ShiftStartTime = activityStartTime,
				ShiftEndTime = DateTime.Today.AddHours(17),
				ShiftStartDateId = shiftStartDateLocalId,
				ShiftEndDateId = shiftStartDateLocalId
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
				ReadyTimeMinutes = 15,
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
				BusinessUnitId = businessUnitId,
				DayOffId = -1
			};
			WithAnalyticsUnitOfWork.Do(() => { Target.PersistFactScheduleDayCountRow(dayCount); });
		}

		private void setupThingsForFactScheduleDayCount()
		{
			analyticsDataFactory.Setup(new CurrentWeekDates());
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(10, Guid.NewGuid()));
			analyticsDataFactory.Setup(new ShiftCategory(-1, Guid.NewGuid(), "Kattegat", Color.Green, _datasource, businessUnitId));
			analyticsDataFactory.Setup(new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId));
			analyticsDataFactory.Setup(new DimDayOff(-1, new Guid("00000000-0000-0000-0000-000000000000"), "Not Defined", _datasource, -1));
		}
		
		[Test]
		public void ShouldBeAbleToDeleteADays()
		{
			//not much of a test, but do it like it was before
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.DeleteFactSchedules(new[]{1}, Guid.NewGuid(), 1);
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