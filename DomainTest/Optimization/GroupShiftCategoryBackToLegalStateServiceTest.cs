using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
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
        private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _shiftCategoryBackToLegalService =
                _mockRepository.StrictMock<IRemoveShiftCategoryBackToLegalService>();
            _scheduleService = _mockRepository.StrictMock<IGroupSchedulingService>();
            _groupPersonBuilderForOptimization = _mockRepository.StrictMock<IGroupPersonBuilderForOptimization>();
            _target = new GroupShiftCategoryBackToLegalStateService(_shiftCategoryBackToLegalService, _scheduleService, _groupPersonBuilderForOptimization);
            _scheduleDayPro = _mockRepository.StrictMock<IScheduleDayPro>();
            _schedulePart = _mockRepository.StrictMock<IScheduleDay>();
            _schedulingOptions = new SchedulingOptions();
        }

        [Test]
        public void VerifyExecute()
        {
            //ISchedulePeriod schedulePeriod = AddTestShiftCategoryLimitationToSchedulePeriod();
            IVirtualSchedulePeriod virtualSchedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();
            var scheduleMatrix = _mockRepository.DynamicMock<IScheduleMatrixPro>();
            var scheduleMatrixList = new List<IScheduleMatrixPro>();

            using (_mockRepository.Record())
            {
                Expect.Call(_shiftCategoryBackToLegalService.Execute(null, _schedulingOptions))
                    .Return(new List<IScheduleDayPro> { _scheduleDayPro, _scheduleDayPro, _scheduleDayPro })
                    .IgnoreArguments()
                    .Repeat.Times(2);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.Times(6);
                //Expect.Call(_scheduleDayService.ScheduleDay(_schedulePart, _schedulingOptions)).Return(true).Repeat.Times(6);
                Expect.Call(virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(Limitations());
            }
            using (_mockRepository.Playback())
            {
                //AddTestShiftCategoryLimitationToSchedulePeriod();
                //Assert.IsTrue(_target.Execute(virtualSchedulePeriod,_schedulingOptions,));
            }
        }

        private static ReadOnlyCollection<IShiftCategoryLimitation> Limitations()
        {
            IShiftCategory shiftCategoryA = ShiftCategoryFactory.CreateShiftCategory("CategoryA");
            IShiftCategoryLimitation limitationA = new ShiftCategoryLimitation(shiftCategoryA);
            limitationA.Weekly = false;
            IShiftCategory shiftCategoryB = ShiftCategoryFactory.CreateShiftCategory("CategoryB");
            IShiftCategoryLimitation limitationB = new ShiftCategoryLimitation(shiftCategoryB);
            limitationB.Weekly = false;

            Collection<IShiftCategoryLimitation> limits = new Collection<IShiftCategoryLimitation> { limitationA, limitationB };
            return new ReadOnlyCollection<IShiftCategoryLimitation>(limits);
        }
    }
}
