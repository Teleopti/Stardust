using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsFactScheduleMapperTest
	{
		public IAnalyticsFactScheduleMapper Target;
		public FakeIntervalLengthFetcher IntervalLength;
		public FakeAnalyticsDateRepository AnalyticsDates;
		public FakeAnalyticsAbsenceRepository AnalyticsAbsences;

		private readonly Guid absenceId = Guid.NewGuid();
		private readonly Guid personCode = Guid.NewGuid();
		private readonly Guid scenarioCode = Guid.NewGuid();


		private IScheduleDay getBaseScheduleDay(DateTime scheduleChangeTime)
		{
			return ScheduleDayFactory.Create(new DateOnly(scheduleChangeTime), PersonFactory.CreatePerson().WithId(personCode), ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioCode));
		}

		[Test]
		public void ShouldReturnEmptyListIfNoShift()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var scheduleChangeTime = DateTime.Now;
			var result = Target.AgentDaySchedule(new ProjectionChangedEventScheduleDay(), getBaseScheduleDay(scheduleChangeTime), null, scheduleChangeTime, 1, 1, scenarioCode, personCode);
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetIntervalLength()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var eventScheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = new DateTime(),
					EndDateTime = new DateTime()
				}
			};
			var scheduleChangeTime = DateTime.Now;
			Target.AgentDaySchedule(eventScheduleDay, getBaseScheduleDay(scheduleChangeTime), null, scheduleChangeTime, 1, 1, scenarioCode, personCode);
		}

		[Test]
		public void ShouldReturnCorrectSchemaWithBrokenInterval() // PBI 37562
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var shiftStart = new DateTime(2015, 1, 1, 8, 10, 0, DateTimeKind.Utc);
			var eventScheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = shiftStart,
					EndDateTime = shiftStart.AddHours(1)
				},
				Date = shiftStart
			};
			eventScheduleDay.Shift.Layers = createLayers(shiftStart, new[] { 60 });
			IntervalLength.Has(15);
			var scheduleChangeTime = DateTime.Now;

			var result = Target.AgentDaySchedule(eventScheduleDay, getBaseScheduleDay(scheduleChangeTime), null, scheduleChangeTime, 1, 1, scenarioCode, personCode);

			result.First().TimePart.ScheduledMinutes.Should().Be.EqualTo(5);
			result.Last().TimePart.ScheduledMinutes.Should().Be.EqualTo(10);
			result.Sum(a => a.TimePart.ScheduledMinutes).Should().Be.EqualTo(60);
			result.Count.Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldReturnNullWhenDateCouldNotBeHandled()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var shiftStart = new DateTime(2015, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var eventScheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = shiftStart,
					EndDateTime = shiftStart.AddHours(1)
				}
			};
			eventScheduleDay.Shift.Layers = createLayers(shiftStart, new[] { 60 });

			IntervalLength.Has(15);
			AnalyticsDates.Clear();
			var scheduleChangeTime = DateTime.Now;

			var result = Target.AgentDaySchedule(eventScheduleDay, getBaseScheduleDay(scheduleChangeTime), null, scheduleChangeTime, 1, 1, scenarioCode, personCode);
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGatherPartsForFactScheduleRow()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var shiftStart = new DateTime(2015, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var eventScheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = shiftStart,
					EndDateTime = shiftStart.AddHours(1)
				},
				Date = shiftStart
			};
			var personPart = new AnalyticsFactSchedulePerson();

			eventScheduleDay.Shift.Layers = createLayers(shiftStart, new[] { 60 });
			IntervalLength.Has(15);
			var scheduleChangeTime = DateTime.Now;

			var result = Target.AgentDaySchedule(eventScheduleDay, getBaseScheduleDay(scheduleChangeTime), personPart, scheduleChangeTime, 1, 1, scenarioCode, personCode);

			result.Count.Should().Be.EqualTo(4);
			result[0].PersonPart.Should().Be.SameInstanceAs(personPart);
		}

		[Test]
		public void ShouldGetTimeCorrectly()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var scheduleChangeTime = DateTime.Now;
			var baseScheduleDay = getBaseScheduleDay(scheduleChangeTime);
			var shiftStart = new DateTime(2015, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var eventScheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = shiftStart.Date,
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = shiftStart,
					EndDateTime = shiftStart.AddHours(1)
				}
			};
			var mainShiftSource = PersonAssignmentFactory.CreateEmptyAssignment(baseScheduleDay.Person, baseScheduleDay.Scenario, new DateTimePeriod(shiftStart, shiftStart.AddMinutes(45)));
			var contractTimeActivity = ActivityFactory.CreateActivity("ContractTime");
			contractTimeActivity.InContractTime = true;

			var workTimeActivity = ActivityFactory.CreateActivity("WorkTime");
			workTimeActivity.InWorkTime = true;

			var overTimeActivity = ActivityFactory.CreateActivity("OverTime");
			overTimeActivity.InWorkTime = true;

			var paidTimeActivity = ActivityFactory.CreateActivity("PaidTime");
			paidTimeActivity.InPaidTime = true;

			mainShiftSource.AddActivity(contractTimeActivity, new DateTimePeriod(shiftStart, shiftStart.AddMinutes(6)));
			mainShiftSource.AddActivity(workTimeActivity, new DateTimePeriod(shiftStart.AddMinutes(15), shiftStart.AddMinutes(22)));
			mainShiftSource.AddActivity(paidTimeActivity, new DateTimePeriod(shiftStart.AddMinutes(30), shiftStart.AddMinutes(38)));
			baseScheduleDay.AddMainShift(mainShiftSource);
			baseScheduleDay.CreateAndAddOvertime(overTimeActivity, new DateTimePeriod(shiftStart.AddMinutes(45), shiftStart.AddMinutes(54)), MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("Something", MultiplicatorType.Overtime));

			var personPart = new AnalyticsFactSchedulePerson();

			eventScheduleDay.Shift.Layers = createLayers(shiftStart, new[] { 60 });

			IntervalLength.Has(15);

			AnalyticsAbsences.AddAbsence(new AnalyticsAbsence
			{
				AbsenceCode = absenceId
			});
			
			var result = Target.AgentDaySchedule(eventScheduleDay, baseScheduleDay, personPart, scheduleChangeTime, 1, 1, scenarioCode, personCode);

			result.Count.Should().Be.EqualTo(4);
			result[0].PersonPart.Should().Be.SameInstanceAs(personPart);
			result[0].TimePart.ContractTimeMinutes.Should().Be.EqualTo(6);
			result[1].TimePart.WorkTimeMinutes.Should().Be.EqualTo(7);
			result[2].TimePart.PaidTimeMinutes.Should().Be.EqualTo(8);
			result[3].TimePart.OverTimeMinutes.Should().Be.EqualTo(9);
		}


		private IEnumerable<ProjectionChangedEventLayer> createLayers(DateTime startOfShift, IEnumerable<int> lengthCollection)
		{
			int accStart = 0;
			var layerList = new List<ProjectionChangedEventLayer>();
			

			foreach (int length in lengthCollection)
			{
				layerList.Add(
					new ProjectionChangedEventLayer
					{
						StartDateTime = startOfShift.AddMinutes(accStart),
						EndDateTime = startOfShift.AddMinutes(accStart).AddMinutes(length),
						ContractTime = new TimeSpan(0, 0, length),
						WorkTime = new TimeSpan(0, 0, length),
						DisplayColor = Color.Brown.ToArgb(),
						IsAbsence = true,
						IsAbsenceConfidential = true,
						Name = "Jonas",
						ShortName = "JN",
						PayloadId = absenceId
					}
					);
				accStart += length;
			}
			return layerList;
		}
	}
}
