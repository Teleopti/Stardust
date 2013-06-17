﻿using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Time
{
    [TestFixture]
    public class LayerViewModelGroupMoveTest
    {
        private ILayerViewModel _mainShiftLayerViewModel;
        private IAbsence _abs;
        private DateTimePeriod _period;
        private IEventAggregator _eventAggregator;
        private IMainShiftActivityLayerNew _actLayer;
        private ILayerViewModel _absenceLayerViewModel;
        private ILayerViewModel _overtimeLayerViewModel;
    

        [SetUp]
        public void Setup()
        {
           
            _eventAggregator = new EventAggregator();
            _abs = AbsenceFactory.CreateAbsence("for test");
            _period = new DateTimePeriod(2001, 1, 1, 2001, 1, 2);
            _actLayer = new MainShiftActivityLayerNew(ActivityFactory.CreateActivity("test"), _period);

            _mainShiftLayerViewModel = new MainShiftLayerViewModel(null, _actLayer, null, null, null);
            _absenceLayerViewModel = new AbsenceLayerViewModel(null, new AbsenceLayer(_abs, _period),null);
	        _overtimeLayerViewModel = new OvertimeLayerViewModel(null,
	                                                             new OvertimeShiftActivityLayer(
		                                                             ActivityFactory.CreateActivity("d"), _period,
		                                                             new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime)),
	                                                             null,
																															 null,
																															 null);
        }

        [Test]
        public void VerifyAbsenceLayerGroupMove()
        {
         
            ILayerViewModel anotherAbsenceLayer = new AbsenceLayerViewModel(null, new AbsenceLayer(_abs, _period), null);

            Assert.IsFalse(_absenceLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel),"Should not move, because its the SAME layer");
            Assert.IsTrue(_absenceLayerViewModel.ShouldBeIncludedInGroupMove(anotherAbsenceLayer), "Should  move");
            Assert.IsFalse(_absenceLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel),"Should not move, only when absence is moved");

        }

        [Test]
        public void VerifyMainShiftLayerViewModelGroupMove()
        {
					MainShiftLayerViewModel anotherMainShiftLayerViewModel = new MainShiftLayerViewModel(null, _actLayer, null, null, null);

            Assert.IsFalse(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel),"Should not move when an absecnelayerviewmodel is moved");
            Assert.IsFalse(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel), "Should not move when its the SAME layer");
            Assert.IsTrue(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(anotherMainShiftLayerViewModel));
            Assert.IsFalse(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(_overtimeLayerViewModel), "not moved when overtime layer is moved");

	        _mainShiftLayerViewModel = new MainShiftLayerViewModel(MockRepository.GenerateMock<IVisualLayer>());
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
            OvertimeLayerViewModel anotherOvertimeLayerViewModel = new OvertimeLayerViewModel(null, new OvertimeShiftActivityLayer(new Activity("d"), new DateTimePeriod(2000,1,1,2000,1,2), new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime)), null, null,null);

            Assert.IsFalse(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel), "not moved when absencelayerviewmodel layer is moved");
            Assert.IsTrue(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(anotherOvertimeLayerViewModel), "should be moved when another overtime layer is moved");
            Assert.IsFalse(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(_overtimeLayerViewModel), "not moved when same is moved");
            Assert.IsTrue(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel), "should be moved when a mainshift layer is moved");
        
        }

        [Test]
        public void VerifyPersonalShiftLayerViewModelGroupMove()
        {
            PersonalShiftLayerViewModel personalShiftLayerViewModel = new PersonalShiftLayerViewModel(null, _actLayer, null, _eventAggregator);
            PersonalShiftLayerViewModel anotherPersonalShiftLayerViewModel = new PersonalShiftLayerViewModel(null, _actLayer, null, _eventAggregator);

            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(_overtimeLayerViewModel), "not moved when an overtimelayerviewmodel layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel), "not moved when a main shift layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel), "not moved when absencelayerviewmodel layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(personalShiftLayerViewModel), "not moved when personalShiftLayerViewModel layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(anotherPersonalShiftLayerViewModel), "not moved when another personalShiftLayerViewModel layer is moved");
        }
    }
}
