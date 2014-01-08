﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class MoveLayerVerticalTest
	{
		[Test]
		public void ShouldMoveUpMainshiftLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			var orgLayers = new List<IMainShiftLayer>(ass.MainActivities());
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveUp(ass, lastLayer);
			//new instances - cant check for equality
			var res = ass.MainActivities().ToArray();
			res[0].Period.Should().Be.EqualTo(firstLayer.Period);
			res[1].Period.Should().Be.EqualTo(lastLayer.Period);
			res[2].Period.Should().Be.EqualTo(middleLayer.Period);
		}

		[Test]
		public void ShouldMoveUpOvertimeLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeOvertimeLayers();
			var orgLayers = ass.OvertimeActivities().ToList();
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveUp(ass, lastLayer);
			//new instances - cant check for equality
			var res = ass.OvertimeActivities().ToArray();
			res[0].Period.Should().Be.EqualTo(firstLayer.Period);
			res[1].Period.Should().Be.EqualTo(lastLayer.Period);
			res[2].Period.Should().Be.EqualTo(middleLayer.Period);
		}

		[Test]
		public void ShouldMoveUpPersonalLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreePersonalLayers();
			var orgLayers = ass.PersonalActivities().ToArray();
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];
			target.MoveUp(ass, lastLayer);
			//new instances - cant check for equality
			var res = ass.PersonalActivities().ToArray();
			res[0].Period.Should().Be.EqualTo(firstLayer.Period);
			res[1].Period.Should().Be.EqualTo(lastLayer.Period);
			res[2].Period.Should().Be.EqualTo(middleLayer.Period);
		}

		[Test]
		public void ShouldMoveDownMainshiftLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			var orgLayers = new List<IMainShiftLayer>(ass.MainActivities());
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveDown(ass, firstLayer);
			//new instances - cant check for equality
			var res = ass.MainActivities().ToArray();
			res[0].Period.Should().Be.EqualTo(middleLayer.Period);
			res[1].Period.Should().Be.EqualTo(firstLayer.Period);
			res[2].Period.Should().Be.EqualTo(lastLayer.Period);
		}

		[Test]
		public void ShouldMoveDownOvertimeLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeOvertimeLayers();
			var orgLayers = ass.OvertimeActivities().ToList();
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveDown(ass, firstLayer);
			//new instances - cant check for equality
			var res = ass.OvertimeActivities().ToArray();
			res[0].Period.Should().Be.EqualTo(middleLayer.Period);
			res[1].Period.Should().Be.EqualTo(firstLayer.Period);
			res[2].Period.Should().Be.EqualTo(lastLayer.Period);
		}

		[Test]
		public void ShouldMoveDownPersonalLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreePersonalLayers();
			var orgLayers = ass.PersonalActivities().ToArray();
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];
			target.MoveDown(ass, firstLayer);
			//new instances - cant check for equality
			var res = ass.PersonalActivities().ToArray();
			res[0].Period.Should().Be.EqualTo(middleLayer.Period);
			res[1].Period.Should().Be.EqualTo(firstLayer.Period);
			res[2].Period.Should().Be.EqualTo(lastLayer.Period);
		}

		[Test]
		public void ShouldMoveUpPersonalLayerWhenOtherLayerIsAbove()
		{
			var activityToLookFor = new Activity("d");
			var target = new MoveLayerVertical();
			var ass = new PersonAssignment(new Person(), new Scenario("sd"), new DateOnly(2000, 1, 1));
			ass.AddActivity(new Activity("d"), new DateTimePeriod());
			ass.AddPersonalActivity(activityToLookFor, new DateTimePeriod());
			ass.AddPersonalActivity(new Activity("d"), new DateTimePeriod());

			target.MoveUp(ass, ass.PersonalActivities().Last());

			ass.PersonalActivities().Last().Payload.Should().Be.SameInstanceAs(activityToLookFor);
		}

		[Test]
		public void ShouldMoveDownOvertimeLayerWhenOtherLayerIsAbove()
		{
			var activityToLookFor = new Activity("d");
			var target = new MoveLayerVertical();
			var ass = new PersonAssignment(new Person(), new Scenario("sd"), new DateOnly(2000, 1, 1));
			ass.AddActivity(new Activity("d"), new DateTimePeriod());
			ass.AddOvertimeActivity(activityToLookFor, new DateTimePeriod(),null);
			ass.AddOvertimeActivity(new Activity("d"), new DateTimePeriod(), null);

			target.MoveDown(ass, ass.OvertimeActivities().First());

			ass.OvertimeActivities().Last().Payload.Should().Be.SameInstanceAs(activityToLookFor);
		}

		[Test]
		public void MoveUpMainLayers_WhenThereAreMainAndPersonalLayers_ShouldNotChangeTheOrderOfTheMainShiftLayers()
		{
			var target = new MoveLayerVertical();
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

			target.MoveUp(scheduleDay.PersonAssignment(), scheduleDay.PersonAssignment().ShiftLayers.First(s => s.Payload.Description.Name == "activity3"));

			scheduleDay.PersonAssignment().MainLayers().Select(l => l.Payload.Description.Name)
			                             .Should().Have.SameSequenceAs("activity1", "activity3", "activity2");
		}

		[Test]
		public void MoveUpPersonal_WhenThereAreMainAndPersonalLayers_ShouldNotChangeTheOrderOfTheMainShiftLayers()
		{
			var target = new MoveLayerVertical();
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime()
											.AddPersonalLayer("activity1")
											.AddMainShiftLayer()
											.AddMainShiftLayer()
											.AddPersonalLayer("activity2")
											.AddMainShiftLayer()
											.AddPersonalLayer("activity3")
											.CreatePart();

			target.MoveUp(scheduleDay.PersonAssignment(), scheduleDay.PersonAssignment().ShiftLayers.First(s => s.Payload.Description.Name == "activity3"));

			scheduleDay.PersonAssignment().PersonalLayers().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity3", "activity2");
		}
	}
}