using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class AgentAssignmentDisplayTest
    {
        private AgentAssignmentDisplay _target;
        private MainShiftActivityLayer _activityLayer;
        private DateTimePeriod _dateTimePeriod;
        private PersonAssignment _personAssignment;
        private Activity _activity;
        private DateTime _start = new DateTime(2007, 11, 5, 8, 0, 0, DateTimeKind.Utc);
        private DateTime _end = new DateTime(2007, 11, 5, 17, 0, 0, DateTimeKind.Utc);
        private Person _agent;
        private Scenario _scenario;
        private MainShift _mainShift;
        private ShiftCategory _shiftCat;
        private DateTimePeriod _temp;
        DateTime _localStart;
        DateTime _localEnd;

        [SetUp]
        public void Setup()
        {
            _localStart = new DateTime(2007, 11, 5, 0, 0, 0);
            _localEnd = new DateTime(2007, 11, 5, 23, 59, 59);
            _scenario = new Scenario("Test");
            _agent = new Person();
            _dateTimePeriod = new DateTimePeriod(_start, _end);
            _activity = new Activity("Telefon");
            _activity.DisplayColor = Color.AliceBlue;
            _personAssignment = new PersonAssignment(_agent, _scenario);
            _shiftCat = new ShiftCategory("Morgon");
            _mainShift = new MainShift(_shiftCat);
            _activityLayer = new MainShiftActivityLayer(_activity, _dateTimePeriod);
            _mainShift.LayerCollection.Add(_activityLayer);
            _personAssignment.SetMainShift(_mainShift);
            _target = new AgentAssignmentDisplay(_personAssignment, _dateTimePeriod);_temp =
                new DateTimePeriod(TimeZoneHelper.ConvertToUtc(_localStart), TimeZoneHelper.ConvertToUtc(_localEnd));
        }
        [Test]
        public void CanCreateInstanceAndSetAndGetProperties()
        {
            DateTime localStart = new DateTime(2007, 11, 5, 0, 0, 0);
            DateTime localEnd = new DateTime(2007, 11, 5, 23, 59, 59);
            DateTimePeriod temp =
                new DateTimePeriod(TimeZoneHelper.ConvertToUtc(localStart), TimeZoneHelper.ConvertToUtc(localEnd));
            _target = new AgentAssignmentDisplay(_personAssignment, temp);
            Assert.IsNotNull(_target);
            Assert.AreEqual(_activityLayer, _target.PersonAssignment.MainShift.LayerCollection[0]);
        }
        [Test]
        public void VerifySetDisplayModeReturnsBeginsAndEndsToday()
        {
            _target = new AgentAssignmentDisplay(_personAssignment, _temp);

            Assert.AreEqual(DisplayMode.BeginsAndEndsToday, _target.DisplayMode);
        }
        [Test]
        public void VerifySetDisplayModeReturnsWholeDay()
        {
            _mainShift.LayerCollection.Clear();
            _start = new DateTime(2007, 10, 5, 8, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2007, 12, 5, 17, 0, 0, DateTimeKind.Utc);
            _dateTimePeriod = new DateTimePeriod(_start, _end);
            _activityLayer = new MainShiftActivityLayer(_activity, _dateTimePeriod);
            _mainShift.LayerCollection.Add(_activityLayer);
            _personAssignment.SetMainShift(_mainShift);
            _target = new AgentAssignmentDisplay(_personAssignment, _temp);

            Assert.AreEqual(DisplayMode.WholeDay, _target.DisplayMode);
        }
        [Test]
        public void VerifySetDisplayModeReturnsBeginsToday()
        {
            _mainShift.LayerCollection.Clear();
            _start = new DateTime(2007, 11, 5, 8, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2007, 12, 5, 17, 0, 0, DateTimeKind.Utc);
            _dateTimePeriod = new DateTimePeriod(_start, _end);
            _activityLayer = new MainShiftActivityLayer(_activity, _dateTimePeriod);
            _mainShift.LayerCollection.Add(_activityLayer);
            _personAssignment.SetMainShift(_mainShift);
            _target = new AgentAssignmentDisplay(_personAssignment, _temp);

            Assert.AreEqual(DisplayMode.BeginsToday, _target.DisplayMode);
        }

        [Test]
        public void VerifySetDisplayModeReturnsEndsToday()
        {
            _mainShift.LayerCollection.Clear();
            _start = new DateTime(2007, 11, 4, 8, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2007, 11, 5, 17, 0, 0, DateTimeKind.Utc);
            _dateTimePeriod = new DateTimePeriod(_start, _end);
            _activityLayer = new MainShiftActivityLayer(_activity, _dateTimePeriod);
            _mainShift.LayerCollection.Add(_activityLayer);
            _personAssignment.SetMainShift(_mainShift);
            _target = new AgentAssignmentDisplay(_personAssignment, _temp);

            Assert.AreEqual(DisplayMode.EndsToday, _target.DisplayMode);
        }
    }
}
