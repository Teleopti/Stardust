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
    public class AgentAbsenceDisplayTest
    {
        private AgentAbsenceDisplay _target;
        private AbsenceLayer _absenceLayer;
        private PersonAbsence _personAbsence;
        private DateTimePeriod _dateTimePeriod;
        private Absence _abs;
        private DateTime _start;
        private DateTime _end;
        DateTimePeriod _temp;
        private DateTime _localEnd;
        private DateTime _localStart;

        [SetUp]
        public void Setup()
        {   _start = new DateTime(2007, 11, 5, 8, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2007, 11, 5, 17, 0, 0, DateTimeKind.Utc);
            _dateTimePeriod = new DateTimePeriod(_start, _end);
            _abs = new Absence();
            _abs.DisplayColor = Color.AliceBlue;
            _absenceLayer = new AbsenceLayer(_abs, _dateTimePeriod, false);
            _personAbsence = new PersonAbsence(new Person(), new Scenario("Test"), _absenceLayer);
            _target = new AgentAbsenceDisplay(_personAbsence, _dateTimePeriod);
            _localStart = new DateTime(2007, 11, 5, 0, 0, 0);
            //_localEnd = new DateTime(2007, 11, 5, 23, 59, 59);
            _localEnd = new DateTime(2007, 11, 6, 0, 0, 0);
            _temp = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(_localStart), TimeZoneHelper.ConvertToUtc(_localEnd));
        }
        [Test]
        public void CanCreateInstanceAndSetAndGetProperties()
        {
            _target = new AgentAbsenceDisplay(_personAbsence, _dateTimePeriod);
            Assert.IsNotNull(_target);
            Assert.AreEqual(_abs.DisplayColor, _target.DisplayColor);
            Assert.AreEqual(_absenceLayer, _target.AbsenceLayer);
            Assert.AreEqual(DisplayMode.BeginsAndEndsToday, _target.DisplayMode);
        }
        [Test]
        public void VerifySetDisplayModeCanReturnWholeDay()
        {
            _absenceLayer.IsFullDayAbsence = true;
            _target = new AgentAbsenceDisplay(_personAbsence, _dateTimePeriod);

            Assert.AreEqual(DisplayMode.WholeDay, _target.DisplayMode);

            _start = new DateTime(2007, 10, 5, 8, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2007, 12, 5, 17, 0, 0, DateTimeKind.Utc);
            _dateTimePeriod = new DateTimePeriod(_start, _end);
            _absenceLayer = new AbsenceLayer(_abs, _dateTimePeriod, false);
            _personAbsence = new PersonAbsence(new Person(), new Scenario("Test"), _absenceLayer);
            _target = new AgentAbsenceDisplay(_personAbsence, _temp);

            Assert.AreEqual(DisplayMode.WholeDay, _target.DisplayMode);

        }
        [Test]
        public void VerifySetDisplayModeCanReturnBeginsAndEndsToday()
        {
            _start = new DateTime(2007, 11, 5, 8, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2007, 11, 5, 17, 0, 0, DateTimeKind.Utc);

            _dateTimePeriod = new DateTimePeriod(_start, _end);
            _absenceLayer = new AbsenceLayer(_abs, _dateTimePeriod, false);
            _personAbsence = new PersonAbsence(new Person(), new Scenario("Test"), _absenceLayer);
            _target = new AgentAbsenceDisplay(_personAbsence, _temp);

            Assert.AreEqual(DisplayMode.BeginsAndEndsToday, _target.DisplayMode);
        }

        [Test]
        public void VerifySetDisplayModeCanReturnBeginsToday()
        {
            _start = new DateTime(2007, 11, 5, 8, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2007, 12, 5, 17, 0, 0, DateTimeKind.Utc);

            _dateTimePeriod = new DateTimePeriod(_start, _end);
            _absenceLayer = new AbsenceLayer(_abs, _dateTimePeriod, false);
            _personAbsence = new PersonAbsence(new Person(), new Scenario("Test"), _absenceLayer);
            _target = new AgentAbsenceDisplay(_personAbsence, _temp);

            Assert.AreEqual(DisplayMode.BeginsToday, _target.DisplayMode);
        }

        [Test]
        public void VerifySetDisplayModeCanReturnEndsToday()
        {
            _start = new DateTime(2007, 10, 5, 8, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2007, 11, 5, 17, 0, 0, DateTimeKind.Utc);
            _dateTimePeriod = new DateTimePeriod(_start, _end);

            _absenceLayer = new AbsenceLayer(_abs, _dateTimePeriod, false);
            _personAbsence = new PersonAbsence(new Person(), new Scenario("Test"), _absenceLayer);
            _target = new AgentAbsenceDisplay(_personAbsence, _temp);

            Assert.AreEqual(DisplayMode.EndsToday, _target.DisplayMode);
        }
    }
}
