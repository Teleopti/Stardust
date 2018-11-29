using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
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
			var newPeriod = new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9);
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
			var newPeriod = new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9);
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
			var newPeriod = new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9);
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
											.AddPersonalLayer()
											.AddPersonalLayer()
											.AddMainShiftLayer()
											.CreatePart();
			var newPeriod = new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9);
			target.Replace(scheduleDay, scheduleDay.PersonAssignment().MainActivities().First(), newActivity, newPeriod);
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
			var newPeriod = new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9);
			target.Replace(scheduleDay, scheduleDay.PersonAssignment().PersonalActivities().First(), newActivity, newPeriod);
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
			var newPeriod = new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9);
			target.Replace(scheduleDay, scheduleDay.PersonAssignment().OvertimeActivities().First(), newActivity, newPeriod);
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
			var newPeriod = new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9);
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
			scheduleDay.PersonAbsenceCollection().Length.Should().Be.EqualTo(1);
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
			var newPeriod = new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9);
			
			target.Replace(scheduleDay, secondMainShiftLayer, newPayload, newPeriod);

			scheduleDay.PersonAssignment().MainActivities().First().Should().Be.SameInstanceAs(firstMainShiftLayer);
		}

		[Test]
		public void ReplaceMainShiftLayer_WhenOtherLayersExistsInMixedOrder_ShouldNotChangeTheOrderOfTheMainShiftLayers()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime()
											.AddPersonalLayer()
											.AddMainShiftLayer("activity1")
											.AddMainShiftLayer("activity2")
											.AddPersonalLayer()
											.AddMainShiftLayer("activity3")
											.AddPersonalLayer()
											.AddPersonalLayer()
											.CreatePart();

			var layer = scheduleDay.PersonAssignment().MainActivities().First(l => l.Payload.Name == "activity2");
			target.Replace(scheduleDay,layer,layer.Payload,layer.Period);

			scheduleDay.PersonAssignment().MainActivities().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity2", "activity3");
		}

		[Test]
		public void ReplaceMainShiftLayer_WhenOnlyOneLayer_ShouldNotChangeTheAmountOfLayers()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddMainShiftLayer()
											.CreatePart();

			var layer = scheduleDay.PersonAssignment().MainActivities().First();
			target.Replace(scheduleDay, layer, layer.Payload, layer.Period);

			scheduleDay.PersonAssignment().MainActivities().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ReplacePersonalShiftLayer_WhenOtherLayersExistsInMixedVerticalOrder_ShouldNotChangeTheOrderOfThePersonalShiftLayers()
		{
			//var target = new MoveLayerVertical();
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime()
											.AddPersonalLayer("activity1")
											.AddMainShiftLayer()
											.AddMainShiftLayer()
											.AddPersonalLayer("activity2")
											.AddMainShiftLayer()
											.AddOvertime()
											.AddPersonalLayer("activity3")
											.CreatePart();

			var layer = scheduleDay.PersonAssignment().PersonalActivities().First(l => l.Payload.Name == "activity2");
			target.Replace(scheduleDay, layer, layer.Payload, layer.Period);

			scheduleDay.PersonAssignment().PersonalActivities().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity2", "activity3");
		}

		[Test]
		public void ReplaceOvertimeLayer_WhenOtherLayersExistsInMixedVerticalOrder_ShouldNotChangeTheOrderOfTheOvertimeLayers()
		{
			var target = new ReplaceLayerInSchedule();

			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime("activity1")
											.AddPersonalLayer()
											.AddMainShiftLayer()
											.AddMainShiftLayer()
											.AddOvertime("activity2")
											.AddPersonalLayer()
											.AddMainShiftLayer()
											.AddPersonalLayer()
											.AddPersonalLayer()
											.AddOvertime("activity3")
											.CreatePart();

			var layer = scheduleDay.PersonAssignment().OvertimeActivities().First(l => l.Payload.Name == "activity2");
			target.Replace(scheduleDay, layer, layer.Payload, layer.Period);

			scheduleDay.PersonAssignment().OvertimeActivities().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity2", "activity3");
		}
	}
}