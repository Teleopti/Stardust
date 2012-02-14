using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
    public class MeetingPersonTest
    {
        MeetingPerson _target;

        [SetUp]
        public void Setup()
        {
            _target = new MeetingPerson(new Person(), true);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target.Person);
            Assert.IsTrue(_target.Optional);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));    
        }

        [Test]
        public void VerifyProperties()
        {
            _target.Person = new Person();
            _target.Optional = false;

            Assert.IsNotNull(_target.Person);
            Assert.IsFalse(_target.Optional);
        }

        [Test]
        public void VerifyClone()
        {
            MeetingPerson clone = (MeetingPerson)_target.Clone();
           
            Assert.AreEqual(_target.Person, clone.Person);
        }
    }
}