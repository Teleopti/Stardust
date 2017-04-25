using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ProTest")]
    [TestFixture]
    public class SchedulePeriodBackToLegalStateServiceProTest
    {
        private SchedulePeriodShiftCategoryBackToLegalStateService _target;
        private MockRepository _mockRepository;
        private IRemoveShiftCategoryBackToLegalService _shiftCategoryBackToLegalService;
        private IScheduleDayService _scheduleDayService;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _schedulePart;
    	private SchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _shiftCategoryBackToLegalService = 
                _mockRepository.StrictMock<IRemoveShiftCategoryBackToLegalService>();
            _scheduleDayService = _mockRepository.StrictMock<IScheduleDayService>();
            _target = new SchedulePeriodShiftCategoryBackToLegalStateService(_shiftCategoryBackToLegalService, _scheduleDayService);
            _scheduleDayPro = _mockRepository.StrictMock<IScheduleDayPro>();
            _schedulePart = _mockRepository.StrictMock<IScheduleDay>();
			_schedulingOptions = new SchedulingOptions();

        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        /// <summary>
        /// Expects that for each ShiftCategoryLimitation in the collection the service
        /// creates a RemoveShiftCategoryBackToLegalState service and runs its execute method.
        /// It is checked by the mocked RemoveShiftCategoryBackToLegalStateFactory
        /// </summary>
        [Test]
        public void VerifyExecute()
        {
            //ISchedulePeriod schedulePeriod = AddTestShiftCategoryLimitationToSchedulePeriod();
            IVirtualSchedulePeriod virtualSchedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();
            using(_mockRepository.Record())
            {
				Expect.Call(_shiftCategoryBackToLegalService.Execute(null, _schedulingOptions))
                    .Return(new List<IScheduleDayPro> { _scheduleDayPro, _scheduleDayPro, _scheduleDayPro })
                    .IgnoreArguments()
                    .Repeat.Times(2);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.Times(6);
				Expect.Call(_scheduleDayService.ScheduleDay(_schedulePart, _schedulingOptions)).Return(true).Repeat.Times(6);
                Expect.Call(virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(Limitations());
            }
            using(_mockRepository.Playback())
            {
                //AddTestShiftCategoryLimitationToSchedulePeriod();
				Assert.IsTrue(_target.Execute(virtualSchedulePeriod, _schedulingOptions));
            }
        }

        //private static ISchedulePeriod AddTestShiftCategoryLimitationToSchedulePeriod()
        //{
        //    DateOnly dateOnly = new DateOnly(2010, 1, 8);
        //    ISchedulePeriod schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(dateOnly);
        //    IShiftCategory shiftCategoryA = ShiftCategoryFactory.CreateShiftCategory("CategoryA");
        //    IShiftCategoryLimitation limitationA = new ShiftCategoryLimitation(shiftCategoryA);
        //    limitationA.Weekly = false;
        //    IShiftCategory shiftCategoryB = ShiftCategoryFactory.CreateShiftCategory("CategoryB");
        //    IShiftCategoryLimitation limitationB = new ShiftCategoryLimitation(shiftCategoryB);
        //    limitationB.Weekly = false;
        //    schedulePeriod.AddShiftCategoryLimitation(limitationA);
        //    schedulePeriod.AddShiftCategoryLimitation(limitationB);
        //    return schedulePeriod;
        //}

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
