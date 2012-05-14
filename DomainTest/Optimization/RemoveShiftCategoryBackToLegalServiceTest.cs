using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class RemoveShiftCategoryBackToLegalServiceTest
    {
        private MockRepository _mocks;
        private IRemoveShiftCategoryBackToLegalService _interface;
        private RemoveShiftCategoryBackToLegalService _target;
        private IScheduleMatrixPro _scheduleMatrixMock;
        private IRemoveShiftCategoryOnBestDateService _removeOnBestDateForTest;
        private IShiftCategoryLimitation _shiftCategoryLimitation;
        private IShiftCategory _shiftCategory;
        private IScheduleDayPro _scheduleDayPro;
        private IList<IScheduleDayPro> _extendedPeriodDays;
        private IList<IScheduleDayPro> _periodDays;
        private IList<bool> _correctCategory;
    	private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleMatrixMock = _mocks.StrictMock<IScheduleMatrixPro>();
            _correctCategory = new List<bool>();
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _removeOnBestDateForTest = new RemoveOnBestDateServiceForTest(_correctCategory, _scheduleDayPro);
            _interface = new RemoveShiftCategoryBackToLegalService(_removeOnBestDateForTest, _scheduleMatrixMock);
            _target = new RemoveShiftCategoryBackToLegalService(_removeOnBestDateForTest, _scheduleMatrixMock);
            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("xx");
            _shiftCategory.SetId(Guid.NewGuid());
            _shiftCategoryLimitation = new ShiftCategoryLimitation(_shiftCategory);
            _extendedPeriodDays = new List<IScheduleDayPro>();
            _periodDays = new List<IScheduleDayPro>();
			_schedulingOptions = new SchedulingOptions();
            
            

            for (int i = 0; i < 21; i++)
            {
                IScheduleDayPro day = _mocks.StrictMock<IScheduleDayPro>();
                _extendedPeriodDays.Add(day);
                if(i > 3 && i < 18)
                    _periodDays.Add(day);
                if(i == 2 || i == 3 || i == 4 || i == 8 || i == 16 || i == 17)
                {
                    _correctCategory.Add(true);
                }
                else
                {
                    _correctCategory.Add(false);
                }
            }

            using (_mocks.Record())
            {
                mockExpectations();
            }

        }

        private void mockExpectations()
        {
            Expect.Call(_scheduleMatrixMock.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(_periodDays))
                    .Repeat.Any();
            Expect.Call(_scheduleMatrixMock.FullWeeksPeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(_extendedPeriodDays))
                    .Repeat.Any();
            for (int i = 0; i < 21; i++)
            {
                IScheduleDayPro day = _extendedPeriodDays[i];
                Expect.Call(day.Day).Return(new DateOnly(2009, 12, 28).AddDays(i)).Repeat.Any();
            }
        }

        [Test]
        public void VerifyExecute()
        {
            _shiftCategoryLimitation.Weekly = false;
            _shiftCategoryLimitation.MaxNumberOf = 10;
			IList<IScheduleDayPro> result = _interface.Execute(_shiftCategoryLimitation, _schedulingOptions);
            Assert.IsNotNull(result);
        }

        [Test]
        public void VerifyExecutePeriodWhenExecuteOneReturnsNullExits()
        {
            _removeOnBestDateForTest = new RemoveOnBestDateServiceForTest2(_correctCategory);
            _interface = new RemoveShiftCategoryBackToLegalService(_removeOnBestDateForTest, _scheduleMatrixMock);
            _shiftCategoryLimitation.Weekly = false;
            _shiftCategoryLimitation.MaxNumberOf = 2;
			IList<IScheduleDayPro> result = _interface.Execute(_shiftCategoryLimitation, _schedulingOptions);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void VerifyExecuteWeekWhenExecuteOneReturnsNullExits()
        {
            _removeOnBestDateForTest = new RemoveOnBestDateServiceForTest2(_correctCategory);
            _interface = new RemoveShiftCategoryBackToLegalService(_removeOnBestDateForTest, _scheduleMatrixMock);
            _shiftCategoryLimitation.Weekly = true;
            _shiftCategoryLimitation.MaxNumberOf = 2;
			IList<IScheduleDayPro> result = _interface.Execute(_shiftCategoryLimitation, _schedulingOptions);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void VerifyExecute1()
        {
            _shiftCategoryLimitation.Weekly = true;
            _shiftCategoryLimitation.MaxNumberOf = 7;
			IList<IScheduleDayPro> result = _interface.Execute(_shiftCategoryLimitation, _schedulingOptions);
            Assert.IsNotNull(result);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyIsShiftCategoryOverPeriodLimitThrowsWhenCalledWithWeek()
        {
            _shiftCategoryLimitation.Weekly = true;
            _shiftCategoryLimitation.MaxNumberOf = 3;

            using (_mocks.Playback())
            {
                _target.IsShiftCategoryOverPeriodLimit(_shiftCategoryLimitation);
            }
        }

        [Test]
        public void VerifyIsShiftCategoryOverPeriodLimit()
        {
            _shiftCategoryLimitation.Weekly = false;
            _shiftCategoryLimitation.MaxNumberOf = 3;
            bool result;

            using(_mocks.Playback())
            {
                result = _target.IsShiftCategoryOverPeriodLimit(_shiftCategoryLimitation);
            }
            
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyIsShiftCategoryOverPeriodLimit2()
        {
            _shiftCategoryLimitation.Weekly = false;
            _shiftCategoryLimitation.MaxNumberOf = 4;
            bool result;

            using (_mocks.Playback())
            {
                result = _target.IsShiftCategoryOverPeriodLimit(_shiftCategoryLimitation);
            }

            Assert.IsFalse(result);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyIsShiftCategoryOverWeekLimitThrowsWhenCalledWithPeriod()
        {
            _shiftCategoryLimitation.Weekly = false;
            _shiftCategoryLimitation.MaxNumberOf = 1;

            using (_mocks.Playback())
            {
                _target.IsShiftCategoryOverWeekLimit(_shiftCategoryLimitation);
            }
        }

        [Test]
        public void VerifyIsShiftCategoryOverWeekLimit()
        {
            _shiftCategoryLimitation.Weekly = true;
            _shiftCategoryLimitation.MaxNumberOf = 1;

            bool result;

            using (_mocks.Playback())
            {
                result = _target.IsShiftCategoryOverWeekLimit(_shiftCategoryLimitation);
            }

            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyIsShiftCategoryOverWeekLimit2()
        {
            _shiftCategoryLimitation.Weekly = true;
            _shiftCategoryLimitation.MaxNumberOf = 3;

            bool result;

            using (_mocks.Playback())
            {
                result = _target.IsShiftCategoryOverWeekLimit(_shiftCategoryLimitation);
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyExecutePeriod()
        {
            _shiftCategoryLimitation.Weekly = false;
            _shiftCategoryLimitation.MaxNumberOf = 3;

            IList<IScheduleDayPro> result;

            //_mocks.BackToRecordAll(BackToRecordOptions.All);
            //using (_mocks.Record())
            //{
            //    mockExpectations();
            //    //Expect.Call(_removeOnBestDateServiceMock.Execute(_shiftCategory)).Return(true).Repeat.Once();
            //    //_shiftCategoryLimitation.MaxNumberOf = 3;
            //    //Expect.Call(_removeOnBestDateServiceMock.Execute(_shiftCategory)).Return(false).Repeat.Once();
            //}

            using (_mocks.Playback())
            {
				result = _target.ExecutePeriod(_shiftCategoryLimitation, _schedulingOptions);
            }

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void VerifyExecuteWeeks()
        {
            _shiftCategoryLimitation.Weekly = true;
            _shiftCategoryLimitation.MaxNumberOf = 2;

            IList<IScheduleDayPro> result;

            using (_mocks.Playback())
            {
				result = _target.ExecuteWeeks(_shiftCategoryLimitation, _schedulingOptions);
            }

            Assert.AreEqual(1, result.Count);
        }
    }

    public class RemoveOnBestDateServiceForTest : IRemoveShiftCategoryOnBestDateService
    {
        private int _callCount;
        private IList<bool> _correctCategory;
        private IScheduleDayPro _dayToReturn;


        public RemoveOnBestDateServiceForTest(IList<bool> correctCategory, IScheduleDayPro dayToReturn)
        {
            _correctCategory = correctCategory;
            _dayToReturn = dayToReturn;
        }

		public IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory, ISchedulingOptions schedulingOptions)
        {
            _callCount++;
            return _dayToReturn;
        }

		public IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory, DateOnlyPeriod period, ISchedulingOptions schedulingOptions)
        {
            _callCount++;
            return _dayToReturn;
        }

    	public bool IsThisDayCorrectShiftCategory(IScheduleDayPro scheduleDayPro, IShiftCategory shiftCategory)
        {
            if(scheduleDayPro.Day == new DateOnly(2010, 1, 1) && _callCount > 0)
                return false;

            int index = (int) scheduleDayPro.Day.Date.Subtract(new DateOnly(2009, 12, 28).Date).TotalDays;
            return _correctCategory[index];
        }
    }

    public class RemoveOnBestDateServiceForTest2 : IRemoveShiftCategoryOnBestDateService
    {
        private int _callCount;
        private IList<bool> _correctCategory;


        public RemoveOnBestDateServiceForTest2(IList<bool> correctCategory)
        {
            _correctCategory = correctCategory;
        }

		public IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory, ISchedulingOptions schedulingOptions)
        {
            _callCount++;
            return null;
        }

		public IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory, DateOnlyPeriod period, ISchedulingOptions schedulingOptions)
        {
            _callCount++;
            return null;
        }

        public bool IsThisDayCorrectShiftCategory(IScheduleDayPro scheduleDayPro, IShiftCategory shiftCategory)
        {
            if (scheduleDayPro.Day == new DateOnly(2010, 1, 1) && _callCount > 0)
                return false;

            int index = (int)scheduleDayPro.Day.Date.Subtract(new DateOnly(2009, 12, 28).Date).TotalDays;
            return _correctCategory[index];
        }
    }
}