using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ScheduleMatrixBackToLegalStateBrutalForceServiceContainerTest
    {
        private IScheduleMatrixBackToLegalStateBrutalForceServiceContainer _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private ISchedulePeriodDayOffBackToLegalStateByBrutalForceService _service;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _service = _mocks.StrictMock<ISchedulePeriodDayOffBackToLegalStateByBrutalForceService>();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _target = new ScheduleMatrixBackToLegalStateBrutalForceServiceContainer(_service, _matrix);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreSame(_service, _target.Service);
            Assert.AreSame(_matrix, _target.ScheduleMatrix);
        }
    }
}