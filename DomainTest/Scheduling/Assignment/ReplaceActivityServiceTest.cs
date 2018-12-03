using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
	public class ReplaceActivityServiceTest
	{
		public ReplaceActivityService Target;

		[Test]
		public void ShouldReplaceActivity()
		{
			var dateOnly = new DateOnly(2018, 03, 26);
			var activity = new Activity().WithId();
			var  replaceWithActivity = new Activity().WithId();
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario();
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory().WithId()).WithLayer(activity, new TimePeriod(8, 16));
			var scheduleDictionary =  ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new []{ass}, new []{agent});
			var scheduleDays = scheduleDictionary[agent].ScheduledDayCollection(dateOnly.ToDateOnlyPeriod()).ToList();

			Target.Replace(scheduleDays, activity, replaceWithActivity, ass.Period.TimePeriod(TimeZoneInfo.Utc), TimeZoneInfo.Utc);

			var shiftLayers = scheduleDays.First().PersonAssignment().ShiftLayers.ToList();
			shiftLayers.Count.Should().Be.EqualTo(2);
			foreach (var layer in shiftLayers)
			{
				layer.Payload.Should().Be.EqualTo(layer.OrderIndex.Equals(0) ? activity : replaceWithActivity);
			}
		}

		[Test]
		public void ShouldReplaceActivtyMultipleDays()
		{
			var dateOnly = new DateOnly(2018, 03, 26);
			var period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			var activity = new Activity().WithId();
			var replaceWithActivity = new Activity().WithId();
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario();
			var ass1 = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory().WithId()).WithLayer(activity, new TimePeriod(8, 16));
			var ass2 = new PersonAssignment(agent, scenario, dateOnly.AddDays(1)).ShiftCategory(new ShiftCategory().WithId()).WithLayer(activity, new TimePeriod(8, 16));
			var scheduleDictionary = ScheduleDictionaryCreator.WithData(scenario, period, new[] { ass1, ass2 }, new[] { agent });
			var scheduleDays = scheduleDictionary[agent].ScheduledDayCollection(period).ToList();

			Target.Replace(scheduleDays, activity, replaceWithActivity, ass1.Period.TimePeriod(TimeZoneInfo.Utc), TimeZoneInfo.Utc);

			foreach (var scheduleDay in scheduleDays)
			{
				var shiftLayers = scheduleDay.PersonAssignment().ShiftLayers.ToList();
				shiftLayers.Count.Should().Be.EqualTo(2);
				foreach (var layer in shiftLayers)
				{
					layer.Payload.Should().Be.EqualTo(layer.OrderIndex.Equals(0) ? activity : replaceWithActivity);
				}
			}	
		}

		[Test]
		public void ShouldReplaceCorrectActivity()
		{
			var dateOnly = new DateOnly(2018, 03, 26);
			var activity1 = new Activity().WithId();
			var activity2 = new Activity().WithId();
			var replaceWithActivity = new Activity().WithId();
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario();
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory().WithId())
				.WithLayer(activity1, new TimePeriod(8, 16))
				.WithLayer(activity2, new TimePeriod(8, 16));
			var scheduleDictionary = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { ass }, new[] { agent });
			var scheduleDays = scheduleDictionary[agent].ScheduledDayCollection(dateOnly.ToDateOnlyPeriod()).ToList();

			Target.Replace(scheduleDays, activity1, replaceWithActivity, ass.Period.TimePeriod(TimeZoneInfo.Utc), TimeZoneInfo.Utc);
			var shiftLayers = scheduleDays.First().PersonAssignment().ShiftLayers.ToList();
			shiftLayers.Count.Should().Be.EqualTo(3);
			foreach (var layer in shiftLayers)
			{
				switch (layer.OrderIndex)
				{
					case 0:
						layer.Payload.Should().Be.EqualTo(activity1);
						break;
					case 1:
						layer.Payload.Should().Be.EqualTo(replaceWithActivity);
						break;
					default:
						layer.Payload.Should().Be.EqualTo(activity2);
						break;
				}
			}
		}

		[Test]
		public void ShouldNotReplaceActivityLackingPeriodInEnclosedPeriod()
		{
			var dateOnly = new DateOnly(2018, 03, 26);
			var activity = new Activity().WithId();
			var replaceWithActivity = new Activity().WithId();
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario();
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory().WithId()).WithLayer(activity, new TimePeriod(8, 16));
			var scheduleDictionary = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { ass }, new[] { agent });
			var scheduleDays = scheduleDictionary[agent].ScheduledDayCollection(dateOnly.ToDateOnlyPeriod()).ToList();

			Target.Replace(scheduleDays, activity, replaceWithActivity, ass.Period.MovePeriod(TimeSpan.FromHours(1)).TimePeriod(TimeZoneInfo.Utc), TimeZoneInfo.Utc);

			var shiftLayers = scheduleDays.First().PersonAssignment().ShiftLayers.ToList();
			shiftLayers.Count.Should().Be.EqualTo(1);
			shiftLayers.First().Payload.Should().Be.EqualTo(activity);
		}

		[Test]
		public void ShouldReplaceFromBottom()
		{
			var dateOnly = new DateOnly(2018, 03, 26);
			var activity1 = new Activity().WithId();
			var replaceWithActivity = new Activity().WithId();
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario();
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory().WithId())
				.WithLayer(activity1, new TimePeriod(8, 16))
				.WithLayer(activity1, new TimePeriod(12, 16));
			var scheduleDictionary = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { ass }, new[] { agent });
			var scheduleDays = scheduleDictionary[agent].ScheduledDayCollection(dateOnly.ToDateOnlyPeriod()).ToList();

			Target.Replace(scheduleDays, activity1, replaceWithActivity, ass.Period.ChangeStartTime(TimeSpan.FromHours(5)).TimePeriod(TimeZoneInfo.Utc), TimeZoneInfo.Utc);
			var shiftLayers = scheduleDays.First().PersonAssignment().ShiftLayers.ToList();
			shiftLayers.Count.Should().Be.EqualTo(3);
			foreach (var layer in shiftLayers)
			{
				switch (layer.OrderIndex)
				{
					case 0:
						layer.Payload.Should().Be.EqualTo(activity1);
						layer.Period.StartDateTime.Hour.Should().Be.EqualTo(8);
						break;
					case 1:
						layer.Payload.Should().Be.EqualTo(activity1);
						layer.Period.StartDateTime.Hour.Should().Be.EqualTo(12);
						break;
					default:
						layer.Payload.Should().Be.EqualTo(replaceWithActivity);
						break;
				}
			}
		}

		[Test]
		public void ShouldWorkDifferentTimeZones([Values("Taipei Standard Time", "UTC", "Mountain Standard Time")] string userTimeZone)
		{
			var dateOnly = new DateOnly(2018, 03, 26);
			var activity = new Activity().WithId();
			var replaceWithActivity = new Activity().WithId();
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario();
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory().WithId()).WithLayer(activity, new TimePeriod(8, 16));
			var scheduleDictionary = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { ass }, new[] { agent });
			var scheduleDays = scheduleDictionary[agent].ScheduledDayCollection(dateOnly.ToDateOnlyPeriod()).ToList();
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));

			Target.Replace(scheduleDays, activity, replaceWithActivity, ass.Period.TimePeriod(timeZone), timeZone);

			var shiftLayers = scheduleDays.First().PersonAssignment().ShiftLayers.ToList();
			shiftLayers.Count.Should().Be.EqualTo(2);
			foreach (var layer in shiftLayers)
			{
				layer.Payload.Should().Be.EqualTo(layer.OrderIndex.Equals(0) ? activity : replaceWithActivity);
			}
		}
	}
}
