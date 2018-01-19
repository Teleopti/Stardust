using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsPersonTerminalDateChangedEventHandlerTest : ISetup
	{
		public FakeAnalyticsScheduleRepository AnalyticsSchedules;
		public FakeAnalyticsPersonPeriodRepository PersonPeriods;
		public AnalyticsPersonTerminalDateChangedEventHandler Target;

		public IBusinessUnitRepository BusinessUnits;

		private const int intervalLengthInMinute = 15;
		private const int intervalCountPerDay = 24 * 60 / intervalLengthInMinute;
		private readonly Guid personId = Guid.NewGuid();
		private readonly Guid businessUnitId = Guid.NewGuid();

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeScheduleStorage_DoNotUse>().For<IScheduleStorage>();
			system.AddService<AnalyticsPersonTerminalDateChangedEventHandler>();
		}

		[Test]
		public void ShouldDoNothingIfTerminalDateIsNull()
		{
			// All fact schedules from 2018-01-01 to 2018-01-10 will be kept.
			handleEventAndCheckResult(null, 10 * intervalCountPerDay);
		}

		[Test]
		public void ShouldDeleteFactSchedulesAfterTerminalDate()
		{
			// Fact schedules after 2018-01-04 will be deleted.
			handleEventAndCheckResult(new DateTime(2018, 01, 04), 3 * intervalCountPerDay);
		}

		private void setupData()
		{
			const int scenarioDimId = 3;
			const int personDimId = 1;
			const int businessUnitDimId = 2;
			var scheduleStartDate = new DateTime(2018, 01, 01);

			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			BusinessUnits.Add(businessUnit);

			PersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				BusinessUnitCode = businessUnitId,
				BusinessUnitId = businessUnitDimId,
				PersonCode = personId,
				PersonId = personDimId,
				ValidFromDate = scheduleStartDate,
				ValidToDate = scheduleStartDate.AddYears(1)
			});

			for (var dateId = 0; dateId < 10; dateId++)
			{
				for (var intervalId = 0; intervalId < intervalCountPerDay; intervalId++)
				{
					var existingSchedule = new FactScheduleRow
					{
						DatePart = new AnalyticsFactScheduleDate
						{
							ScheduleDateId = dateId + 1,
							ActivityStartTime = scheduleStartDate.AddDays(dateId).AddMinutes(intervalId * intervalLengthInMinute)
						},
						PersonPart = new AnalyticsFactSchedulePerson
						{
							BusinessUnitId = businessUnitDimId,
							PersonId = personDimId
						},
						TimePart = new AnalyticsFactScheduleTime
						{
							ScenarioId = scenarioDimId
						}
					};
					AnalyticsSchedules.FactScheduleRows.Add(existingSchedule);
				}
			}
		}

		private void handleEventAndCheckResult(DateTime? terminalDate, int expectedFactScheduleRowCount)
		{
			setupData();
			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId,
				TerminationDate = terminalDate
			});

			AnalyticsSchedules.FactScheduleRows.Count.Should().Be.EqualTo(expectedFactScheduleRowCount);
		}
	}
}