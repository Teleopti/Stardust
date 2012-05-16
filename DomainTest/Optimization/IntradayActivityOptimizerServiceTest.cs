using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class IntradayActivityOptimizerServiceTest
    {
        private IIntradayActivityOptimizerService _target;
        private MockRepository _mocks;
        private IScheduleDayService _scheduleDayService;
        private IScheduleDay _scheduleDay;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDay = _mocks.DynamicMock<IScheduleDay>();
            _scheduleDayService = _mocks.DynamicMock<IScheduleDayService>();
            _target = new IntradayActivityOptimizerService(_scheduleDayService);
        }

        [Test]
        public void VerifyOptimize()
        {
            using (_mocks.Record())
            {

            }

            using(_mocks.Playback())
            {
                Assert.IsFalse(_target.Optimize(_scheduleDay, new SchedulingOptions()));
            }
           
        }
    }
}