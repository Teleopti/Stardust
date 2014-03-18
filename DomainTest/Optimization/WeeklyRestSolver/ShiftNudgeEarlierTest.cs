using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class ShiftNudgeEarlierTest
    {
        private ShiftNudgeEarlier _target;
        private MockRepository _mock;
        private IScheduleDay _scheduleDay;
        private IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;

        [SetUp]
        public void setUp()
        {
            
            _mock = new MockRepository();
            _deleteAndResourceCalculateService = _mock.StrictMock<IDeleteAndResourceCalculateService>();
            _target = new ShiftNudgeEarlier(_deleteAndResourceCalculateService);
        }

        [Test]
        public void ShouldNudge()
        {
            bool result = _target.Nudge(_scheduleDay);
            Assert.IsTrue(result);
        }

    }
}
