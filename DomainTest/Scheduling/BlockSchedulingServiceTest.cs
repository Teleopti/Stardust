using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class BlockSchedulingServiceTest
    {
        private IBlockSchedulingService _interface;
        private BlockSchedulingService _instance;
        private MockRepository _mocks;
        private IBlockFinder _blockFinder;
        private IBestBlockShiftCategoryFinder _blockShiftCategoryFinder;
        private IList<IScheduleMatrixPro> _matrixList;
        private IScheduleMatrixPro _matrix0;
        private IScheduleDayService _scheduleDayService;
        private IPerson _person;
        private IBlockFinderFactory _blockFinderFactory;
        private readonly IDictionary<string, IWorkShiftFinderResult> _reportList = new Dictionary<string, IWorkShiftFinderResult>();
        private IList<DateOnly> _dateOnlyList;
        private IBlockFinderResult _result;

    	private IScheduleDictionary _dictionary;
    	private IScheduleRange _range;
    	private IFairnessValueResult _fairness;
        private bool _eventFired;
    	private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mocks=new MockRepository();
            _blockFinder = _mocks.StrictMock<IBlockFinder>();
            _blockShiftCategoryFinder = _mocks.StrictMock<IBestBlockShiftCategoryFinder>();
            _matrix0 = _mocks.StrictMock<IScheduleMatrixPro>();
            _matrixList = new List<IScheduleMatrixPro>{ _matrix0 };
            _scheduleDayService = _mocks.StrictMock<IScheduleDayService>();
            _person = PersonFactory.CreatePerson();
            _blockFinderFactory = _mocks.StrictMock<IBlockFinderFactory>();
            _dateOnlyList = new List<DateOnly> {new DateOnly(2010, 1, 1)};
            _result = new BlockFinderResult(null, _dateOnlyList, new Dictionary<string, IWorkShiftFinderResult>());
        	_dictionary = _mocks.StrictMock<IScheduleDictionary>();
        	_range = _mocks.StrictMock<IScheduleRange>();
			_fairness = new FairnessValueResult { FairnessPoints = 5, TotalNumberOfShifts = 20 };
			_schedulingOptions = new SchedulingOptions();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyExecute()
        {
            _interface = new BlockSchedulingService(_blockShiftCategoryFinder, _scheduleDayService, _blockFinderFactory);

            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
			_schedulingOptions.UseBlockScheduling = BlockFinderType.SchedulePeriod;

            using (_mocks.Record())
            {
				Expect.Call(_blockFinderFactory.CreateFinder(_matrix0, _schedulingOptions.UseBlockScheduling)).Return(_blockFinder).Repeat.Any();
                Expect.Call(_blockFinder.ScheduleMatrix).Return(_matrix0).Repeat.Any();
                Expect.Call(_blockFinder.NextBlock()).Return(new BlockFinderResult(null, new List<DateOnly> { new DateOnly(2010, 1, 1) }, _reportList)).Repeat.
                    Twice();
                Expect.Call(_matrix0.GetScheduleDayByKey(new DateOnly(2010, 1, 1))).Return(scheduleDayPro).Repeat.Any();
				Expect.Call(_blockShiftCategoryFinder.ScheduleDictionary).Return(_dictionary);
            	Expect.Call(_dictionary.FairnessPoints()).Return(_fairness);
            	Expect.Call(_dictionary[_person]).Return(_range);
            	Expect.Call(_range.FairnessPoints()).Return(_fairness);
                Expect.Call(
                    _blockShiftCategoryFinder.BestShiftCategoryForDays(_result,
																	   _person, _fairness, _fairness, _schedulingOptions)).Return(
					new BestShiftCategoryResult(ShiftCategoryFactory.CreateShiftCategory("xx"),FailureCause.NoFailure)).Repeat.Any();
                    Expect.Call(scheduleDayPro.DaySchedulePart()).Return(schedulePart).Repeat.Any();
                    Expect.Call(_blockFinder.NextBlock()).Return(new BlockFinderResult(null, new List<DateOnly>(), _reportList)).Repeat.
                    Twice();
                Expect.Call(_matrix0.EffectivePeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>())).Repeat.Any();
                Expect.Call(_matrix0.UnlockedDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {scheduleDayPro})).Repeat.Any();
                Expect.Call(_matrix0.Person).Return(_person).Repeat.Any();
                Expect.Call(_scheduleDayService.ScheduleDay(schedulePart, _schedulingOptions)).Return(true).Repeat.Any();
                _blockFinder.ResetBlockPointer();
                LastCall.Repeat.Any();
            }
            _interface.Execute(_matrixList, _schedulingOptions, _reportList);
        }

        [Test]
        public void VerifyBlockFinderList()
        {
            _instance = new BlockSchedulingService(_blockShiftCategoryFinder, _scheduleDayService, _blockFinderFactory);
            IList<IBlockFinder> result = _instance.CreateFinders(_matrixList, BlockFinderType.BetweenDayOff);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void VerifyCorrectTypeIsCreated()
        {
            IBlockFinderFactory factory = new BlockFinderFactory();

            IBlockFinder finder = factory.CreateFinder(_matrix0, BlockFinderType.BetweenDayOff);
            Assert.IsTrue(typeof(BetweenDayOffBlockFinder) == finder.GetType());

            finder = factory.CreateFinder(_matrix0, BlockFinderType.SchedulePeriod);
            Assert.IsTrue(typeof(SchedulePeriodBlockFinder) == finder.GetType());

        }

        [Test]
        public void VerifyActOnResultReturnsWhenNoBlock()
        {
            _instance = new BlockSchedulingService(_blockShiftCategoryFinder, _scheduleDayService, _blockFinderFactory);
            IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly>(), _reportList);
			_instance.ActOnResult(result, null, _schedulingOptions);
        }

        [Test]
        public void VerifyActOnResultFindsBestCategoryIfResultCategoryIsNull()
        {
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
        	
            using(_mocks.Record())
            {
				Expect.Call(_blockShiftCategoryFinder.ScheduleDictionary).Return(_dictionary);
				Expect.Call(_dictionary.FairnessPoints()).Return(_fairness);
				Expect.Call(_dictionary[_person]).Return(_range);
				Expect.Call(_range.FairnessPoints()).Return(_fairness);
                Expect.Call(
					_blockShiftCategoryFinder.BestShiftCategoryForDays(_result, _person, _fairness, _fairness, _schedulingOptions)).Return(
					new BestShiftCategoryResult(ShiftCategoryFactory.CreateShiftCategory("xx"), FailureCause.NoFailure)).IgnoreArguments().Repeat.Once();
                Expect.Call(_matrix0.Person).Return(_person).Repeat.Any();
                Expect.Call(_matrix0.GetScheduleDayByKey(new DateOnly(2010, 1, 1))).Return(scheduleDayPro).Repeat.Any();
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(schedulePart).Repeat.Any();
                Expect.Call(_matrix0.UnlockedDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDayPro })).Repeat.Any();
				Expect.Call(_scheduleDayService.ScheduleDay(schedulePart, _schedulingOptions)).Return(true).Repeat.Once();
            }
            _instance = new BlockSchedulingService(_blockShiftCategoryFinder, _scheduleDayService, _blockFinderFactory);
            IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly> { new DateOnly(2010, 1, 1) }, _reportList);

            using(_mocks.Playback())
            {
				_instance.ActOnResult(result, _matrix0, _schedulingOptions);
            }
            
        }

        [Test]
        public void VerifyReturnIfNoShiftCategoryFound()
        {
            using (_mocks.Record())
            {
				Expect.Call(_blockShiftCategoryFinder.ScheduleDictionary).Return(_dictionary);
				Expect.Call(_dictionary.FairnessPoints()).Return(_fairness);
				Expect.Call(_dictionary[_person]).Return(_range);
				Expect.Call(_range.FairnessPoints()).Return(_fairness);
            	Expect.Call(
            		_blockShiftCategoryFinder.BestShiftCategoryForDays(null, null, _fairness, _fairness, _schedulingOptions)).
            		IgnoreArguments().Return(new BestShiftCategoryResult(null, FailureCause.AlreadyAssigned)).Repeat.Once();
                Expect.Call(_matrix0.Person).Return(_person).Repeat.Any();
            }
            _instance = new BlockSchedulingService(_blockShiftCategoryFinder, _scheduleDayService, _blockFinderFactory);
            IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly> { new DateOnly(2010, 1, 1) }, _reportList);

			_instance.ActOnResult(result, _matrix0, _schedulingOptions);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyActOnResultTryAnotherCategoryIfFirstFails()
        {
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();

            using (_mocks.Record())
            {
				Expect.Call(_blockShiftCategoryFinder.ScheduleDictionary).Return(_dictionary);
				Expect.Call(_dictionary.FairnessPoints()).Return(_fairness).Repeat.Twice();
				Expect.Call(_dictionary[_person]).Return(_range).Repeat.Twice();
				Expect.Call(_range.FairnessPoints()).Return(_fairness).Repeat.Twice();
                Expect.Call(
                    _blockShiftCategoryFinder.BestShiftCategoryForDays(_result,
																	   _person, _fairness, _fairness, _schedulingOptions)).Return(
					new BestShiftCategoryResult(ShiftCategoryFactory.CreateShiftCategory("xx"), FailureCause.NoFailure)).IgnoreArguments().Repeat.Once();
                Expect.Call(
                    _blockShiftCategoryFinder.BestShiftCategoryForDays(_result,
																	   _person, _fairness, _fairness, _schedulingOptions)).Return(
					new BestShiftCategoryResult(ShiftCategoryFactory.CreateShiftCategory("yy"), FailureCause.NoFailure)).IgnoreArguments().Repeat.Once();
                Expect.Call(_matrix0.Person).Return(_person).Repeat.Any();
                Expect.Call(_matrix0.GetScheduleDayByKey(new DateOnly(2010, 1, 1))).Return(scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(schedulePart).Repeat.Any();
                Expect.Call(_matrix0.UnlockedDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDayPro })).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayService.ScheduleDay(schedulePart, _schedulingOptions)).Return(false).Repeat.Once();
                //Expect.Call(_scheduleDayService.DeleteMainShift(new List<IScheduleDay>())).Return(new List<IScheduleDay>())
                //    .Repeat.Once();
				Expect.Call(_scheduleDayService.ScheduleDay(schedulePart, _schedulingOptions)).Return(true).Repeat.Once();
            }
            _instance = new BlockSchedulingService(_blockShiftCategoryFinder, _scheduleDayService, _blockFinderFactory);
            IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly> { new DateOnly(2010, 1, 1) }, _reportList);

            using (_mocks.Playback())
            {
				_instance.ActOnResult(result, _matrix0, _schedulingOptions);
            }

            Assert.AreEqual("xx", _schedulingOptions.NotAllowedShiftCategories[0].Description.Name);
        }

        [Test]
        public void VerifyLockedDaysDoesNotGetScheduled()
        {
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();

            using (_mocks.Record())
            {
                Expect.Call(_matrix0.Person).Return(_person).Repeat.Any();
                Expect.Call(_matrix0.GetScheduleDayByKey(new DateOnly(2010, 1, 1))).Return(scheduleDayPro).Repeat.Any();
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(schedulePart).Repeat.Any();
                Expect.Call(_matrix0.UnlockedDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>())).Repeat.Any();
				Expect.Call(_scheduleDayService.ScheduleDay(schedulePart, _schedulingOptions)).Repeat.Never();
            }
            _instance = new BlockSchedulingService(_blockShiftCategoryFinder, _scheduleDayService, _blockFinderFactory);
            IBlockFinderResult result = new BlockFinderResult(ShiftCategoryFactory.CreateShiftCategory("xx"), new List<DateOnly> { new DateOnly(2010, 1, 1) }, _reportList);

            using (_mocks.Playback())
            {
				_instance.ActOnResult(result, _matrix0, _schedulingOptions);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Test]
        public void VerifyBlockScheduledEvent()
        {
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
			_schedulingOptions.UseBlockScheduling = BlockFinderType.SchedulePeriod;

            using (_mocks.Record())
            {
                Expect.Call(_blockFinder.NextBlock()).Return(new BlockFinderResult(null, new List<DateOnly> { new DateOnly(2010, 1, 1) }, _reportList)).Repeat.
                    Once();
                Expect.Call(_blockFinder.NextBlock()).Return(new BlockFinderResult(null, new List<DateOnly>(), _reportList)).Repeat.
                    Once();
                _blockFinder.ResetBlockPointer();
                LastCall.Repeat.Any();
				Expect.Call(_blockFinderFactory.CreateFinder(_matrix0, _schedulingOptions.UseBlockScheduling)).Return(_blockFinder).Repeat.Any();
                Expect.Call(_blockFinder.ScheduleMatrix).Return(_matrix0).Repeat.Any();
                Expect.Call(_blockFinder.NextBlock()).Return(new BlockFinderResult(null, new List<DateOnly> { new DateOnly(2010, 1, 1) }, _reportList)).Repeat.
                    Once();
                Expect.Call(_matrix0.GetScheduleDayByKey(new DateOnly(2010, 1, 1))).Return(scheduleDayPro).Repeat.Any();

				Expect.Call(_blockShiftCategoryFinder.ScheduleDictionary).Return(_dictionary);
				Expect.Call(_dictionary.FairnessPoints()).Return(_fairness);
				Expect.Call(_dictionary[_person]).Return(_range);
				Expect.Call(_range.FairnessPoints()).Return(_fairness);

				Expect.Call(
					_blockShiftCategoryFinder.BestShiftCategoryForDays(_result, _person, _fairness, _fairness, _schedulingOptions)).
					Return(new BestShiftCategoryResult(ShiftCategoryFactory.CreateShiftCategory("xx"), FailureCause.NoFailure)).IgnoreArguments().Repeat.Any();
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(schedulePart).Repeat.Any();
                Expect.Call(_blockFinder.NextBlock()).Return(new BlockFinderResult(null, new List<DateOnly>(), _reportList)).Repeat.
                    Once();
                Expect.Call(_matrix0.EffectivePeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>())).Repeat.Any();
                Expect.Call(_matrix0.UnlockedDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDayPro })).Repeat.Any();
                Expect.Call(_matrix0.Person).Return(_person).Repeat.Any();
				Expect.Call(_scheduleDayService.ScheduleDay(schedulePart, _schedulingOptions)).Return(true).Repeat.Any();
                
            }


            using(_mocks.Playback())
            {
                _interface = new BlockSchedulingService(_blockShiftCategoryFinder, _scheduleDayService, _blockFinderFactory);
                _interface.BlockScheduled += interfaceBlockScheduled;
				_interface.Execute(_matrixList, _schedulingOptions, _reportList);
                _interface.BlockScheduled -= interfaceBlockScheduled; 
            }
            
            Assert.IsTrue(_eventFired);
            
        }

        [Test]
        public void VerifyTryScheduleBlockReturnsTrueIfBlockSuccessfullyScheduled()
        {
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var unlockedDays = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>{scheduleDayPro});
            using (_mocks.Record())
            {
                Expect.Call(_matrix0.GetScheduleDayByKey(new DateOnly(2010, 1, 1))).Return(scheduleDayPro).Repeat.Once();
                Expect.Call(_matrix0.UnlockedDays).Return(unlockedDays).Repeat.Once();
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(schedulePart).Repeat.Once();
				Expect.Call(_scheduleDayService.ScheduleDay(schedulePart, _schedulingOptions)).Return(true).Repeat.Once();
            }
            _instance = new BlockSchedulingService(_blockShiftCategoryFinder, _scheduleDayService, _blockFinderFactory);
            IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly> { new DateOnly(2010, 1, 1) }, _reportList);

			Assert.IsTrue(_instance.TryScheduleBlock(result, _matrix0, _schedulingOptions));
        }

        [Test]
        public void VerifyTryScheduleBlockReturnsFalseIfNotBlockSuccessfullyScheduled()
        {
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            var schedulePart = _mocks.StrictMock<IScheduleDay>();
            var schedulePartToFail = _mocks.StrictMock<IScheduleDay>();
            var unlockedDays = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDayPro, scheduleDayPro1 });
            using (_mocks.Record())
            {
                Expect.Call(_matrix0.GetScheduleDayByKey(new DateOnly(2010, 1, 1))).Return(scheduleDayPro).Repeat.Once();
                Expect.Call(_matrix0.GetScheduleDayByKey(new DateOnly(2010, 1, 2))).Return(scheduleDayPro1).Repeat.Once();
                Expect.Call(_matrix0.UnlockedDays).Return(unlockedDays).Repeat.Twice();
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(schedulePart).Repeat.Once();
				Expect.Call(_scheduleDayService.ScheduleDay(schedulePart, _schedulingOptions)).Return(true).Repeat.Once();
                Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(schedulePartToFail).Repeat.Once();
				Expect.Call(_scheduleDayService.ScheduleDay(schedulePartToFail, _schedulingOptions)).Return(false).Repeat.Once();
				Expect.Call(_scheduleDayService.DeleteMainShift(new List<IScheduleDay> { schedulePart }, _schedulingOptions)).Return(
                    new List<IScheduleDay> {schedulePart}).Repeat.Once();
            }
            _instance = new BlockSchedulingService(_blockShiftCategoryFinder, _scheduleDayService, _blockFinderFactory);
            IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly> { new DateOnly(2010, 1, 1), new DateOnly(2010, 1, 2) }, _reportList);

            using(_mocks.Playback())
            {
				Assert.IsFalse(_instance.TryScheduleBlock(result, _matrix0, _schedulingOptions));
            }
            
        }


        void interfaceBlockScheduled(object sender, System.EventArgs e)
        {
            _eventFired = true;
        }

    }
}
