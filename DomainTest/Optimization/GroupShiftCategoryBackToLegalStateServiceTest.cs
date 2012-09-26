using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    public class GroupShiftCategoryBackToLegalStateServiceTest
    {
        private GroupShiftCategoryBackToLegalStateService _target;
        private MockRepository _mockRepository;
        private IRemoveShiftCategoryBackToLegalService _shiftCategoryBackToLegalService;
        private IGroupSchedulingService _scheduleService;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _schedulePart;
        private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _shiftCategoryBackToLegalService =
                _mockRepository.StrictMock<IRemoveShiftCategoryBackToLegalService>();
            _scheduleService = _mockRepository.StrictMock<IGroupSchedulingService>();
            _target = new GroupShiftCategoryBackToLegalStateService(_shiftCategoryBackToLegalService, _scheduleService);
            _scheduleDayPro = _mockRepository.StrictMock<IScheduleDayPro>();
            _schedulePart = _mockRepository.StrictMock<IScheduleDay>();
            _schedulingOptions = new SchedulingOptions();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyExecute()
        {
           
        }
    }
}
