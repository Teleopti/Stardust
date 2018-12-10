using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
    /// <summary>
    /// Tests an absence layer
    /// </summary>
    [TestFixture]
    public class AbsenceLayerTest
    {
        private IAbsenceLayer target;

        /// <summary>
        /// Runs before each test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            DateTimePeriod period = new DateTimePeriod(2007, 8, 20, 2007, 08, 30);
            target = new AbsenceLayer(AbsenceFactory.CreateAbsence("Sommarlov"), period);
        }

        /// <summary>
        /// Verifies that constructor is working
        /// </summary>
        [Test]
        public void CanCreateNewInstance()
        {
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Can create an instance
        /// </summary>
        [Test]
        public void ProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        [Test]
        public void ShouldNotAcceptPeriodWithSeconds()
        {
			Assert.Throws<ArgumentException>(() => target = new AbsenceLayer(target.Payload, target.Period.MovePeriod(TimeSpan.FromSeconds(4))));
        }

    }
}