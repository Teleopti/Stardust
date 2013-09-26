using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class ModifyEventArgsTest
    {
        ModifyEventArgs _target;
        DateTimePeriod _rangePeriod;
        Person _person;

        [SetUp]
        public void Setup()
        {
            _rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);
            _person = new Person();
            _target = new ModifyEventArgs(ScheduleModifier.Request, _person,_rangePeriod, null);
        }

        [Test]
        public void CanGetProperties()
        {
            Assert.AreEqual(_person, _target.ModifiedPerson);
            Assert.AreEqual(_rangePeriod, _target.ModifiedPeriod);
            Assert.AreEqual(ScheduleModifier.Request, _target.Modifier);
        }
    }
}
