using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.RealTimeAdherence;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class DefaultStateGroupExceptionTest
    {
        private DefaultStateGroupException _target;

        [SetUp]
        public void Setup()
        {
            _target = new DefaultStateGroupException();
        }
        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
            _target = new DefaultStateGroupException("Fel!");
            Assert.IsNotNull(_target);
            _target = new DefaultStateGroupException("Fel igen!", new ArgumentException("Nu är det fel!"));
            Assert.IsNotNull(_target);
        }
    }
}
