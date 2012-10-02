using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
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
        private IGroupOptimizerFindMatrixesForGroup _groupOptimerFindMatrixesForGroup;

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
            _groupOptimerFindMatrixesForGroup = _mockRepository.DynamicMock<IGroupOptimizerFindMatrixesForGroup>();
        }

        [Test]
        public void VerifyExecute()
        {
            var virtualSchedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();
            var scheduleMatrix = _mockRepository.DynamicMock<IScheduleMatrixPro>();
            var scheduleMatrixList = new List<IScheduleMatrixPro>();
            scheduleMatrixList.Add((scheduleMatrix));
            var person = new Person() ;
            var dateOnly = new DateOnly();
            var groupPerson = _mockRepository.DynamicMock<IGroupPerson>();
            var dateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, new CccTimeZoneInfo());

            using (_mockRepository.Record())
            {
                Expect.Call(_shiftCategoryBackToLegalService.Execute(null, _schedulingOptions))
                    .Return(new List<IScheduleDayPro> { _scheduleDayPro, _scheduleDayPro, _scheduleDayPro })
                    .IgnoreArguments()
                    .Repeat.Times(2);
                Expect.Call(virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(Limitations());
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(dateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(_groupOptimerFindMatrixesForGroup.Find(person, dateOnly)).IgnoreArguments().Return(scheduleMatrixList).Repeat.AtLeastOnce();
                Expect.Call(scheduleMatrix.Person).Return(person);
                Expect.Call(scheduleMatrix.SchedulePeriod).Return(virtualSchedulePeriod);
                Expect.Call(virtualSchedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateOnly,dateOnly.AddDays(1))).Repeat.AtLeastOnce() ;
                Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, dateOnly)).Return(groupPerson ).Repeat.AtLeastOnce();
                Expect.Call(_scheduleService.ScheduleOneDay(dateOnly, _schedulingOptions,groupPerson,scheduleMatrixList )).Return(true).Repeat.Times(6);

            }
            using (_mockRepository.Playback())
            {
                Assert.AreEqual( 1, _target.Execute(virtualSchedulePeriod, _schedulingOptions, scheduleMatrixList, _groupOptimerFindMatrixesForGroup).Count );
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
