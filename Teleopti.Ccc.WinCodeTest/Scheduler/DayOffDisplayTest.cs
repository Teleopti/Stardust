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
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class DayOffDisplayTest
    {
        private DayOffDisplay _target;
        private AnchorDateTimePeriod _dateTimePeriod;
        private PersonDayOff _personDayOff;
        private DateTime _start = new DateTime(2007, 11, 5, 8, 0, 0, DateTimeKind.Utc);
        private Person _agent;
        private Scenario _scenario;
        private TimeSpan _targetLength;
        private Percent _percent;

        [SetUp]
        public void Setup()
        {
            _percent = new Percent(1);
            _targetLength = new TimeSpan(1, 0, 0);
            _scenario = new Scenario("Test");
            _agent = new Person();
            _dateTimePeriod = new AnchorDateTimePeriod(_start, _targetLength, _percent);
            _personDayOff = new PersonDayOff(_agent, _scenario, _dateTimePeriod);
            _target = new DayOffDisplay(_personDayOff);//, _start);
        }
        [Test]
        public void CanCreateInstanceAndSetAndGetProperties()
        {
            //DateTime localStart = new DateTime(2007, 11, 5, 0, 0, 0);
            //_target = new DayOffDisplay(_personDayOff, localStart);
            Assert.AreEqual(_start.Date, _target.AnchorDate);
            Assert.IsNotNull(_target);
            Assert.AreEqual(_personDayOff, _target.DayOff);
            Assert.AreEqual(DisplayMode.DayOff, _target.DisplayMode);
        }
    }
}
