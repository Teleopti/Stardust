using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.Denormalize;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class PersonPeriodChangedMessageTest
    {
        private PersonPeriodChangedMessage _target;
        private Guid _tempGuid;
        [SetUp]
        public void Setup()
        {
            _tempGuid = Guid.NewGuid();
            _target = new PersonPeriodChangedMessage();
            _target.SetPersonIdCollection( new Guid[] { _tempGuid });

        }

        [Test]
        public void PropertiesShouldWork()
        {
            _target.Identity .Should().Not.Be.EqualTo(Guid.Empty);
            _target.PersonIdCollection  .First().Should().Be.EqualTo(_tempGuid);

            _target.SerializedPersonPeriod   = "Test1";
            _target.SerializedPersonPeriod.Should().Be.EqualTo("Test1");


        }
    }
}

