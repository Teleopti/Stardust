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
			var orgLayerCollection = scheduleDay.PersonAssignment().MainLayers();

			target.Replace(scheduleDay, orgLayerCollection.First(), newPayload, newPeriod);

			var newLayerCollection = scheduleDay.PersonAssignment().MainLayers();
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
			var orgLayerCollection = scheduleDay.PersonAssignment().PersonalLayers();

			target.Replace(scheduleDay, orgLayerCollection.Single(), newPayload, newPeriod);

			var newLayerCollection = scheduleDay.PersonAssignment().PersonalLayers();
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
			var orgLayerCollection = scheduleDay.PersonAssignment().OvertimeLayers().ToList();

			target.Replace(scheduleDay, orgLayerCollection.Single(), newPayload, newPeriod);

			var newLayerCollection = scheduleDay.PersonAssignment().OvertimeLayers();
			orgLayerCollection.Count.Should().Be.EqualTo(newLayerCollection.Count());
			var newLayer = newLayerCollection.Single();
			newLayer.Payload.Should().Be.SameInstanceAs(newPayload);
			newLayer.Period.Should().Be.EqualTo(newPeriod);
			newLayer.DefinitionSet.Should()
							.Be.SameInstanceAs(
								scheduleDay.PersonAssignment().OvertimeLayers()
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
			target.Replace(scheduleDay, scheduleDay.PersonAssignment().MainLayers().First(), newActivity, new DateTimePeriod());
			scheduleDay.PersonAssignment().MainLayers().First().Payload
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
			target.Replace(scheduleDay, scheduleDay.PersonAssignment().PersonalLayers().First(), newActivity, new DateTimePeriod());
			scheduleDay.PersonAssignment().PersonalLayers().First().Payload
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
			target.Replace(scheduleDay, scheduleDay.PersonAssignment().OvertimeLayers().First(), newActivity, new DateTimePeriod());
			scheduleDay.PersonAssignment().OvertimeLayers().First().Payload
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
			var orgLayerCollection = scheduleDay.PersonAssignment().MainLayers();

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

			var firstMainShiftLayer = scheduleDay.PersonAssignment().MainLayers().First();
			var secondMainShiftLayer = scheduleDay.PersonAssignment().MainLayers().Skip(1).First();
		
			var newPayload = new Activity("d");
			var newPeriod = new DateTimePeriod();
			
			target.Replace(scheduleDay, secondMainShiftLayer, newPayload, newPeriod);

			scheduleDay.PersonAssignment().MainLayers().First().Should().Be.SameInstanceAs(firstMainShiftLayer);
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

			var layer = scheduleDay.PersonAssignment().MainLayers().First(l => l.Payload.Name == "activity2");
			target.Replace(scheduleDay,layer,layer.Payload,layer.Period);

			scheduleDay.PersonAssignment().MainLayers().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity2", "activity3");
		}

		[Test]
		public void ReplaceMainShiftLayer_WhenOnlyOneLayer_ShouldNotChangeTheAmountOfLayers()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddMainShiftLayer()
											.CreatePart();

			var layer = scheduleDay.PersonAssignment().MainLayers().First();
			target.Replace(scheduleDay, layer, layer.Payload, layer.Period);

			scheduleDay.PersonAssignment().MainLayers().Count().Should().Be.EqualTo(1);
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

			var layer = scheduleDay.PersonAssignment().PersonalLayers().First(l => l.Payload.Name == "activity2");
			target.Replace(scheduleDay, layer, layer.Payload, layer.Period);

			scheduleDay.PersonAssignment().PersonalLayers().Select(l => l.Payload.Description.Name)
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

			var layer = scheduleDay.PersonAssignment().OvertimeLayers().First(l => l.Payload.Name == "activity2");
			target.Replace(scheduleDay, layer, layer.Payload, layer.Period);

			scheduleDay.PersonAssignment().OvertimeLayers().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity3", "activity2");
		}
	}
}