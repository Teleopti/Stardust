﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class WorkShiftBackToLegalStateBitArrayCreatorTest
    {
        private WorkShiftBackToLegalStateBitArrayCreator _target;
        private IScheduleMatrixPro _matrix;
        private IPossibleMinMaxWorkShiftLengthExtractor _minMaxCalculator;
        private MockRepository _mocks;

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
        private IScheduleDay _schedulePartWork;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _minMaxCalculator = _mocks.StrictMock<IPossibleMinMaxWorkShiftLengthExtractor>();
            _target = new WorkShiftBackToLegalStateBitArrayCreator(_minMaxCalculator);
            CreateScheduleDayList();
        }


        [Test]
        public void VerifyWeeklyBitArrayOnlyMainShiftsUnlocked()
        {
            using (_mocks.Record())
            {
                ExpectationsOnlyMainShiftsUnlocked();
            }
            using (_mocks.Playback())
            {
                ILockableBitArray lockableBitArray = _target.CreateWeeklyBitArray(1, _matrix);

                for (int i = 0; i < 7; i++)
                {
                    Assert.IsTrue(lockableBitArray.IsLocked(i, true));
                }
                Assert.IsTrue(lockableBitArray.IsLocked(7, true));
                Assert.IsFalse(lockableBitArray.IsLocked(8, true));
                Assert.IsFalse(lockableBitArray.IsLocked(9, true));
                Assert.IsTrue(lockableBitArray.IsLocked(10, true));
                Assert.IsTrue(lockableBitArray.IsLocked(11, true));
                Assert.IsTrue(lockableBitArray.IsLocked(12, true));
                Assert.IsFalse(lockableBitArray.IsLocked(13, true));
                for (int i = 14; i < 21; i++)
                {
                    Assert.IsTrue(lockableBitArray.IsLocked(i, true));
                }
            }

        }

        [Test]
        public void VerifyPeriodBitArrayOnlyMainShiftsUnlocked()
        {
            using (_mocks.Record())
            {
                ExpectationsOnlyMainShiftsUnlocked();
            }
            using (_mocks.Playback())
            {
                ILockableBitArray lockableBitArray = _target.CreatePeriodBitArray(true, _matrix);

                Assert.IsTrue(lockableBitArray.IsLocked(0, true));
                Assert.IsTrue(lockableBitArray.IsLocked(1, true));
                Assert.IsFalse(lockableBitArray.IsLocked(2, true));
                Assert.IsFalse(lockableBitArray.IsLocked(3, true));
                Assert.IsTrue(lockableBitArray.IsLocked(4, true));
                Assert.IsFalse(lockableBitArray.IsLocked(5, true));
                Assert.IsTrue(lockableBitArray.IsLocked(6, true));

                Assert.IsTrue(lockableBitArray.IsLocked(7, true));
                Assert.IsFalse(lockableBitArray.IsLocked(8, true));
                Assert.IsFalse(lockableBitArray.IsLocked(9, true));
                Assert.IsTrue(lockableBitArray.IsLocked(10, true));
                Assert.IsTrue(lockableBitArray.IsLocked(11, true));
                Assert.IsTrue(lockableBitArray.IsLocked(12, true));
                Assert.IsFalse(lockableBitArray.IsLocked(13, true));

                Assert.IsTrue(lockableBitArray.IsLocked(14, true));
                Assert.IsTrue(lockableBitArray.IsLocked(15, true));
                Assert.IsTrue(lockableBitArray.IsLocked(16, true));
                Assert.IsTrue(lockableBitArray.IsLocked(17, true));
                Assert.IsFalse(lockableBitArray.IsLocked(18, true));
                Assert.IsTrue(lockableBitArray.IsLocked(19, true));
                Assert.IsTrue(lockableBitArray.IsLocked(20, true));

            }

        }

        private void ExpectationsOnlyMainShiftsUnlocked()
        {
            IPerson person = PersonFactory.CreatePerson();

            IList<IScheduleDayPro> unlockedList = new List<IScheduleDayPro>
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
            IList<IScheduleDayPro> fullWeeksList = new List<IScheduleDayPro>
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
            IList<IScheduleDayPro> periodList = new List<IScheduleDayPro>
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

            Expect.Call(_matrix.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(unlockedList)).Repeat.Any();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(fullWeeksList)).Repeat.Any();
            Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodList)).Repeat.Any();

            for (int i = 0; i < 21; i++)
            {
                IScheduleDayPro day = unlockedList[i];
                Expect.Call(day.Day).Return(new DateOnly(2010, 4, 5).AddDays(i)).Repeat.Any();
            }

            // list of locked types
            Expect.Call(_schedulePartEmpty.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
            Expect.Call(_schedulePartDo.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            Expect.Call(_schedulePartCoff.SignificantPart()).Return(SchedulePartView.ContractDayOff).Repeat.Any();
            Expect.Call(_schedulePartAbsence.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();

            Expect.Call(_schedulePartWork.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_matrix.Person).Return(person).Repeat.Any();

            var projectionService = _mocks.StrictMock<IProjectionService>();
            var layerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            var minMaxContractTime = new MinMax<TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(8, 0, 0));

            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(), _matrix)).IgnoreArguments()
                .Return(minMaxContractTime).Repeat.Any();

            var contractTime = new TimeSpan(7, 0, 0);
            Expect.Call(_schedulePartWork.ProjectionService()).Return(projectionService).Repeat.Any();
            Expect.Call(projectionService.CreateProjection()).Return(layerCollection).Repeat.Any();
            Expect.Call(layerCollection.ContractTime()).Return(contractTime).Repeat.Any();

            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartCoff).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();

            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartCoff).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro14.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();

            Expect.Call(_scheduleDayPro15.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro16.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro17.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro18.DaySchedulePart()).Return(_schedulePartCoff).Repeat.Any();
            Expect.Call(_scheduleDayPro19.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro20.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro21.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
        }

        [Test]
        public void VerifyWeeklyBitArrayWorkingHoursInsideMinMaxRangeUnlocked()
        {
            using (_mocks.Record())
            {
                ExpectationsWorkingHoursInsideMinMaxRangeUnlocked();
            }
            using (_mocks.Playback())
            {
                ILockableBitArray lockableBitArray = _target.CreateWeeklyBitArray(1, _matrix);

                for (int i = 0; i < 7; i++)
                {
                    Assert.IsTrue(lockableBitArray.IsLocked(i, true));
                }
                Assert.IsFalse(lockableBitArray.IsLocked(7, true));
                Assert.IsTrue(lockableBitArray.IsLocked(8, true));
                Assert.IsTrue(lockableBitArray.IsLocked(9, true));
                Assert.IsFalse(lockableBitArray.IsLocked(10, true));
                Assert.IsTrue(lockableBitArray.IsLocked(11, true));
                Assert.IsFalse(lockableBitArray.IsLocked(12, true));
                Assert.IsTrue(lockableBitArray.IsLocked(13, true));
                for (int i = 14; i < 21; i++)
                {
                    Assert.IsTrue(lockableBitArray.IsLocked(i, true));
                }
            }
        }


        [Test]
        public void VerifyPeriodBitArrayWorkingHoursInsideMinMaxRangeUnlockedWithReducing()
        {
            using (_mocks.Record())
            {
                ExpectationsWorkingHoursInsideMinMaxRangeUnlocked();
            }
            using (_mocks.Playback())
            {
                ILockableBitArray lockableBitArray = _target.CreatePeriodBitArray(false, _matrix);

                Assert.IsFalse(lockableBitArray.IsLocked(0, true));
                Assert.IsTrue(lockableBitArray.IsLocked(1, true));
                Assert.IsFalse(lockableBitArray.IsLocked(2, true));
                Assert.IsTrue(lockableBitArray.IsLocked(3, true));
                Assert.IsTrue(lockableBitArray.IsLocked(4, true));
                Assert.IsFalse(lockableBitArray.IsLocked(5, true));
                Assert.IsTrue(lockableBitArray.IsLocked(6, true));

                Assert.IsFalse(lockableBitArray.IsLocked(7, true));
                Assert.IsTrue(lockableBitArray.IsLocked(8, true));
                Assert.IsTrue(lockableBitArray.IsLocked(9, true));
                Assert.IsFalse(lockableBitArray.IsLocked(10, true));
                Assert.IsTrue(lockableBitArray.IsLocked(11, true));
                Assert.IsFalse(lockableBitArray.IsLocked(12, true));
                Assert.IsTrue(lockableBitArray.IsLocked(13, true));

                Assert.IsTrue(lockableBitArray.IsLocked(14, true));
                Assert.IsFalse(lockableBitArray.IsLocked(15, true));
                Assert.IsTrue(lockableBitArray.IsLocked(16, true));
                Assert.IsFalse(lockableBitArray.IsLocked(17, true));
                Assert.IsTrue(lockableBitArray.IsLocked(18, true));
                Assert.IsTrue(lockableBitArray.IsLocked(19, true));
                Assert.IsFalse(lockableBitArray.IsLocked(20, true));
            }
        }

        [Test]
        public void VerifyPeriodBitArrayWorkingHoursInsideMinMaxRangeUnlockedWithRaising()
        {
            using (_mocks.Record())
            {
                ExpectationsWorkingHoursInsideMinMaxRangeUnlocked();
            }
            using (_mocks.Playback())
            {
                ILockableBitArray lockableBitArray = _target.CreatePeriodBitArray(true, _matrix);

                Assert.IsFalse(lockableBitArray.IsLocked(0, true));
                Assert.IsFalse(lockableBitArray.IsLocked(1, true));
                Assert.IsTrue(lockableBitArray.IsLocked(2, true));
                Assert.IsTrue(lockableBitArray.IsLocked(3, true));
                Assert.IsTrue(lockableBitArray.IsLocked(4, true));
                Assert.IsFalse(lockableBitArray.IsLocked(5, true));
                Assert.IsFalse(lockableBitArray.IsLocked(6, true));

                Assert.IsTrue(lockableBitArray.IsLocked(7, true));
                Assert.IsTrue(lockableBitArray.IsLocked(8, true));
                Assert.IsTrue(lockableBitArray.IsLocked(9, true));
                Assert.IsFalse(lockableBitArray.IsLocked(10, true));
                Assert.IsFalse(lockableBitArray.IsLocked(11, true));
                Assert.IsTrue(lockableBitArray.IsLocked(12, true));
                Assert.IsTrue(lockableBitArray.IsLocked(13, true));

                Assert.IsTrue(lockableBitArray.IsLocked(14, true));
                Assert.IsFalse(lockableBitArray.IsLocked(15, true));
                Assert.IsFalse(lockableBitArray.IsLocked(16, true));
                Assert.IsTrue(lockableBitArray.IsLocked(17, true));
                Assert.IsTrue(lockableBitArray.IsLocked(18, true));
                Assert.IsTrue(lockableBitArray.IsLocked(19, true));
                Assert.IsFalse(lockableBitArray.IsLocked(20, true));
            }
        }

        private void ExpectationsWorkingHoursInsideMinMaxRangeUnlocked()
        {
            IPerson person = PersonFactory.CreatePerson();

            IList<IScheduleDayPro> unlockedList = new List<IScheduleDayPro>
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
            IList<IScheduleDayPro> fullWeeksList = new List<IScheduleDayPro>
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
            IList<IScheduleDayPro> periodList = new List<IScheduleDayPro>
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

            Expect.Call(_matrix.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(unlockedList)).Repeat.Any();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(fullWeeksList)).Repeat.Any();
            Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodList)).Repeat.Any();

            for (int i = 0; i < 21; i++)
            {
                IScheduleDayPro day = unlockedList[i];
                DateOnly dateOnly = day.Day;
                Expect.Call(dateOnly).Return(new DateOnly(2010, 4, 5).AddDays(i)).Repeat.Any();
            }

            // list of locked types
            Expect.Call(_schedulePartEmpty.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
            Expect.Call(_schedulePartDo.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            Expect.Call(_schedulePartCoff.SignificantPart()).Return(SchedulePartView.ContractDayOff).Repeat.Any();
            Expect.Call(_schedulePartAbsence.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();

            Expect.Call(_schedulePartWork.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_matrix.Person).Return(person).Repeat.Any();

            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection layerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            TimeSpan contractTime = new TimeSpan(7, 0, 0);
            Expect.Call(_schedulePartWork.ProjectionService()).Return(projectionService).Repeat.Any();
            Expect.Call(projectionService.CreateProjection()).Return(layerCollection).Repeat.Any();
            Expect.Call(layerCollection.ContractTime()).Return(contractTime).Repeat.Any();

            // working time is 7 all the time
            MinMax<TimeSpan> minMaxContractTimeUpDown = new MinMax<TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(8, 0, 0));
            MinMax<TimeSpan> minMaxContractTimeUpOnly = new MinMax<TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(8, 0, 0));
            MinMax<TimeSpan> minMaxContractTimeDownOnly = new MinMax<TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(7, 0, 0));
            MinMax<TimeSpan> minMaxContractTimeNoUpNoDown = new MinMax<TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(7, 0, 0));

            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 5), _matrix))
                .Return(minMaxContractTimeUpDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 6), _matrix))
                .Return(minMaxContractTimeUpOnly).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 7), _matrix))
                .Return(minMaxContractTimeDownOnly).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 8), _matrix))
                .Return(minMaxContractTimeNoUpNoDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 9), _matrix))
                .Return(minMaxContractTimeNoUpNoDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 10), _matrix))
                .Return(minMaxContractTimeUpDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 11), _matrix))
                .Return(minMaxContractTimeUpOnly).Repeat.Any();

            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 12), _matrix))
                .Return(minMaxContractTimeDownOnly).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 13), _matrix))
                .Return(minMaxContractTimeNoUpNoDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 14), _matrix))
                .Return(minMaxContractTimeNoUpNoDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 15), _matrix))
                .Return(minMaxContractTimeUpDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 16), _matrix))
                .Return(minMaxContractTimeUpOnly).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 17), _matrix))
                .Return(minMaxContractTimeDownOnly).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 18), _matrix))
                .Return(minMaxContractTimeNoUpNoDown).Repeat.Any();

            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 19), _matrix))
                .Return(minMaxContractTimeNoUpNoDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 20), _matrix))
                .Return(minMaxContractTimeUpDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 21), _matrix))
                .Return(minMaxContractTimeUpOnly).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 22), _matrix))
                .Return(minMaxContractTimeDownOnly).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 23), _matrix))
                .Return(minMaxContractTimeNoUpNoDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 24), _matrix))
                .Return(minMaxContractTimeNoUpNoDown).Repeat.Any();
            Expect.Call(_minMaxCalculator.PossibleLengthsForDate(new DateOnly(2010, 4, 25), _matrix))
                .Return(minMaxContractTimeUpDown).Repeat.Any();

            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();

            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro14.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();

            Expect.Call(_scheduleDayPro15.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro16.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro17.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro18.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro19.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro20.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
            Expect.Call(_scheduleDayPro21.DaySchedulePart()).Return(_schedulePartWork).Repeat.Any();
        }

        private void CreateScheduleDayList()
        {
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

            _schedulePartWork = _mocks.StrictMock<IScheduleDay>();
        }
    }
}
