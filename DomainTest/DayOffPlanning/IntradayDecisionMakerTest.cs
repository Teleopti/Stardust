using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class IntradayDecisionMakerTest
    {
        private IntradayDecisionMaker _target;
        private MockRepository _mocks;
        private IScheduleMatrixLockableBitArrayConverter _matrixConverter;
        private IScheduleResultDataExtractor _dataExtractor;
        private IScheduleMatrixPro _scheduleMatrix;
        private IList<double?> _values;
        private IScheduleDayPro _scheduleDayPro0201Mon;
        private IScheduleDayPro _scheduleDayPro0202Tue;
        private IScheduleDayPro _scheduleDayPro0203Wed;
        private IScheduleDayPro _scheduleDayPro0204Thu;
        private IScheduleDayPro _scheduleDayPro0205Fri;
        private IScheduleDayPro _scheduleDayPro0206Sat;
        private IScheduleDayPro _scheduleDayPro0207Sun;

        private IScheduleDayPro _scheduleDayPro0208Mon;
        
        private IScheduleDayPro _scheduleDayPro0209Tue;
        private IScheduleDayPro _scheduleDayPro0210Wed;
        private IScheduleDayPro _scheduleDayPro0211Thu;
        private IScheduleDayPro _scheduleDayPro0212Fri;

        private IScheduleDayPro _scheduleDayPro0213Sat;
        private IScheduleDayPro _scheduleDayPro0214Sun;

        private IScheduleDayPro _scheduleDayPro0215Mon;
        private IScheduleDayPro _scheduleDayPro0216Tue;
        private IScheduleDayPro _scheduleDayPro0217Wed;
        private IScheduleDayPro _scheduleDayPro0218Thu;
        private IScheduleDayPro _scheduleDayPro0219Fri;
        private IScheduleDayPro _scheduleDayPro0220Sat;
        private IScheduleDayPro _scheduleDayPro0221Sun;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new IntradayDecisionMaker();

            _scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _matrixConverter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _dataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _values = new List<double?> { 20, 10, 30, 0 };

            _scheduleDayPro0201Mon = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0202Tue = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0203Wed = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0204Thu = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0205Fri = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0206Sat = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0207Sun = _mocks.StrictMock<IScheduleDayPro>();

            _scheduleDayPro0208Mon = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0209Tue = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0210Wed = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0211Thu = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0212Fri = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0213Sat = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0214Sun = _mocks.StrictMock<IScheduleDayPro>();

            _scheduleDayPro0215Mon = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0216Tue = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0217Wed = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0218Thu = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0219Fri = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0220Sat = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0221Sun = _mocks.StrictMock<IScheduleDayPro>();
        }

        [Test]
        public void SimpleTest()
        {

            ILockableBitArray bitArray = createBitArray();

            using (_mocks.Record())
            {
                simpleMatrixExpectations();

                Expect.Call(_matrixConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.Any();
                Expect.Call(_matrixConverter.Convert(false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(_values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            DateOnly? result = _target.Execute(_matrixConverter, _dataExtractor);

            DateOnly expected = new DateOnly(2010, 02, 11);

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(expected, result.Value);
        }

        //[Test]
        //public void SimpleLockedTest()
        //{

        //    ILockableBitArray bitArray = createBitArray();
        //    bitArray.Lock(1, true);
        //    bitArray.Lock(4, true);

        //    using (_mocks.Record())
        //    {
        //        simpleMatrixExpectations();

        //        Expect.Call(_matrixConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.Any();
        //        Expect.Call(_matrixConverter.Convert(false, false)).Return(bitArray).Repeat.Any();
        //        Expect.Call(_dataExtractor.Values()).Return(_values).Repeat.Any();
        //    }

        //    // day counterparts are > febr 9, 10, 11, 12
        //    IList<DateOnly> result = _target.Execute(_matrixConverter, _dataExtractor);
        //    Assert.AreEqual(2, result.Count);

        //    DateOnly mostUnderStaffingDay = new DateOnly(2010, 02, 10);
        //    DateOnly mostOverStaffingDay = new DateOnly(2010, 02, 11);

        //    Assert.AreEqual(mostUnderStaffingDay, result[0]);
        //    Assert.AreEqual(mostOverStaffingDay, result[1]);
        //}

        private void simpleMatrixExpectations()
        {
            IDictionary<DateOnly, IScheduleDayPro> unlockedDaysDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
            IDictionary<DateOnly, IScheduleDayPro> fullWeekPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();

            IList<IScheduleDayPro> outerPeriodList = new List<IScheduleDayPro>
                                                    {
                                                        _scheduleDayPro0201Mon,
                                                        _scheduleDayPro0202Tue,
                                                        _scheduleDayPro0203Wed,
                                                        _scheduleDayPro0204Thu,
                                                        _scheduleDayPro0205Fri,
                                                        _scheduleDayPro0206Sat,
                                                        _scheduleDayPro0207Sun,

                                                        _scheduleDayPro0208Mon,
                                                        _scheduleDayPro0209Tue,
                                                        _scheduleDayPro0210Wed,
                                                        _scheduleDayPro0211Thu,
                                                        _scheduleDayPro0212Fri,
                                                        _scheduleDayPro0213Sat,
                                                        _scheduleDayPro0214Sun,
                                                        
                                                        _scheduleDayPro0215Mon,
                                                        _scheduleDayPro0216Tue,
                                                        _scheduleDayPro0217Wed,
                                                        _scheduleDayPro0218Thu,
                                                        _scheduleDayPro0219Fri,
                                                        _scheduleDayPro0220Sat,
                                                        _scheduleDayPro0221Sun,
                                                    };




            for (int i = 0; i < 21; i++)
            {
                DateOnly currentDate = new DateOnly(2010, 02, 01).AddDays(i);
                IScheduleDayPro scheduleDay = outerPeriodList[i];
                Expect.Call(scheduleDay.Day).Return(currentDate).Repeat.Any();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(currentDate)).Return(outerPeriodList[i]).Repeat.Any();

                if (i >= 7 && i <= 13)
                {
                    fullWeekPeriodDictionary.Add(currentDate, scheduleDay);
                }

                if (i >= 8 && i <= 11)
                {
                    unlockedDaysDictionary.Add(currentDate, scheduleDay);
                }
            }

            Expect.Call(_scheduleMatrix.FullWeeksPeriodDays)
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(fullWeekPeriodDictionary.Values))).Repeat.Any();
            Expect.Call(_scheduleMatrix.UnlockedDays)
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(unlockedDaysDictionary.Values))).Repeat.Any();
        }


        private static ILockableBitArray createBitArray()
        {
            ILockableBitArray bitArray = new LockableBitArray(7, false, false, null);
            bitArray.PeriodArea = new MinMax<int>(1, 4);
            bitArray.Lock(0, true);
            bitArray.Lock(5, true);
            bitArray.Lock(6, true);
            return bitArray;
        }
    }
}