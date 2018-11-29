using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Common.Time
{
    [TestFixture]
    public class LayerViewModelGroupMoveTest
    {
        private ILayerViewModel _mainShiftLayerViewModel;
        private IAbsence _abs;
        private DateTimePeriod _period;
        private IEventAggregator _eventAggregator;
        private MainShiftLayer _actLayer;
        private ILayerViewModel _absenceLayerViewModel;
        private ILayerViewModel _overtimeLayerViewModel;
    

        [SetUp]
        public void Setup()
        {
           
            _eventAggregator = new EventAggregator();
            _abs = AbsenceFactory.CreateAbsence("for test");
            _period = new DateTimePeriod(2001, 1, 1, 2001, 1, 2);
            _actLayer = new MainShiftLayer(ActivityFactory.CreateActivity("test"), _period);

            _mainShiftLayerViewModel = new MainShiftLayerViewModel(null, _actLayer, new PersonAssignment(new Person(), new Scenario(), DateOnly.Today), null);
            _absenceLayerViewModel = new AbsenceLayerViewModel(null, new PersonAbsence(new Person(), new Scenario(), new AbsenceLayer(_abs, _period)),null);
	        _overtimeLayerViewModel = new OvertimeLayerViewModel(null,
	                                                             new OvertimeShiftLayer(
		                                                             ActivityFactory.CreateActivity("d"), _period,
		                                                             new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime)),
				new PersonAssignment(new Person(), new Scenario(), DateOnly.Today),
																															 null);
        }

        [Test]
        public void VerifyAbsenceLayerGroupMove()
        {
         
            ILayerViewModel anotherAbsenceLayer = new AbsenceLayerViewModel(null, new PersonAbsence(new Person(), new Scenario(), new AbsenceLayer(_abs, _period)), null);

            Assert.IsFalse(_absenceLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel),"Should not move, because its the SAME layer");
            Assert.IsTrue(_absenceLayerViewModel.ShouldBeIncludedInGroupMove(anotherAbsenceLayer), "Should  move");
            Assert.IsFalse(_absenceLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel),"Should not move, only when absence is moved");

        }

        [Test]
        public void VerifyMainShiftLayerViewModelGroupMove()
        {
					MainShiftLayerViewModel anotherMainShiftLayerViewModel = new MainShiftLayerViewModel(null, _actLayer, new PersonAssignment(new Person(), new Scenario(), DateOnly.Today), null);

            Assert.IsFalse(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel),"Should not move when an absecnelayerviewmodel is moved");
            Assert.IsFalse(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel), "Should not move when its the SAME layer");
            Assert.IsTrue(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(anotherMainShiftLayerViewModel));
            Assert.IsFalse(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(_overtimeLayerViewModel), "not moved when overtime layer is moved");

	        _mainShiftLayerViewModel = new MainShiftLayerViewModel(MockRepository.GenerateMock<IVisualLayer>(), new Person());
            Assert.IsTrue(_mainShiftLayerViewModel.IsProjectionLayer,"Just checking that it's a Projectionlayer");
            Assert.False(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(anotherMainShiftLayerViewModel),"Projectionlayers should not be moved");
           
        }

        [Test]
        public void MeetingLayerViewModelGroupMove()
        {
            //Meetings should never move for now, only when using MeetingEditor!

            MeetingLayerViewModel meetingLayerViewModel = new MeetingLayerViewModel(null, createPersonMeeting(),_eventAggregator);
            MeetingLayerViewModel anohterMeetingLayerViewModel = new MeetingLayerViewModel(null, createPersonMeeting(), _eventAggregator);

            Assert.IsFalse(meetingLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel),"not moved when mainshift is moved");
            Assert.IsFalse(meetingLayerViewModel.ShouldBeIncludedInGroupMove(anohterMeetingLayerViewModel), "not moved when another meeting is moved");
            Assert.IsFalse(meetingLayerViewModel.ShouldBeIncludedInGroupMove(meetingLayerViewModel), "not moved when same is moved");

        }

			private static IPersonMeeting createPersonMeeting()
			{
				var meetingPerson = new MeetingPerson(new Person(), false);
				var meeting = new Meeting(new Person(), new[] { meetingPerson }, "subject", "location", "description", ActivityFactory.CreateActivity("activity"), ScenarioFactory.CreateScenarioAggregate());
				return new PersonMeeting(meeting, meetingPerson, new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			}

        [Test]
        public void VerifyOvertimeLayerViewModelGroupMove()
        {
            OvertimeLayerViewModel anotherOvertimeLayerViewModel = new OvertimeLayerViewModel(null, new OvertimeShiftLayer(new Activity("d"), new DateTimePeriod(2000,1,1,2000,1,2), new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime)), new PersonAssignment(new Person(), new Scenario(), DateOnly.Today), null);

            Assert.IsFalse(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel), "not moved when absencelayerviewmodel layer is moved");
            Assert.IsTrue(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(anotherOvertimeLayerViewModel), "should be moved when another overtime layer is moved");
            Assert.IsFalse(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(_overtimeLayerViewModel), "not moved when same is moved");
            Assert.IsTrue(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel), "should be moved when a mainshift layer is moved");
        
        }

        [Test]
        public void VerifyPersonalShiftLayerViewModelGroupMove()
        {
	        var pLayer = new PersonalShiftLayer(new Activity("d"), _period);
            PersonalShiftLayerViewModel personalShiftLayerViewModel = new PersonalShiftLayerViewModel(null, pLayer, new PersonAssignment(new Person(), new Scenario(), DateOnly.Today), _eventAggregator);
						PersonalShiftLayerViewModel anotherPersonalShiftLayerViewModel = new PersonalShiftLayerViewModel(null, pLayer, new PersonAssignment(new Person(), new Scenario(), DateOnly.Today), _eventAggregator);

            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(_overtimeLayerViewModel), "not moved when an overtimelayerviewmodel layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel), "not moved when a main shift layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel), "not moved when absencelayerviewmodel layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(personalShiftLayerViewModel), "not moved when personalShiftLayerViewModel layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(anotherPersonalShiftLayerViewModel), "not moved when another personalShiftLayerViewModel layer is moved");
        }
    }
}
