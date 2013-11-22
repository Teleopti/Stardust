using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class ReplaceLayerInScheduleTest
	{
		[Test]
		public void ShouldThrowIfActivityLayerDoesntExist()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain().AddMainShiftLayer().CreatePart();
			Assert.Throws<ArgumentException>(() =>
						 target.Replace(scheduleDay, new MainShiftLayer(new Activity("d"), new DateTimePeriod()), new Activity("d"), new DateTimePeriod()));
		}

		[Test]
		public void ShouldThrowIfAbsenceLayerDoesntExist()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain().AddAbsence().CreatePart();
			Assert.Throws<ArgumentException>(() =>
																			 target.Replace(scheduleDay, new AbsenceLayer(new Absence(), new DateTimePeriod()), new Absence(), new DateTimePeriod()));
		}

		[Test]
		public void ShouldThrowIfScheduleDayHasNoAssignmentAndLayerIsReplaced()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePart();
			Assert.Throws<ArgumentException>(() =>
						 target.Replace(scheduleDay, new MainShiftLayer(new Activity("d"), new DateTimePeriod()), new Activity("d"), new DateTimePeriod()));
		}

		[Test]
		public void ShouldReplaceAbsenceLayer()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain().AddAbsence().CreatePart();
			var orgLayer = scheduleDay.PersonAbsenceCollection().Single().Layer;
			var newPayload = new Absence();
			var newPeriod = new DateTimePeriod(orgLayer.Period.StartDateTime, orgLayer.Period.EndDateTime.AddMinutes(-15));

			target.Replace(scheduleDay, orgLayer, newPayload, newPeriod);

			var newLayerCollection = scheduleDay.PersonAbsenceCollection().Single().Layer;
			var newLayer = newLayerCollection;
			newLayer.Payload.Should().Be.SameInstanceAs(newPayload);
			newLayer.Period.Should().Be.EqualTo(newPeriod);
		}

		[Test]
		public void ShouldReplaceMainShiftLayer()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain().AddMainShiftLayer().CreatePart();
			var newPayload = new Activity("d");
			var newPeriod = new DateTimePeriod();
			var orgLayerCollection = scheduleDay.PersonAssignment().MainActivities();

			target.Replace(scheduleDay, orgLayerCollection.First(), newPayload, newPeriod);

			var newLayerCollection = scheduleDay.PersonAssignment().MainActivities();
			orgLayerCollection.Count().Should().Be.EqualTo(newLayerCollection.Count());
			var newLayer = newLayerCollection.First();
			newLayer.Payload.Should().Be.SameInstanceAs(newPayload);
			newLayer.Period.Should().Be.EqualTo(newPeriod);
		}

		[Test]
		public void ShouldReplacePersonalShiftLayer()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain().AddPersonalLayer().CreatePart();
			var newPayload = new Activity("d");
			var newPeriod = new DateTimePeriod();
			var orgLayerCollection = scheduleDay.PersonAssignment().PersonalActivities();

			target.Replace(scheduleDay, orgLayerCollection.Single(), newPayload, newPeriod);

			var newLayerCollection = scheduleDay.PersonAssignment().PersonalActivities();
			orgLayerCollection.Count().Should().Be.EqualTo(newLayerCollection.Count());
			var newLayer = newLayerCollection.Single();
			newLayer.Payload.Should().Be.SameInstanceAs(newPayload);
			newLayer.Period.Should().Be.EqualTo(newPeriod);
		}

		[Test]
		public void ShouldReplaceOvertimeLayer()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain().AddOvertime().CreatePart();
			var newPayload = new Activity("d");
			var newPeriod = new DateTimePeriod();
			var orgLayerCollection = scheduleDay.PersonAssignment().OvertimeActivities().ToList();

			target.Replace(scheduleDay, orgLayerCollection.Single(), newPayload, newPeriod);

			var newLayerCollection = scheduleDay.PersonAssignment().OvertimeActivities();
			orgLayerCollection.Count.Should().Be.EqualTo(newLayerCollection.Count());
			var newLayer = newLayerCollection.Single();
			newLayer.Payload.Should().Be.SameInstanceAs(newPayload);
			newLayer.Period.Should().Be.EqualTo(newPeriod);
			newLayer.DefinitionSet.Should()
							.Be.SameInstanceAs(
								scheduleDay.PersonAssignment().OvertimeActivities()
													 .Single()
													 .DefinitionSet);
		}

		[Test]
		public void ShouldReplaceCorrectMainShiftLayerWhenMixedLayerTypes()
		{
			var newActivity = new Activity("d");
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime()
											.AddMainShiftLayer()
											.AddMainShiftLayer()
											.AddPersonalLayer()
											.CreatePart();
			target.Replace(scheduleDay, scheduleDay.PersonAssignment().MainActivities().First(), newActivity, new DateTimePeriod());
			scheduleDay.PersonAssignment().MainActivities().First().Payload
				.Should().Be.SameInstanceAs(newActivity);
		}

		[Test]
		public void ShouldReplaceCorrectPersonalShiftLayerWhenMixedLayerTypes()
		{
			var newActivity = new Activity("d");
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime()
											.AddPersonalLayer()
											.AddPersonalLayer()
											.AddMainShiftLayer()
											.CreatePart();
			target.Replace(scheduleDay, scheduleDay.PersonAssignment().PersonalActivities().First(), newActivity, new DateTimePeriod());
			scheduleDay.PersonAssignment().PersonalActivities().First().Payload
				.Should().Be.SameInstanceAs(newActivity);
		}

		[Test]
		public void ShouldReplaceCorrectOvertimeShiftLayerWhenMixedLayerTypes()
		{
			var newActivity = new Activity("d");
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddPersonalLayer()
											.AddOvertime()
											.AddOvertime()
											.AddMainShiftLayer()
											.CreatePart();
			target.Replace(scheduleDay, scheduleDay.PersonAssignment().OvertimeActivities().First(), newActivity, new DateTimePeriod());
			scheduleDay.PersonAssignment().OvertimeActivities().First().Payload
				.Should().Be.SameInstanceAs(newActivity);
		}

		[Test]
		public void ShouldKeepAssignmentInstance()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain().AddMainShiftLayer().CreatePart();
			var orgAss = scheduleDay.PersonAssignment();
			var newPayload = new Activity("d");
			var newPeriod = new DateTimePeriod();
			var orgLayerCollection = scheduleDay.PersonAssignment().MainActivities();

			target.Replace(scheduleDay, orgLayerCollection.First(), newPayload, newPeriod);

			scheduleDay.PersonAssignment().Should().Be.SameInstanceAs(orgAss);
		}

		[Test]
		public void ShouldKeepPersonAbsenceInstance()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain().AddAbsence().CreatePart();
			var orgAbsence = scheduleDay.PersonAbsenceCollection().Single();
			target.Replace(scheduleDay, orgAbsence.Layer, new Absence(), orgAbsence.Layer.Period);
			scheduleDay.PersonAbsenceCollection().Single().Should().Be.SameInstanceAs(orgAbsence);
			scheduleDay.PersonAbsenceCollection().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ReplaceMainShiftLayer_WhenOtherLayersExists_ShouldKeepOldLayers()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain()
				.AddMainShiftLayer()
				.AddMainShiftLayer()
				.CreatePart();

			var firstMainShiftLayer = scheduleDay.PersonAssignment().MainActivities().First();
			var secondMainShiftLayer = scheduleDay.PersonAssignment().MainActivities().Skip(1).First();
		
			var newPayload = new Activity("d");
			var newPeriod = new DateTimePeriod();
			
			target.Replace(scheduleDay, secondMainShiftLayer, newPayload, newPeriod);

			scheduleDay.PersonAssignment().MainActivities().First().Should().Be.SameInstanceAs(firstMainShiftLayer);
		}
	}
}