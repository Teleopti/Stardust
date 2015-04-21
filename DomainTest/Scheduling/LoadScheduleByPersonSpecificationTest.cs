using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class LoadScheduleByPersonSpecificationTest
    {
        private LoadScheduleByPersonSpecification _target;
        private MockRepository _mocks;
        private ILoaderDeciderResult _decider;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _decider = _mocks.StrictMock<ILoaderDeciderResult>();

            _target = new LoadScheduleByPersonSpecification();
        }

        [Test]
        public void ShouldBeSatisfiedWhenDeciderFilteredOut70Percent()
        {
            Expect.Call(_decider.PercentageOfPeopleFiltered).Return(70d);
            _mocks.ReplayAll();

            Assert.IsTrue(_target.IsSatisfiedBy(_decider));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldBeSatisfiedWhenDeciderFilteredOutLessThan70Percent()
        {
            Expect.Call(_decider.PercentageOfPeopleFiltered).Return(0d);
            _mocks.ReplayAll();

            Assert.IsTrue(_target.IsSatisfiedBy(_decider));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldNotBeSatisfiedWhenDeciderFilteredOutMoreThan70Percent()
        {
            Expect.Call(_decider.PercentageOfPeopleFiltered).Return(85d);
            _mocks.ReplayAll();

            Assert.IsFalse(_target.IsSatisfiedBy(_decider));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldNotBeSatisfiedWhenDeciderIsNull()
        {
            Assert.IsFalse(_target.IsSatisfiedBy(null));
        }
    }
}
