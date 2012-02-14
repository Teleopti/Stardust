using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class ClipboardAssignmentTest
    {
        private ClipboardAssignment _target;

        [SetUp]
        public void Setup()
        {
            _target = new ClipboardAssignment();
        }

        [Test]
        public void CanAddAgentAbsences()
        {
            
            PersonAbsence personAbs = new PersonAbsence(new Person(), 
                                            new Scenario("Test"), 
                                            new AbsenceLayer(new Absence(), new DateTimePeriod(2000,1,1,2002,1,1),false));
            _target.AgentAbsenceList.Add(personAbs);
            Assert.IsTrue(_target.AgentAbsenceList.Contains(personAbs));
        }
        [Test]
        public void CanAddAgentAssignment()
        {
            PersonAssignment personAss = new PersonAssignment(new Person(), new Scenario("Test"));
            _target.AgentAssignmentList.Add(personAss);
            Assert.IsTrue(_target.AgentAssignmentList.Contains(personAss));
        }
        [Test]
        public void CanAddAgentDayOff()
        {
            PersonDayOff personDO = new PersonDayOff(new Person(), new Scenario("Test"), new Teleopti.Ccc.Domain.Time.AnchorDateTimePeriod());
            _target.PersonDayOff = personDO;
            Assert.AreEqual(_target.PersonDayOff, personDO);
        }

        [Test]
        public void CanAddPerson()
        {
            Person person = new Person();
            _target.Agent = person;
            Assert.AreEqual(_target.Agent, person);
        }

        [Test]
        public void CanAddDate()
        {
            _target.ScheduledDate = new DateTime(2000, 1, 1);
            Assert.AreEqual(new DateTime(2000,1,1),_target.ScheduledDate);
        }

    }
}
