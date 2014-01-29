using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class MoveLayerVerticalTest
	{
		[Test]
		public void MoveUpPersonal_WhenThereAreMainAndPersonalLayers_ShowMoveLayerBeforeTheTargetLayer()
		{
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime()
											.AddPersonalLayer("activity1")
											.AddMainShiftLayer()
											.AddMainShiftLayer()
											.AddPersonalLayer("activity2")
											.AddMainShiftLayer()
											.AddPersonalLayer("activity3")
											.CreatePart();

			var layerToMove = scheduleDay.PersonAssignment().ShiftLayers.First(s => s.Payload.Description.Name == "activity3");

			var target = new MoveShiftLayerUp();
			scheduleDay.PersonAssignment().MoveLayerVertical(target,layerToMove);

			scheduleDay.PersonAssignment().PersonalActivities().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity3", "activity2");
		}

		[Test]
		public void MoveDownPersonal_WhenThereAreMainAndPersonalLayers_ShowMoveLayerAfterTheTargetLayer()
		{
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime()
											.AddPersonalLayer("activity1")
											.AddMainShiftLayer()
											.AddMainShiftLayer()
											.AddPersonalLayer("activity2")
											.AddMainShiftLayer()
											.AddPersonalLayer("activity3")
											.CreatePart();

			var layerToMove = scheduleDay.PersonAssignment().ShiftLayers.First(s => s.Payload.Description.Name == "activity1");
			var target = new MoveShiftLayerDown();

			scheduleDay.PersonAssignment().MoveLayerVertical(target, layerToMove);

			scheduleDay.PersonAssignment().PersonalActivities().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity2", "activity1", "activity3");
		}

		[Test]
		public void Move_MainShiftLayerUp_ShouldMoveTheTargetLayerAboveTheMainShiftLayerThatIsBeforeTheTargetLayer()
		{
			
			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime()
											.AddPersonalLayer()
											.AddMainShiftLayer("activity1")
											.AddMainShiftLayer("activity2")
											.AddPersonalLayer()
											.AddMainShiftLayer("activity3")
											.AddPersonalLayer()
											.CreatePart();

			var layerToMove = scheduleDay.PersonAssignment().ShiftLayers.First(s => s.Payload.Description.Name == "activity3");
			var target = new MoveShiftLayerUp();

			scheduleDay.PersonAssignment().MoveLayerVertical(target,layerToMove);

			scheduleDay.PersonAssignment().MainActivities().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity3", "activity2");
		}

		[Test]
		public void Move_MainShiftLayerDown_ShouldMoveTheTargetLayerBelowTheMainShiftLayerThatIsAfterTheTargetLayer()
		{

			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime()
											.AddPersonalLayer()
											.AddMainShiftLayer("activity1")
											.AddMainShiftLayer("activity2")
											.AddPersonalLayer()
											.AddMainShiftLayer("activity3")
											.AddPersonalLayer()
											.CreatePart();

			var layerToMove = scheduleDay.PersonAssignment().ShiftLayers.First(s => s.Payload.Description.Name == "activity2");
			var target = new MoveShiftLayerDown();

			scheduleDay.PersonAssignment().MoveLayerVertical(target, layerToMove);

			scheduleDay.PersonAssignment().MainActivities().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity3", "activity2");
		}

		[Test]
		public void Move_OvertimeShiftLayerUp_ShouldMoveTheTargetLayerAboveTheOvertimeShiftLayerThatIsBeforeTheTargetLayer()
		{

			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddOvertime("activity1")
											.AddPersonalLayer()
											.AddOvertime("activity2")
											.AddMainShiftLayer()
											.AddPersonalLayer()
											.AddOvertime("activity3")
											.AddPersonalLayer()
											.CreatePart();

			var layerToMove = scheduleDay.PersonAssignment().ShiftLayers.First(s => s.Payload.Description.Name == "activity3");
			var target = new MoveShiftLayerUp();

			scheduleDay.PersonAssignment().MoveLayerVertical(target, layerToMove);

			scheduleDay.PersonAssignment().OvertimeActivities().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity3", "activity2");
		}

		[Test]
		public void Move_OvertimeShiftLayerDown_ShouldMoveTheTargetLayerBelowTheovertimeShiftLayerThatIsAfterTheTargetLayer()
		{

			var scheduleDay = new SchedulePartFactoryForDomain()
											.AddMainShiftLayer()
											.AddPersonalLayer()
											.AddOvertime("activity1")
											.AddOvertime("activity2")
											.AddPersonalLayer()
											.AddOvertime("activity3")
											.AddPersonalLayer()
											.CreatePart();

			var layerToMove = scheduleDay.PersonAssignment().ShiftLayers.First(s => s.Payload.Description.Name == "activity2");
			var target = new MoveShiftLayerDown();

			scheduleDay.PersonAssignment().MoveLayerVertical(target, layerToMove);

			scheduleDay.PersonAssignment().OvertimeActivities().Select(l => l.Payload.Description.Name)
										 .Should().Have.SameSequenceAs("activity1", "activity3", "activity2");
		}

		[Test]
		public void MoveUp_WhenThereAreTwoPersonalLayers_ShouldMoveTheLastPersonalLayerToTheTop()
		{
			var scheduleDay = new SchedulePartFactoryForDomain()
										.AddPersonalLayer("first")
										.AddPersonalLayer("second")
										.CreatePart();

			var layerToMove = scheduleDay.PersonAssignment().PersonalActivities().Last();
			var target = new MoveShiftLayerUp();

			scheduleDay.PersonAssignment().MoveLayerVertical(target, layerToMove);

			scheduleDay.PersonAssignment().PersonalActivities().First().Payload.Description.Name.Should().Be.EqualTo("second");
		}

		[Test]
		public void MoveUp_WhenThereAreTwoMainLayers_ShouldMoveTheLastPersonalLayerToTheTop()
		{
			var scheduleDay = new SchedulePartFactoryForDomain()
										.AddMainShiftLayer("first")
										.AddMainShiftLayer("second")
										.CreatePart();

			var layerToMove = scheduleDay.PersonAssignment().MainActivities().Last();
			var target = new MoveShiftLayerUp();

			scheduleDay.PersonAssignment().MoveLayerVertical(target, layerToMove);

			scheduleDay.PersonAssignment().MainActivities().First().Payload.Description.Name.Should().Be.EqualTo("second");
		}

		[Test]
		public void MoveDown_WhenThereAreTwoMainLayers_ShouldMoveTheFirstMainShiftLayerToTheBottom()
		{
			var scheduleDay = new SchedulePartFactoryForDomain()
										.AddMainShiftLayer("first")
										.AddMainShiftLayer("second")
										.CreatePart();

			var layerToMove = scheduleDay.PersonAssignment().MainActivities().First();
			var target = new MoveShiftLayerDown();

			scheduleDay.PersonAssignment().MoveLayerVertical(target, layerToMove);

			scheduleDay.PersonAssignment().MainActivities().Last().Payload.Description.Name.Should().Be.EqualTo("first");
		}
	}
}