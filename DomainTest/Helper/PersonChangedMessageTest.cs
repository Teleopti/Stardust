using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class PersonChangedMessageTest
    {
        private PersonChangedMessage _target;
        private Guid _tempGuid;
        [SetUp]
        public void Setup()
        {
            _tempGuid = Guid.NewGuid();
            _target = new PersonChangedMessage();
            _target.SetPersonIdCollection(new Guid[] { _tempGuid });

        }

        [Test]
        public void PropertiesShouldWork()
        {
            _target.Identity.Should().Not.Be.EqualTo(Guid.Empty);
            _target.PersonIdCollection.First().Should().Be.EqualTo(_tempGuid );

            _target.SerializedPeople = "Test1";
            _target.SerializedPeople.Should().Be.EqualTo("Test1");


        }
    }
}
