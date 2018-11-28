using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning.Scheduling
{
    [TestFixture]
    public class WorkShiftMinMaxLengthCalculatorTest
    {
        private IWorkShiftMinMaxCalculator _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private IScheduleDayPro _scheduleDayPro0;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3; //p
        private IScheduleDayPro _scheduleDayPro4; //p
        private IScheduleDayPro _scheduleDayPro5; //p sa
        private IScheduleDayPro _scheduleDayPro6; //p su
        private IScheduleDayPro _scheduleDayPro7; //p
        private IScheduleDayPro _scheduleDayPro8; //p
        private IScheduleDayPro _scheduleDayPro9; //p
        private IScheduleDayPro _scheduleDayPro10; //p 
        private IScheduleDayPro _scheduleDayPro11; //p
        private IScheduleDayPro _scheduleDayPro12; //p sa
        private IScheduleDayPro _scheduleDayPro13; //su
        private IScheduleDay _schedulePartEmpty;
        private IScheduleDay _schedulePartDo;
        private IScheduleDay _schedulePartAbsence;
        private IScheduleDay _schedulePartShift;
        private IProjectionService _projectionServiceForAbsence;
        private IProjectionService _projectionServiceForShift;
        private IVisualLayerCollection _layerCollectionForAbsence;
        private IVisualLayerCollection _layerCollectionForShift;
        private IWorkShiftWeekMinMaxCalculator _workShiftWeekMinMaxCalculator;
        private IPersonContract _personContract;
        private IVirtualSchedulePeriod _schedulePeriod;
        private PossibleMinMaxWorkShiftLengthExtractorForTest _possibleMinMaxWorkShiftLengthExtractorForTest;
        private SchedulePeriodTargetTimeCalculatorForTest _schedulePeriodTargetTimeCalculatorForTest;
        private IPerson _person;
        private IContract _newContract;
        private SchedulingOptions _schedulingOptions;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _personContract = PersonContractFactory.CreatePersonContract("hej", "du", "glade");
			_personContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(40), TimeSpan.Zero, TimeSpan.Zero);
            _workShiftWeekMinMaxCalculator = _mocks.StrictMock<IWorkShiftWeekMinMaxCalculator>();
            _possibleMinMaxWorkShiftLengthExtractorForTest = new PossibleMinMaxWorkShiftLengthExtractorForTest();
            _schedulePeriodTargetTimeCalculatorForTest = new SchedulePeriodTargetTimeCalculatorForTest(new MinMax<TimeSpan>(TimeSpan.FromHours(55), TimeSpan.FromHours(57)));
            _target = new WorkShiftMinMaxCalculator(_possibleMinMaxWorkShiftLengthExtractorForTest,
                _schedulePeriodTargetTimeCalculatorForTest,
                _workShiftWeekMinMaxCalculator);
            _scheduleDayPro0 = _mocks.StrictMock<IScheduleDayPro>();
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
            _schedulePartEmpty = _mocks.StrictMock<IScheduleDay>();
            _schedulePartDo = _mocks.StrictMock<IScheduleDay>();
            _schedulePartAbsence = _mocks.StrictMock<IScheduleDay>();
            _schedulePartShift = _mocks.StrictMock<IScheduleDay>();
            _projectionServiceForAbsence = _mocks.StrictMock<IProjectionService>();
            _projectionServiceForShift = _mocks.StrictMock<IProjectionService>();
            _layerCollectionForAbsence = _mocks.StrictMock<IVisualLayerCollection>();
            _layerCollectionForShift = _mocks.StrictMock<IVisualLayerCollection>();
            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2010, 1, 1), new List<ISkill>());
            _person.AddSchedulePeriod(new SchedulePeriod(new DateOnly(2010, 8, 1), SchedulePeriodType.Day, 1 ));
            _newContract = new Contract("kalle");
            _schedulingOptions = new SchedulingOptions();
        }

        [Test]
        public void VerifyInLegalState()
        {
            using (_mocks.Record())
            {
                commonMocks();
                legalStateMocks();
                Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(0, null, null, _matrix)).IgnoreArguments()
                    .Return(new TimeSpan(0)).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                bool ret = _target.IsPeriodInLegalState(_matrix, _schedulingOptions);
                Assert.IsTrue(ret);
            }
           
        }

        [Test]
        public void VerifyNotInLegalStateTooLong()
        {
            using (_mocks.Record())
            {
                commonMocks();
                illegalStateMocksTooLong();
            }

            using (_mocks.Playback())
            {
                bool ret = _target.IsPeriodInLegalState(_matrix, _schedulingOptions);
                Assert.IsFalse(ret);
            }

        }

        [Test]
        public void VerifyNotInLegalStateTooShortAfterWeekCorrection()
        {
            using (_mocks.Record())
            {
                commonMocks();
                legalStateMocks();
                Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(0, null, null, _matrix)).IgnoreArguments()
                    .Return(TimeSpan.FromHours(5)).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                bool ret = _target.IsPeriodInLegalState(_matrix, _schedulingOptions);
                Assert.IsFalse(ret);
            }
        }

        [Test]
        public void VerifyNotInLegalStateTooShort()
        {
            using (_mocks.Record())
            {
                commonMocks();
                illegalStateMocksTooShort();
            }

            using (_mocks.Playback())
            {
                bool ret = _target.IsPeriodInLegalState(_matrix, _schedulingOptions);
                Assert.IsFalse(ret);
            }
        }

        [Test]
        public void VerifyWeekInLegalState()
        {
            using (_mocks.Record())
            {
                commonMocks();
                legalStateMocks();
                Expect.Call(_workShiftWeekMinMaxCalculator.IsInLegalState(0, new Dictionary<DateOnly, MinMax<TimeSpan>>(), _matrix)).IgnoreArguments().Return(true).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                bool ret = _target.IsWeekInLegalState(0, _matrix, _schedulingOptions);
                Assert.IsTrue(ret);
            }
        }

       
		[Test]
		public void VerifyWeeInLegalStateByDate()
		{
			using (_mocks.Record())
			{
				commonMocks();
				legalStateMocks();
                Expect.Call(_workShiftWeekMinMaxCalculator.IsInLegalState(0, new Dictionary<DateOnly, MinMax<TimeSpan>>(), _matrix)).IgnoreArguments().Return(true).Repeat.Any();
			}
			using (_mocks.Playback())
			{
                bool ret = _target.IsWeekInLegalState(new DateOnly(2010, 8, 2), _matrix, _schedulingOptions);
				Assert.IsTrue(ret);
			}
		}

		[Test]
		public void VerifyWeeInLegalStateByLastDate()
		{
			using (_mocks.Record())
			{
				commonMocks();
				legalStateMocks();
                Expect.Call(_workShiftWeekMinMaxCalculator.IsInLegalState(1, new Dictionary<DateOnly, MinMax<TimeSpan>>(), _matrix)).IgnoreArguments().Return(true).Repeat.Any();
			}
			using (_mocks.Playback())
			{
                bool ret = _target.IsWeekInLegalState(new DateOnly(2010, 8, 15), _matrix, _schedulingOptions);
				Assert.IsTrue(ret);
			}
		}

        [Test]
        public void VerifyMinMaxAllowedShiftContractTimeWithoutWeekCorrection()
        {
            DateOnly day = new DateOnly(2010, 8, 10);
            using (_mocks.Record())
            {
                commonMocks();
                verifyMinMaxAllowedShiftContractTimeWithoutWeekCorrectionMocks(day);
            }
            using (_mocks.Playback())
            {
                MinMax<TimeSpan> ret = _target.MinMaxAllowedShiftContractTime(day, _matrix, _schedulingOptions).Value;
                Assert.AreEqual(TimeSpan.FromHours(7), ret.Minimum);
                Assert.AreEqual(TimeSpan.FromHours(8), ret.Maximum);
            }
        }

        
        [Test]
        public void VerifyMinMaxAllowedShiftContractTimeWithWeekCorrection()
        {
            DateOnly day = new DateOnly(2010, 8, 6);
            _schedulePeriodTargetTimeCalculatorForTest.MinMax = new MinMax<TimeSpan>(TimeSpan.FromHours(56), TimeSpan.FromHours(56));
            using (_mocks.Record())
            {
                commonMocks();
                verifyMinMaxAllowedShiftContractTimeWithWeekCorrectionMocks(day);
            }
            using (_mocks.Playback())
            {
                MinMax<TimeSpan> ret = _target.MinMaxAllowedShiftContractTime(day, _matrix, _schedulingOptions).Value;
                Assert.AreEqual(TimeSpan.FromHours(8), ret.Minimum);
                Assert.AreEqual(TimeSpan.FromHours(9), ret.Maximum);
            }
        }

        [Test]
        public void VerifyMinMaxAllowedShiftContractTimeWithWeekCorrectionDifferentMaxWeekWorkTime()
        {
            DateOnly day = new DateOnly(2010, 8, 6);
            IPersonPeriod newPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 8, 5));
			_newContract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(1), TimeSpan.Zero, TimeSpan.Zero);
            newPeriod.PersonContract.Contract = _newContract;
            _person.AddPersonPeriod(newPeriod);
            _person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 8, 15)));
            _schedulePeriodTargetTimeCalculatorForTest.MinMax = new MinMax<TimeSpan>(TimeSpan.FromHours(56), TimeSpan.FromHours(56));
            using (_mocks.Record())
            {
                commonMocksNotFullWeeks();
                verifyMinMaxAllowedShiftContractTimeWithWeekCorrectionMocksNotFullWeeks(day);
            }
            using (_mocks.Playback())
            {
                MinMax<TimeSpan> ret = _target.MinMaxAllowedShiftContractTime(day, _matrix, _schedulingOptions).Value;
                Assert.AreEqual(TimeSpan.FromHours(7), ret.Minimum);
                Assert.AreEqual(TimeSpan.FromHours(9), ret.Maximum);
            }
        }


        [Test]
        public void VerifyWeeklyMaximumHours()
        {
            DateOnly day = new DateOnly(2010, 8, 6);
            using(_mocks.Record())
            {
                commonMocks();
                verifyMaximumWeeklyHoursMocks(day);
            }
            using (_mocks.Playback())
            {
                MinMax<TimeSpan> ret = _target.MinMaxAllowedShiftContractTime(day, _matrix, _schedulingOptions).Value;
                Assert.AreEqual(TimeSpan.FromHours(7), ret.Minimum);
                Assert.AreEqual(TimeSpan.FromHours(7), ret.Maximum);
            }
        }

        [Test]
        public void VerifyWeeklyMaximumWhenReturningNull()
        {
            var day = new DateOnly(2010, 8, 6);
            using (_mocks.Record())
            {
                commonMocks();
                verifyMaximumWeeklyHoursWhenReturningNullMocks(day);
            }
            using (_mocks.Playback())
            {
                MinMax<TimeSpan>? ret = _target.MinMaxAllowedShiftContractTime(day, _matrix, _schedulingOptions);
                Assert.IsFalse(ret.HasValue);
            }
        }

        [Test]
        public void WeeklyMaximumShouldReturnTrueIfWeekIsSplitAndNoSchedulePeriodBeforeExists()
        {
            //removing scheduleperiod
            _person.RemoveAllSchedulePeriods();
            var day = new DateOnly(2010, 8, 6);
            using (_mocks.Record())
            {
                commonMocksNotFullWeeks();
                verifyMinMaxAllowedShiftContractTimeWithWeekCorrectionMocksNotFullWeeks(day);
            }
            using (_mocks.Playback())
            {
                MinMax<TimeSpan> ret = _target.MinMaxAllowedShiftContractTime(day, _matrix, _schedulingOptions).Value;
                Assert.AreEqual(TimeSpan.FromHours(7), ret.Minimum);
                Assert.AreEqual(TimeSpan.FromHours(9), ret.Maximum);
            }
        }

        [Test]
        public void WeeklyMaximumShouldReturnTrueIfWeekIsSplitAndNoSchedulePeriodBeforeExistsXX()
        {
            //removing scheduleperiod
            _person.RemoveAllSchedulePeriods();
            var day = new DateOnly(2010, 8, 6);
            using (_mocks.Record())
            {
                commonMocks();
                verifyMinMaxAllowedShiftContractTimeWithWeekCorrectionMocks(day);
            }
            using (_mocks.Playback())
            {
                MinMax<TimeSpan> ret = _target.MinMaxAllowedShiftContractTime(day, _matrix, _schedulingOptions).Value;
                Assert.AreEqual(TimeSpan.FromHours(7), ret.Minimum);
                Assert.AreEqual(TimeSpan.FromHours(9), ret.Maximum);
            }
        }


        private void commonMocks()
        {
            Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
            Expect.Call(_schedulePeriod.Contract).Return(_personContract.Contract).Repeat.Any();
            Expect.Call(_matrix.EffectivePeriodDays).Return(createPeriodList()).Repeat.Any();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(createCompleteList()).Repeat.Any();
            Expect.Call(_schedulePartEmpty.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
            Expect.Call(_schedulePartDo.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            Expect.Call(_schedulePartAbsence.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();
            Expect.Call(_schedulePartShift.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_schedulePartAbsence.ProjectionService()).Return(_projectionServiceForAbsence).Repeat.Any();
            Expect.Call(_schedulePartShift.ProjectionService()).Return(_projectionServiceForShift).Repeat.Any();
            Expect.Call(_projectionServiceForAbsence.CreateProjection()).Return(_layerCollectionForAbsence).Repeat.
                Any();
            Expect.Call(_projectionServiceForShift.CreateProjection()).Return(_layerCollectionForShift).Repeat.
                Any();
            Expect.Call(_scheduleDayPro0.Day).Return(new DateOnly(2010, 8, 2)).Repeat.Any();
            Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2010, 8, 3)).Repeat.Any();
            Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2010, 8, 4)).Repeat.Any();
            Expect.Call(_scheduleDayPro3.Day).Return(new DateOnly(2010, 8, 5)).Repeat.Any();
            Expect.Call(_scheduleDayPro4.Day).Return(new DateOnly(2010, 8, 6)).Repeat.Any();
            Expect.Call(_scheduleDayPro5.Day).Return(new DateOnly(2010, 8, 7)).Repeat.Any();
            Expect.Call(_scheduleDayPro6.Day).Return(new DateOnly(2010, 8, 8)).Repeat.Any();
            Expect.Call(_scheduleDayPro7.Day).Return(new DateOnly(2010, 8, 9)).Repeat.Any();
            Expect.Call(_scheduleDayPro8.Day).Return(new DateOnly(2010, 8, 10)).Repeat.Any();
            Expect.Call(_scheduleDayPro9.Day).Return(new DateOnly(2010, 8, 11)).Repeat.Any();
            Expect.Call(_scheduleDayPro10.Day).Return(new DateOnly(2010, 8, 12)).Repeat.Any();
            Expect.Call(_scheduleDayPro11.Day).Return(new DateOnly(2010, 8, 13)).Repeat.Any();
            Expect.Call(_scheduleDayPro12.Day).Return(new DateOnly(2010, 8, 14)).Repeat.Any();
            Expect.Call(_scheduleDayPro13.Day).Return(new DateOnly(2010, 8, 15)).Repeat.Any();
            Expect.Call(_matrix.Person).Return(_person).Repeat.Any();
            Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2010, 8, 2),
                                                                                  new DateOnly(2010, 8, 15))).Repeat.Any
                ();

        }

        private void commonMocksNotFullWeeks()
        {
            Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
            Expect.Call(_schedulePeriod.Contract).Return(_newContract).Repeat.Any();
            
            Expect.Call(_matrix.EffectivePeriodDays).Return(createPeriodList()).Repeat.Any();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(createCompleteList()).Repeat.Any();
            Expect.Call(_schedulePartEmpty.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
            Expect.Call(_schedulePartDo.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            Expect.Call(_schedulePartAbsence.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();
            Expect.Call(_schedulePartShift.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_schedulePartAbsence.ProjectionService()).Return(_projectionServiceForAbsence).Repeat.Any();
            Expect.Call(_schedulePartShift.ProjectionService()).Return(_projectionServiceForShift).Repeat.Any();
            Expect.Call(_projectionServiceForAbsence.CreateProjection()).Return(_layerCollectionForAbsence).Repeat.
                Any();
            Expect.Call(_projectionServiceForShift.CreateProjection()).Return(_layerCollectionForShift).Repeat.
                Any();
            Expect.Call(_scheduleDayPro0.Day).Return(new DateOnly(2010, 8, 2)).Repeat.Any();
            Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2010, 8, 3)).Repeat.Any();
            Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2010, 8, 4)).Repeat.Any();
            Expect.Call(_scheduleDayPro3.Day).Return(new DateOnly(2010, 8, 5)).Repeat.Any();
            Expect.Call(_scheduleDayPro4.Day).Return(new DateOnly(2010, 8, 6)).Repeat.Any();
            Expect.Call(_scheduleDayPro5.Day).Return(new DateOnly(2010, 8, 7)).Repeat.Any();
            Expect.Call(_scheduleDayPro6.Day).Return(new DateOnly(2010, 8, 8)).Repeat.Any();
            Expect.Call(_scheduleDayPro7.Day).Return(new DateOnly(2010, 8, 9)).Repeat.Any();
            Expect.Call(_scheduleDayPro8.Day).Return(new DateOnly(2010, 8, 10)).Repeat.Any();
            Expect.Call(_scheduleDayPro9.Day).Return(new DateOnly(2010, 8, 11)).Repeat.Any();
            Expect.Call(_scheduleDayPro10.Day).Return(new DateOnly(2010, 8, 12)).Repeat.Any();
            Expect.Call(_scheduleDayPro11.Day).Return(new DateOnly(2010, 8, 13)).Repeat.Any();
            Expect.Call(_scheduleDayPro12.Day).Return(new DateOnly(2010, 8, 14)).Repeat.Any();
            Expect.Call(_scheduleDayPro13.Day).Return(new DateOnly(2010, 8, 15)).Repeat.Any();
            Expect.Call(_matrix.Person).Return(_person).Repeat.Any();
            Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2010, 8, 5),
                                                                                  new DateOnly(2010, 8, 14))).Repeat.Any
                ();

        }

        private void verifyMinMaxAllowedShiftContractTimeWithWeekCorrectionMocks(DateOnly day)
        {
               TimeSpan? weeklyMax = TimeSpan.FromHours(9);

            Expect.Call(_workShiftWeekMinMaxCalculator.MaxAllowedLength(0, possibleShiftLengths(), day,_matrix)).Return(weeklyMax);

			_personContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(48), TimeSpan.Zero, TimeSpan.Zero);
            Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(0, possibleShiftLengths(), day, _matrix)).Return(TimeSpan.FromHours(0)).Repeat.Once();
            Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(1, possibleShiftLengths(), day, _matrix)).Return(TimeSpan.FromHours(6)).Repeat.Once();

            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();

            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();

            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.Any();
        }

        private void verifyMinMaxAllowedShiftContractTimeWithWeekCorrectionMocksNotFullWeeks(DateOnly day)
        {
			_personContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(48), TimeSpan.Zero, TimeSpan.Zero);

	        Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(1, possibleShiftLengths(), day, _matrix))
		        .IgnoreArguments()
		        .Return(TimeSpan.FromHours(0))
		        .Repeat.Any();

            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();

            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();

            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
        }

        private void verifyMinMaxAllowedShiftContractTimeWithoutWeekCorrectionMocks(DateOnly day)
        {
               TimeSpan? weeklyMax = TimeSpan.FromHours(8);

               Expect.Call(_workShiftWeekMinMaxCalculator.MaxAllowedLength(1, possibleShiftLengths(), day, _matrix)).Return(weeklyMax);
               Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(0, possibleShiftLengths(), day, _matrix)).Return(TimeSpan.FromHours(0)).Repeat.Once();
               Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(1, possibleShiftLengths(), day, _matrix)).Return(TimeSpan.FromHours(0)).Repeat.Once();
           Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();

            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();

            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.Any();
        }

        private void verifyMaximumWeeklyHoursMocks(DateOnly day)
        {
            TimeSpan? weeklyMax = TimeSpan.FromHours(7);


            Expect.Call(_workShiftWeekMinMaxCalculator.MaxAllowedLength(0, possibleShiftLengths(), new DateOnly(2010, 8, 6), _matrix)).Return(weeklyMax);

            Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(0, possibleShiftLengths(), day, _matrix)).Return(TimeSpan.FromHours(0)).Repeat.Once();
            Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(1, possibleShiftLengths(), day, _matrix)).Return(TimeSpan.FromHours(0)).Repeat.Once();

            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();

            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();

            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.Any();
        }

        private void verifyMaximumWeeklyHoursWhenReturningNullMocks(DateOnly day)
        {
            TimeSpan? weeklyMax = null;
            Expect.Call(_workShiftWeekMinMaxCalculator.MaxAllowedLength(0, possibleShiftLengths(), new DateOnly(2010, 8, 6), _matrix)).Return(weeklyMax);

            Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(0, possibleShiftLengths(), day, _matrix)).Return(TimeSpan.FromHours(0)).Repeat.Once();
            Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(1, possibleShiftLengths(), day, _matrix)).Return(TimeSpan.FromHours(0)).Repeat.Once();

            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();

            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();

            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.Any();
        }

        private void legalStateMocks()
        {
            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
        }

        private void illegalStateMocksTooLong()
        {
            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.Any();
            Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(0, null, null, _matrix)).IgnoreArguments()
                .Return(new TimeSpan(0)).Repeat.AtLeastOnce();
        }

        private void illegalStateMocksTooShort()
        {
            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro12.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro13.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.Any();
            Expect.Call(_workShiftWeekMinMaxCalculator.CorrectionDiff(0, null, null, _matrix)).IgnoreArguments()
                .Return(new TimeSpan(0)).Repeat.AtLeastOnce();
        }

        private IScheduleDayPro[] createPeriodList()
        {
	        return new[]
	        {
		        _scheduleDayPro3,
		        _scheduleDayPro4,
		        _scheduleDayPro5,
		        _scheduleDayPro6,
		        _scheduleDayPro7,
		        _scheduleDayPro8,
		        _scheduleDayPro9,
		        _scheduleDayPro10,
		        _scheduleDayPro11,
		        _scheduleDayPro12
	        };
        }

        private IScheduleDayPro[] createCompleteList()
        {
	        return new[]
	        {
		        _scheduleDayPro0,
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
		        _scheduleDayPro13
	        };
        }

        private IDictionary<DateOnly, MinMax<TimeSpan>> possibleShiftLengths()
        {
            IDictionary<DateOnly, MinMax<TimeSpan>> ret = new Dictionary<DateOnly, MinMax<TimeSpan>>();
            for (int i = 0; i < 14; i++)
            {
                DateOnly dateOnly = new DateOnly(2010, 8, 2).AddDays(i);
                ret.Add(dateOnly, _possibleMinMaxWorkShiftLengthExtractorForTest.PossibleLengthsForDate(dateOnly, _matrix, _schedulingOptions, null));
            }
            return ret;
        }
    }

    public class SchedulePeriodTargetTimeCalculatorForTest : ISchedulePeriodTargetTimeCalculator
    {
        public MinMax<TimeSpan> MinMax { get;  set; }

        public SchedulePeriodTargetTimeCalculatorForTest(MinMax<TimeSpan> minMax)
        {
            MinMax = minMax;
        }

        public TimePeriod TargetWithTolerance(IScheduleMatrixPro matrix)
        {
            return new TimePeriod(MinMax.Minimum, MinMax.Maximum);
        }

    	public TimeSpan TargetTime(IScheduleMatrixPro matrix)
    	{
    		return MinMax.Minimum;
    	}

    	public TimeSpan TargetTime(IVirtualSchedulePeriod virtualSchedulePeriod, IEnumerable<IScheduleDay> scheduleDays) { throw new NotImplementedException(); }
    	public TimePeriod TargetTimeWithTolerance(IVirtualSchedulePeriod virtualSchedulePeriod, IEnumerable<IScheduleDay> scheduleDays) { throw new NotImplementedException(); }
    }

    public class PossibleMinMaxWorkShiftLengthExtractorForTest : IPossibleMinMaxWorkShiftLengthExtractor
    {
        public MinMax<TimeSpan> PossibleLengthsForDate(DateOnly dateOnly, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, OpenHoursSkillResult openHoursSkillResult)
        {
            if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
                return new MinMax<TimeSpan>(TimeSpan.FromHours(9), TimeSpan.FromHours(9));

            return new MinMax<TimeSpan>(TimeSpan.FromHours(7), TimeSpan.FromHours(9));
        }

        public void ResetCache()
        {
        }
    }
}
