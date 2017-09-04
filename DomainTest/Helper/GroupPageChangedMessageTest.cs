using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.Denormalize;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class GroupPageChangedMessageTest
    {
        private GroupPageChangedMessage _target;
        private Guid _tempGuid;
        [SetUp]
        public void Setup()
        {
            _tempGuid = Guid.NewGuid();
            _target = new GroupPageChangedMessage();
            _target.SetGroupPageIdCollection( new Guid[] { _tempGuid });

        }

        [Test]
        public void PropertiesShouldWork()
        {
            _target.Identity .Should().Not.Be.EqualTo(Guid.Empty);
            _target.GroupPageIdCollection .First().Should().Be.EqualTo(_tempGuid);

            _target.SerializedGroupPage  = "Test1";
            _target.SerializedGroupPage.Should().Be.EqualTo("Test1");


        }
    }
}
