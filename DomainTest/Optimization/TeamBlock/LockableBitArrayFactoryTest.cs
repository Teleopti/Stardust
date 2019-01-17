using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
    [TestFixture]
    public class LockableBitArrayFactoryTest
    {
        private ILockableBitArrayFactory _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;

        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3;
        private IScheduleDayPro _scheduleDayPro4;
        private IScheduleDayPro _scheduleDayPro5;
        private IScheduleDayPro _scheduleDayPro6;
        private IScheduleDayPro _scheduleDayPro7;
        private IScheduleDayPro _scheduleDayPro8;
        private IScheduleDayPro _scheduleDayPro9;
        private IScheduleDayPro _scheduleDayPro10;
        private IScheduleDayPro _scheduleDayPro11;
        private IScheduleDayPro _scheduleDayPro12;
        private IScheduleDayPro _scheduleDayPro13;
        private IScheduleDayPro _scheduleDayPro14;
        private IScheduleDayPro _scheduleDayPro15;
        private IScheduleDayPro _scheduleDayPro16;
        private IScheduleDayPro _scheduleDayPro17;
        private IScheduleDayPro _scheduleDayPro18;
        private IScheduleDayPro _scheduleDayPro19;
        private IScheduleDayPro _scheduleDayPro20;
        private IScheduleDayPro _scheduleDayPro21;
        private IScheduleDay _schedulePartEmpty;
        private IScheduleDay _schedulePartDo;
		private IScheduleDay _schedulePartCoff;
        private IScheduleDay _schedulePartAbsence;
        private IScheduleDay _schedulePartEarly;
        private IScheduleDay _schedulePartLate;
        private IShiftCategory _early;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_target = new LockableBitArrayFactory();

            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro4 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro5 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro6 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro7 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro8 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro9 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro10 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro11 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro12 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro13 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro14 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro15 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro16 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro17 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro18 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro19 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro20 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro21 = _mocks.StrictMock<IScheduleDayPro>();
            _schedulePartEmpty = _mocks.StrictMock<IScheduleDay>();
            _schedulePartDo = _mocks.StrictMock<IScheduleDay>();
			_schedulePartCoff = _mocks.StrictMock<IScheduleDay>();
            _schedulePartAbsence = _mocks.StrictMock<IScheduleDay>();
            _schedulePartEarly = _mocks.StrictMock<IScheduleDay>();
            _schedulePartLate = _mocks.StrictMock<IScheduleDay>();
        }

        [Test]
        public void VerifyConvert()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                scheduleExpectations();
            }
            ILockableBitArray ret;
            using (_mocks.Playback())
            {
                ret = _target.ConvertFromMatrix(false, false, _matrix);
            }
            
            Assert.AreEqual(7, ret.Count);
            Assert.AreEqual(new MinMax<int>(2, 4) , ret.PeriodArea);
            Assert.IsFalse(ret[2]);
            Assert.IsTrue(ret[3]);
            Assert.IsTrue(ret.IsLocked(2, true));
            Assert.IsFalse(ret.IsLocked(3, true));
        }

        [Test]
        public void VerifyConvertWeekBefore()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                scheduleExpectations();
            }
            ILockableBitArray ret;
            using (_mocks.Playback())
            {
				ret = _target.ConvertFromMatrix(true, false, _matrix);
            }

            Assert.AreEqual(14, ret.Count);
            Assert.AreEqual(new MinMax<int>(9, 11), ret.PeriodArea);
        }

        [Test]
        public void VerifyConvertWeekAfter()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                scheduleExpectations();
            }
            ILockableBitArray ret;
            using (_mocks.Playback())
            {
				ret = _target.ConvertFromMatrix(false, true, _matrix);
            }

            Assert.AreEqual(14, ret.Count);
            Assert.AreEqual(new MinMax<int>(2, 4), ret.PeriodArea);
        }

        [Test]
        public void VerifyConvertWeekBeforeAndAfter()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                scheduleExpectations();
            }
            ILockableBitArray ret;
            using (_mocks.Playback())
            {
				ret = _target.ConvertFromMatrix(true, true, _matrix);
            }

            Assert.AreEqual(21, ret.Count);
            Assert.AreEqual(new MinMax<int>(9, 11), ret.PeriodArea);
        }

        [Test]
        public void VerifyFullDayAbsencesAreNotLockedByConverter()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                scheduleExpectations();
            }
            ILockableBitArray ret;
            using (_mocks.Playback())
            {
				ret = _target.ConvertFromMatrix(false, false, _matrix);
            }

            Assert.IsFalse(ret.IsLocked(4, true));
        }

        private void mockExpectations()
        {
            IPerson person = PersonFactory.CreatePerson();
            _early = ShiftCategoryFactory.CreateShiftCategory("XX");
            IShiftCategory late = ShiftCategoryFactory.CreateShiftCategory("YY");
            IPersonAssignment earlyAssignment = _mocks.StrictMock<IPersonAssignment>();
            IPersonAssignment lateAssignment = _mocks.StrictMock<IPersonAssignment>();
            var outerWeeksList = new []
                                                        {
                                                            _scheduleDayPro1,
                                                            _scheduleDayPro2,
                                                            _scheduleDayPro3,
                                                            _scheduleDayPro4,
                                                            _scheduleDayPro5,
                                                            _scheduleDayPro6,
                                                            _scheduleDayPro7,
                                                            _scheduleDayPro8,
                                                            _scheduleDayPro9,
                                                            _scheduleDayPro10,
                                                            _scheduleDayPro11,
                                                            _scheduleDayPro12,
                                                            _scheduleDayPro13,
                                                            _scheduleDayPro14,
                                                            _scheduleDayPro15,
                                                            _scheduleDayPro16,
                                                            _scheduleDayPro17,
                                                            _scheduleDayPro18,
                                                            _scheduleDayPro19,
                                                            _scheduleDayPro20,
                                                            _scheduleDayPro21
                                                        };
            var periodList = new []
                                                    {
                                                        _scheduleDayPro10,
                                                        _scheduleDayPro11,
                                                        _scheduleDayPro12
                                                    };
            var unlockedList = new HashSet<IScheduleDayPro>
                                                      {
                                                          _scheduleDayPro11,
                                                          _scheduleDayPro12
                                                      };

            var fullWeeksList = new []
                                                       {
                                                           _scheduleDayPro8,
                                                           _scheduleDayPro9,
                                                           _scheduleDayPro10,
                                                           _scheduleDayPro11,
                                                           _scheduleDayPro12,
                                                           _scheduleDayPro13,
                                                           _scheduleDayPro14
                                                       };
            var weekBeforeOuterPeriodList = new []
                                                                   {
                                                                       _scheduleDayPro1,
                                                                       _scheduleDayPro2,
                                                                       _scheduleDayPro3,
                                                                       _scheduleDayPro4,
                                                                       _scheduleDayPro5,
                                                                       _scheduleDayPro6,
                                                                       _scheduleDayPro7,
                                                                       _scheduleDayPro8,
                                                                       _scheduleDayPro9,
                                                                       _scheduleDayPro10,
                                                                       _scheduleDayPro11,
                                                                       _scheduleDayPro12,
                                                                       _scheduleDayPro13,
                                                                       _scheduleDayPro14
                                                                   };
            var weekAfterOuterPeriodList = new []
                                                                  {
                                                                      _scheduleDayPro8,
                                                                      _scheduleDayPro9,
                                                                      _scheduleDayPro10,
                                                                      _scheduleDayPro11,
                                                                      _scheduleDayPro12,
                                                                      _scheduleDayPro13,
                                                                      _scheduleDayPro14,
                                                                      _scheduleDayPro15,
                                                                      _scheduleDayPro16,
                                                                      _scheduleDayPro17,
                                                                      _scheduleDayPro18,
                                                                      _scheduleDayPro19,
                                                                      _scheduleDayPro20,
                                                                      _scheduleDayPro21
                                                                  };

            Expect.Call(_matrix.UnlockedDays).Return(unlockedList).Repeat.Any();
            Expect.Call(_matrix.EffectivePeriodDays).Return(periodList).Repeat.Any();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(fullWeeksList).Repeat.Any();
            Expect.Call(_matrix.WeekBeforeOuterPeriodDays).Return(weekBeforeOuterPeriodList).Repeat.Any();
            Expect.Call(_matrix.WeekAfterOuterPeriodDays).Return(weekAfterOuterPeriodList).Repeat.Any();
            Expect.Call(_matrix.OuterWeeksPeriodDays).Return(outerWeeksList).Repeat.Any();

            for (int i = 0; i < 21; i++)
            {
                IScheduleDayPro day = outerWeeksList[i];
                Expect.Call(day.Day).Return(new DateOnly(2010, 4, 5).AddDays(i)).Repeat.Any();
            }
            Expect.Call(_schedulePartEmpty.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
            Expect.Call(_schedulePartDo.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
			Expect.Call(_schedulePartCoff.SignificantPart()).Return(SchedulePartView.ContractDayOff).Repeat.Any();
            Expect.Call(_schedulePartAbsence.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();
            Expect.Call(_schedulePartEarly.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_schedulePartEarly.PersonAssignment()).Return(earlyAssignment).Repeat.Any();
			Expect.Call(earlyAssignment.ShiftCategory).Return(_early).Repeat.Any();
            Expect.Call(_schedulePartLate.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_schedulePartLate.PersonAssignment()).Return(lateAssignment).Repeat.Any();
			Expect.Call(lateAssignment.ShiftCategory).Return(late).Repeat.Any();
            Expect.Call(_matrix.Person).Return(person).Repeat.Any();
        }

        private void scheduleExpectations()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartCoff).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();


            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();

            // effective period days (3 days)
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();

            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro14.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();


            Expect.Call(_scheduleDayPro15.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro16.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro17.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro18.DaySchedulePart()).Return(_schedulePartCoff).Repeat.Any();
            Expect.Call(_scheduleDayPro19.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro20.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro21.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
        }
    }
}