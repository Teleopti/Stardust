﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
        private ActivityLayer _actLayer;
        private ILayerViewModel _absenceLayerViewModel;
        private ILayerViewModel _overtimeLayerViewModel;
    

        [SetUp]
        public void Setup()
        {
           
            _eventAggregator = new EventAggregator();
            _abs = AbsenceFactory.CreateAbsence("for test");
            _period = new DateTimePeriod(2001, 1, 1, 2001, 1, 2);
            _actLayer = new ActivityLayer(ActivityFactory.CreateActivity("test"), _period);

            _mainShiftLayerViewModel = new MainShiftLayerViewModel(_actLayer, _eventAggregator);
            _absenceLayerViewModel = new AbsenceLayerViewModel(new AbsenceLayer(_abs, _period), _eventAggregator);
            _overtimeLayerViewModel = new OvertimeLayerViewModel(_actLayer, _eventAggregator);
        }

        [Test]
        public void VerifyAbsenceLayerGroupMove()
        {
         
            ILayerViewModel anotherAbsenceLayer = new AbsenceLayerViewModel(new AbsenceLayer(_abs, _period),_eventAggregator);

            Assert.IsFalse(_absenceLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel),"Should not move, because its the SAME layer");
            Assert.IsTrue(_absenceLayerViewModel.ShouldBeIncludedInGroupMove(anotherAbsenceLayer), "Should  move");
            Assert.IsFalse(_absenceLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel),"Should not move, only when absence is moved");

        }

        [Test]
        public void VerifyMainShiftLayerViewModelGroupMove()
        {
            MainShiftLayerViewModel anotherMainShiftLayerViewModel = new MainShiftLayerViewModel(_actLayer, _eventAggregator);

            Assert.IsFalse(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel),"Should not move when an absecnelayerviewmodel is moved");
            Assert.IsFalse(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel), "Should not move when its the SAME layer");
            Assert.IsTrue(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(anotherMainShiftLayerViewModel));
            Assert.IsFalse(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(_overtimeLayerViewModel), "not moved when overtime layer is moved");

            ((MainShiftLayerViewModel)_mainShiftLayerViewModel).IsProjectionLayer = true;
            Assert.IsTrue(_mainShiftLayerViewModel.IsProjectionLayer,"Just checking that it's a Projectionlayer");
            Assert.False(_mainShiftLayerViewModel.ShouldBeIncludedInGroupMove(anotherMainShiftLayerViewModel),"Projectionlayers should not be moved");
           
        }

        [Test]
        public void MeetingLayerViewModelGroupMove()
        {
            //Meetings should never move for now, only when using MeetingEditor!

            MeetingLayerViewModel meetingLayerViewModel = new MeetingLayerViewModel(_actLayer,_eventAggregator);
            MeetingLayerViewModel anohterMeetingLayerViewModel = new MeetingLayerViewModel(_actLayer, _eventAggregator);

            Assert.IsFalse(meetingLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel),"not moved when mainshift is moved");
            Assert.IsFalse(meetingLayerViewModel.ShouldBeIncludedInGroupMove(anohterMeetingLayerViewModel), "not moved when another meeting is moved");
            Assert.IsFalse(meetingLayerViewModel.ShouldBeIncludedInGroupMove(meetingLayerViewModel), "not moved when same is moved");

        }

        [Test]
        public void VerifyOvertimeLayerViewModelGroupMove()
        {
            OvertimeLayerViewModel anotherOvertimeLayerViewModel = new OvertimeLayerViewModel(_actLayer, _eventAggregator);

            Assert.IsFalse(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel), "not moved when absencelayerviewmodel layer is moved");
            Assert.IsTrue(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(anotherOvertimeLayerViewModel), "should be moved when another overtime layer is moved");
            Assert.IsFalse(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(_overtimeLayerViewModel), "not moved when same is moved");
            Assert.IsTrue(_overtimeLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel), "should be moved when a mainshift layer is moved");
        
        }

        [Test]
        public void VerifyPersonalShiftLayerViewModelGroupMove()
        {
            PersonalShiftLayerViewModel personalShiftLayerViewModel = new PersonalShiftLayerViewModel(_actLayer, _eventAggregator);
            PersonalShiftLayerViewModel anotherPersonalShiftLayerViewModel = new PersonalShiftLayerViewModel(_actLayer, _eventAggregator);

            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(_overtimeLayerViewModel), "not moved when an overtimelayerviewmodel layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(_mainShiftLayerViewModel), "not moved when a main shift layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(_absenceLayerViewModel), "not moved when absencelayerviewmodel layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(personalShiftLayerViewModel), "not moved when personalShiftLayerViewModel layer is moved");
            Assert.IsFalse(personalShiftLayerViewModel.ShouldBeIncludedInGroupMove(anotherPersonalShiftLayerViewModel), "not moved when another personalShiftLayerViewModel layer is moved");
        }
    }
}
