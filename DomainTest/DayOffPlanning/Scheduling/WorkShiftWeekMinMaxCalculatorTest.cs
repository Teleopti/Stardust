using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning.Scheduling
{
    [TestFixture]
    public class WorkShiftWeekMinMaxCalculatorTest
    {
        private IWorkShiftWeekMinMaxCalculator _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private IScheduleDayPro _scheduleDayPro0;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3; //p
        private IScheduleDayPro _scheduleDayPro4; //p
        private IScheduleDayPro _scheduleDayPro5; //p sa
        private IScheduleDayPro _scheduleDayPro6; //p su
        private IScheduleDay _schedulePartEmpty;
        private IScheduleDay _schedulePartDo;
        private IScheduleDay _schedulePartAbsence;
        private IScheduleDay _schedulePartShift;
        private IProjectionService _projectionServiceForAbsence;
        private IProjectionService _projectionServiceForShift;
        private IVisualLayerCollection _layerCollectionForAbsence;
        private IVisualLayerCollection _layerCollectionForShift;
        private IVirtualSchedulePeriod _schedulePeriod;
        //private IPersonPeriod _personPeriod;
        private DateOnly _startDate;
        private IPersonContract _personContract;
        private IScheduleDayPro[] _periodList;
        private SchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _startDate = new DateOnly(2010, 9, 6);
            _personContract = PersonContractFactory.CreatePersonContract("hej", "du", "glade");
			_personContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(40), TimeSpan.Zero, TimeSpan.Zero);
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro0 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro4 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro5 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro6 = _mocks.StrictMock<IScheduleDayPro>();
            _schedulePartEmpty = _mocks.StrictMock<IScheduleDay>();
            _schedulePartDo = _mocks.StrictMock<IScheduleDay>();
            _schedulePartAbsence = _mocks.StrictMock<IScheduleDay>();
            _schedulePartShift = _mocks.StrictMock<IScheduleDay>();
            _projectionServiceForAbsence = _mocks.StrictMock<IProjectionService>();
            _projectionServiceForShift = _mocks.StrictMock<IProjectionService>();
            _layerCollectionForAbsence = _mocks.StrictMock<IVisualLayerCollection>();
            _layerCollectionForShift = _mocks.StrictMock<IVisualLayerCollection>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _schedulingOptions = new SchedulingOptions();
            _target = new WorkShiftWeekMinMaxCalculator();
            _periodList = new[] { _scheduleDayPro3, _scheduleDayPro4, _scheduleDayPro5, _scheduleDayPro6 };
		}

        [Test]
        public void VerifyIsInLegalState()
        {
	        IPerson person = _mocks.StrictMock<IPerson>();
	        IPersonPeriod personPeriod = _mocks.StrictMock<IPersonPeriod>();
            using (_mocks.Record())
            {
				Expect.Call(_matrix.FullWeeksPeriodDays).Return(_periodList).Repeat.AtLeastOnce();
	            Expect.Call(_matrix.Person).Return(person).Repeat.AtLeastOnce();
	            Expect.Call(person.FirstDayOfWeek).Return(DayOfWeek.Monday);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_startDate, _startDate.AddDays(6))).Repeat.AtLeastOnce();
				Expect.Call(person.Period(_startDate.AddDays(3))).Return(personPeriod);
				Expect.Call(personPeriod.StartDate).Return(_startDate);
				Expect.Call(personPeriod.EndDate()).Return(_startDate.AddDays(28));
                commonMocks();
                legalStateMocks();
            }
            using (_mocks.Playback())
            {
                bool ret = _target.IsInLegalState(0, createList(), _matrix);
                Assert.IsTrue(ret);
            }
        }

        [Test]
        public void VerifyIsNotInLegalState()
        {
			IPerson person = _mocks.StrictMock<IPerson>();
			IPersonPeriod personPeriod = _mocks.StrictMock<IPersonPeriod>();
            using (_mocks.Record())
            {
				Expect.Call(_matrix.FullWeeksPeriodDays).Return(_periodList).Repeat.AtLeastOnce();
				Expect.Call(_matrix.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(person.FirstDayOfWeek).Return(DayOfWeek.Monday);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_startDate, _startDate.AddDays(6))).Repeat.AtLeastOnce();
				Expect.Call(person.Period(_startDate.AddDays(3))).Return(personPeriod);
				Expect.Call(personPeriod.StartDate).Return(_startDate);
				Expect.Call(personPeriod.EndDate()).Return(_startDate.AddDays(28));
                commonMocks();
                ilegalStateMocks();
            }
            using (_mocks.Playback())
            {
                bool ret = _target.IsInLegalState(0, createList(), _matrix);
                Assert.IsFalse(ret);
            }
        }

        [Test]
        public void VerifyCorrectionDiff()
        {
			_personContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(40), TimeSpan.Zero, TimeSpan.Zero);
            using (_mocks.Record())
            {
                commonMocks();
                corrDiffShouldBeTwoHoursMock();
            }
            using (_mocks.Playback())
            {
                TimeSpan ret = _target.CorrectionDiff(0, createList(), _startDate,_matrix);
                Assert.AreEqual(TimeSpan.FromHours(2), ret);
            }
        }

        [Test]
        public void VerifyCorrectionDiffCanBeZero()
        {
			_personContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(40), TimeSpan.Zero, TimeSpan.Zero);
            using (_mocks.Record())
            {
                commonMocks();
                corrDiffShouldBeZeroMock();
            }
            using (_mocks.Playback())
            {
                TimeSpan ret = _target.CorrectionDiff(0, createList(), _startDate,_matrix);
                Assert.AreEqual(TimeSpan.FromHours(0), ret);
            }
        }

        [Test]
        public void VerifyMaxAllowedShiftLength()
        {
			_personContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(48), TimeSpan.Zero, TimeSpan.Zero);
            using (_mocks.Record())
            {
                commonMocks();
                maxShiftLengthShouldBeSevenHoursMock();
            }
            using (_mocks.Playback())
            {
                TimeSpan ret = _target.MaxAllowedLength(0, createList(), _startDate.AddDays(3), _matrix).Value;
                Assert.AreEqual(TimeSpan.FromHours(7), ret);
            }
        }

        [Test]
        public void VerifyMaxAllowedShiftLengthReturnsNullIfWeekNotInLegalState()
        {
			_personContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(48), TimeSpan.Zero, TimeSpan.Zero);
            using (_mocks.Record())
            {
                commonMocks();
                maxShiftLengthShouldBeNull();
            }
            using (_mocks.Playback())
            {
                TimeSpan? ret = _target.MaxAllowedLength(0, createList(), _startDate.AddDays(3), _matrix);
                Assert.IsFalse(ret.HasValue);
            }
        }

        [Test]
        public void VerifyMaxAllowedShiftLengthCannotReturnMoreThanMaxPossible()
        {
			_personContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(48), TimeSpan.Zero, TimeSpan.Zero);
            using (_mocks.Record())
            {
                commonMocks();
                maxShiftLengthShouldBeNineHours();
            }
            using (_mocks.Playback())
            {
                TimeSpan ret = _target.MaxAllowedLength(0, createList(), _startDate.AddDays(3), _matrix).Value;
                Assert.AreEqual(TimeSpan.FromHours(9), ret);
            }
        }

        [Test]
        public void VerifyWeeklyRest()
        {
			_personContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(48), TimeSpan.Zero, TimeSpan.Zero);
            using (_mocks.Record())
            {
                commonMocks();
                maxShiftLengthShouldBeNineHours();
            }
            using (_mocks.Playback())
            {
                TimeSpan ret = _target.MaxAllowedLength(0, createList(), _startDate.AddDays(3),_matrix).Value;
                Assert.AreEqual(TimeSpan.FromHours(9), ret);
            }
        }



        private IDictionary<DateOnly, MinMax<TimeSpan>> createList()
        {
            
            IDictionary<DateOnly, MinMax<TimeSpan>> ret = new Dictionary<DateOnly, MinMax<TimeSpan>>();
            IPossibleMinMaxWorkShiftLengthExtractor extractor = new PossibleMinMaxWorkShiftLengthExtractorForThisTest();
            for (int i = 0; i < 7; i++)
            {
                ret.Add(_startDate.AddDays(i), extractor.PossibleLengthsForDate(_startDate.AddDays(i), _matrix, _schedulingOptions, null));
            }

            return ret;
        }

        private void commonMocks()
        {
            Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
            Expect.Call(_schedulePeriod.Contract).Return(_personContract.Contract).Repeat.Any();
            //Expect.Call(_schedulePeriod.PersonPeriod).Return(_personPeriod).Repeat.Any();
            //Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Any();

            Expect.Call(_matrix.GetScheduleDayByKey(_startDate.AddDays(0))).Return(_scheduleDayPro0).Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(_startDate.AddDays(1))).Return(_scheduleDayPro1).Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(_startDate.AddDays(2))).Return(_scheduleDayPro2).Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(_startDate.AddDays(3))).Return(_scheduleDayPro3).Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(_startDate.AddDays(4))).Return(_scheduleDayPro4).Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(_startDate.AddDays(5))).Return(_scheduleDayPro5).Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(_startDate.AddDays(6))).Return(_scheduleDayPro6).Repeat.Any();

            Expect.Call(_scheduleDayPro0.Day).Return(_startDate.AddDays(0)).Repeat.Any();
            Expect.Call(_scheduleDayPro1.Day).Return(_startDate.AddDays(1)).Repeat.Any();
            Expect.Call(_scheduleDayPro2.Day).Return(_startDate.AddDays(2)).Repeat.Any();
            Expect.Call(_scheduleDayPro3.Day).Return(_startDate.AddDays(3)).Repeat.Any();
            Expect.Call(_scheduleDayPro4.Day).Return(_startDate.AddDays(4)).Repeat.Any();
            Expect.Call(_scheduleDayPro5.Day).Return(_startDate.AddDays(5)).Repeat.Any();
            Expect.Call(_scheduleDayPro6.Day).Return(_startDate.AddDays(6)).Repeat.Any();

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
            Expect.Call(_matrix.EffectivePeriodDays).Return(_periodList).Repeat.Any();
        }

        private void maxShiftLengthShouldBeNineHours()
        {
            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.Any();
        }

        private void maxShiftLengthShouldBeNull()
        {
            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.Any();
        }

        private void maxShiftLengthShouldBeSevenHoursMock()
        {
            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.Any();
        }

        private void corrDiffShouldBeZeroMock()
        {
            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.Any();
        }

        private void corrDiffShouldBeTwoHoursMock()
        {
            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
        }

        private void legalStateMocks()
        {
            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
        }

        private void ilegalStateMocks()
        {
            Expect.Call(_scheduleDayPro0.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_layerCollectionForAbsence.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.Any();
            Expect.Call(_layerCollectionForShift.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
        }
    }

    public class PossibleMinMaxWorkShiftLengthExtractorForThisTest : IPossibleMinMaxWorkShiftLengthExtractor
    {
        public MinMax<TimeSpan> PossibleLengthsForDate(DateOnly dateOnly, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, OpenHoursSkillResult openHoursSkillResult)
        {
            if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
                return new MinMax<TimeSpan>(TimeSpan.FromHours(9), TimeSpan.FromHours(9));

            return new MinMax<TimeSpan>(TimeSpan.FromHours(7), TimeSpan.FromHours(9));
        }

        public void ResetCache()
        {
            throw new NotImplementedException();
        }
    }
}
