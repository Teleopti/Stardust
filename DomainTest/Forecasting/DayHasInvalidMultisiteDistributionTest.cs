using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class DayHasInvalidMultisiteDistributionTest
    {
        private DayHasInvalidMultisiteDistribution _target;
        private MockRepository mocks;

        [Test]
        public void Setup()
        {
            mocks = new MockRepository();
            _target = new DayHasInvalidMultisiteDistribution();
        }

        [Test]
        public void VerifyHandlesValidItem()
        {
            IMultisiteDay multisiteDay = mocks.StrictMock<IMultisiteDay>();
            IMultisitePeriod multisitePeriod = mocks.StrictMock<IMultisitePeriod>();

            Expect.Call(multisiteDay.MultisitePeriodCollection).Return(
                new ReadOnlyCollection<IMultisitePeriod>(new List<IMultisitePeriod> {multisitePeriod}));
            Expect.Call(multisitePeriod.IsValid).Return(true);

            mocks.ReplayAll();
            Assert.IsFalse(_target.IsSatisfiedBy(multisiteDay));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyHandlesInvalidItem()
        {
            IMultisiteDay multisiteDay = mocks.StrictMock<IMultisiteDay>();
            IMultisitePeriod multisitePeriod = mocks.StrictMock<IMultisitePeriod>();

            Expect.Call(multisiteDay.MultisitePeriodCollection).Return(
                new ReadOnlyCollection<IMultisitePeriod>(new List<IMultisitePeriod> { multisitePeriod }));
            Expect.Call(multisitePeriod.IsValid).Return(false);

            mocks.ReplayAll();
            Assert.IsTrue(_target.IsSatisfiedBy(multisiteDay));
            mocks.VerifyAll();
        }
    }
}
