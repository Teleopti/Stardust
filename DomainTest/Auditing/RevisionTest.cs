﻿using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Auditing
{
    [TestFixture]
    public class RevisionTest
    {
        private Revision _target;

        [SetUp]
        public void Setup()
        {
            _target = new Revision { Id = 100 };
        }

        [Test]
        public void ShouldSetRevisionData()
        {
            var person = PersonFactory.CreatePerson("person1");
            var dateTime = DateTime.UtcNow;
            _target.SetRevisionData(person, dateTime);

            Assert.That(person, Is.EqualTo(_target.ModifiedBy));
            Assert.That(_target.ModifiedAt, Is.EqualTo(dateTime));
        }

        [Test]
        public void ShouldGetHashCode()
        {
            Assert.That(_target.GetHashCode(), Is.EqualTo(100.GetHashCode()));
        }

        [Test]
        public void ShouldCheckEquality()
        {
            var revision = new Revision { Id = 100 };
            Assert.That(_target.Equals(revision), Is.True);
        }
    }
}
