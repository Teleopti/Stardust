using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.Fairness;

namespace Teleopti.Ccc.DomainTest.Optimization.Fairness
{
    public class PriortiseWeekDayTest
    {
        private MockRepository _mock;
        private IPriortiseWeekDay _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new PriortiseWeekDay();
        }

        [Test]
        public void TestThis()
        {
        }
    }
}