using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class AgentDateTest
    {
        private AgentDate _target;
        private Person _agent;
        private DateTime _dateTime;

        [SetUp]
        public void Setup()
        {
            _dateTime = new DateTime(2007, 11, 06);
            _agent = new Person();
            _target = new AgentDate(_agent, _dateTime);
        }

        [Test]
        public void CanCreateInstanceAndSetAndGetProperties()
        {
            _target = new AgentDate(_agent, _dateTime);
            Assert.IsNotNull(_target);
            Assert.AreSame(_agent, _target.Agent);
            Assert.AreEqual(_dateTime, _target.Date);
        }
        [Test]
        public void VerifyOverriddenToStringMethodWorks()
        {
            Name name = new Name("Urban", "Vångstedt");
            _target.Agent.Name = name;
            Assert.AreEqual("Urban Vångstedt " + _target.Date, _target.ToString());
        }

        [Test]
        public void VerifyEqualsWorks()
        {
            _dateTime = new DateTime(2007, 11, 06);
            _agent = new Person();
            _target = new AgentDate(_agent, _dateTime);

            //vAriant 1 - båda är fel
            Assert.IsFalse(_target.Equals(new AgentDate(new Person(), new DateTime(2007, 11, 11))));
           
            //Variant 2 - agent är fel
            Assert.IsFalse(_target.Equals(new AgentDate(new Person(), new DateTime(2007, 11, 06))));

            //Variant 3 - datum är fel
            Assert.IsFalse(_target.Equals(new AgentDate(_agent, new DateTime(2007, 11, 11))));

            //variant 4 - allt stämmer
            Assert.IsTrue(_target.Equals(new AgentDate(_agent, new DateTime(2007, 11, 06))));

            //Variant 5 - jämför med en icke agentDate
            Assert.IsFalse(_target.Equals(new object()));

            Assert.IsTrue(_target.Equals((object)_target));
        }

        [Test]
        public void VerifyOperatorEqualWorks()
        {
            AgentDate agentDate2 = _target;
            Assert.IsTrue(_target == agentDate2);
            agentDate2 = new AgentDate(new Person(), new DateTime());
            Assert.IsFalse(_target == agentDate2);
        }
        [Test]
        public void VerifyOperatorNotEqualWorks()
        {
            AgentDate agentDate2 = _target;
            Assert.IsFalse(_target != agentDate2);
            agentDate2 = new AgentDate(new Person(), new DateTime());
            Assert.IsTrue(_target != agentDate2);
        }
        [Test]
        public void VerifyGetHashCodeWorks()
        {
            Assert.AreEqual(_target.Date.GetHashCode() ^ _target.Agent.GetHashCode(), _target.GetHashCode());
        }
    }
}