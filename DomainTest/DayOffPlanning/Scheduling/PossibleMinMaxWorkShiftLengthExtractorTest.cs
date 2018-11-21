using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning.Scheduling
{
    [TestFixture]
    public class PossibleMinMaxWorkShiftLengthExtractorTest
    {
        private IPossibleMinMaxWorkShiftLengthExtractor _target;
        private MockRepository _mocks;
        private IRestrictionExtractor _restrictionExctractor;
        private IScheduleMatrixPro _matrix;
        private IScheduleDayPro _scheduleDayPro0;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3; //p
        private IScheduleDayPro _scheduleDayPro4; //p
        private IScheduleDayPro _scheduleDayPro5; //p sa
        private IScheduleDayPro _scheduleDayPro6; //su
        private IScheduleDay _schedulePartEmpty;
        private IEffectiveRestriction _extractedRestriction;
        private IWorkShiftWorkTime _workShiftWorkTime;
        private IPerson _person;
        private IRuleSetBag _ruleSetBag;
    	private IVirtualSchedulePeriod _virtualSchedulePeriod;
        private SchedulingOptions _schedulingOptions;

	    [SetUp]
	    public void Setup()
	    {
		    _mocks = new MockRepository();
		    _restrictionExctractor = _mocks.StrictMock<IRestrictionExtractor>();
		    _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
		    _scheduleDayPro0 = _mocks.StrictMock<IScheduleDayPro>();
		    _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
		    _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
		    _scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
		    _scheduleDayPro4 = _mocks.StrictMock<IScheduleDayPro>();
		    _scheduleDayPro5 = _mocks.StrictMock<IScheduleDayPro>();
		    _scheduleDayPro6 = _mocks.StrictMock<IScheduleDayPro>();
		    _schedulePartEmpty = _mocks.StrictMock<IScheduleDay>();
		    _person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
		    _ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
		    _person.Period(DateOnly.MinValue).RuleSetBag = _ruleSetBag;
		    _person.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2010, 8, 2)));
		    _virtualSchedulePeriod = _person.VirtualSchedulePeriod((new DateOnly(2010, 8, 2)));
		    var start = new StartTimeLimitation();
		    var end = new EndTimeLimitation();
		    var time = new WorkTimeLimitation();
		    _extractedRestriction = new EffectiveRestriction(start, end, time, null, null, null,
			    new List<IActivityRestriction>());
		    _workShiftWorkTime =
			    new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
		    _schedulingOptions = new SchedulingOptions();

		    _target = new PossibleMinMaxWorkShiftLengthExtractor(_restrictionExctractor, _workShiftWorkTime, new RuleSetBagExtractorProvider());
	    }

	    [Test]
        public void ShouldBeAKeyForEveryFullWeeksDateInMatrix()
        {
            using (_mocks.Record())
            {
                matrixMock(false);
            	Expect.Call(_matrix.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                for (int i = 0; i < 7; i++)
                {
                    Assert.AreEqual(new MinMax<TimeSpan>(TimeSpan.FromHours(7), TimeSpan.FromHours(9)), _target.PossibleLengthsForDate(new DateOnly(2010, 8, 2).AddDays(i), _matrix, _schedulingOptions, null));
                }
            }
        }

		[Test]
		public void ShouldNotThrowExceptionIfNoHitInShiftBag()
		{
			using (_mocks.Record())
			{
				matrixMock(true);
				Expect.Call(_matrix.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				for (int i = 0; i < 7; i++)
				{
                    Assert.AreEqual(new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero), _target.PossibleLengthsForDate(new DateOnly(2010, 8, 2).AddDays(i), _matrix, _schedulingOptions, null));
				}
			}
		}

        [Test]
        public void ShouldReturnAverageWorkTimeIfPreferredAbsence()
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("hej");
            absence.InContractTime = true;
            _extractedRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, absence, new List<IActivityRestriction>());
            using (_mocks.Record())
            {
                matrixMock2();
                Expect.Call(_matrix.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                for (int i = 0; i < 1; i++)
                {
                    Assert.AreEqual(new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(8)).Maximum, _target.PossibleLengthsForDate(new DateOnly(2010, 8, 2).AddDays(i), _matrix, _schedulingOptions, null).Maximum);
                }
            }
        }

        private void matrixMock2()
        {
	        var extractedRestrictionResult = _mocks.StrictMock<IExtractedRestrictionResult>();
            IWorkTimeMinMax wtMinMax = new WorkTimeMinMax();
            wtMinMax.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(9));

            var scheduleDayPros = createFullWeekList();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(scheduleDayPros).Repeat.Any();

			IList<IScheduleDay> scheduleDays = createScheduleDayList();
            for (int i = 0; i < 7; i++)
            {
                Expect.Call(scheduleDayPros[i].Day).Return(new DateOnly(2010, 8, 2).AddDays(i)).Repeat.Any();
                Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 8, 2).AddDays(i))).Return(scheduleDayPros[i]).Repeat.Any();
                Expect.Call(scheduleDayPros[i].DaySchedulePart()).Return(scheduleDays[i]).Repeat.Any();
            }

			Expect.Call(_restrictionExctractor.Extract(scheduleDays[0])).Return(extractedRestrictionResult);
			Expect.Call(extractedRestrictionResult.CombinedRestriction(_schedulingOptions)).Return(_extractedRestriction);
			Expect.Call(_matrix.Person).Return(_person).Repeat.Any();
        }

        private void matrixMock(bool noShiftInBag)
        {
	        var extractedRestrictionResult = _mocks.StrictMock<IExtractedRestrictionResult>();
            IWorkTimeMinMax wtMinMax = new WorkTimeMinMax();
            wtMinMax.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(7),TimeSpan.FromHours(9));

			if (noShiftInBag)
				wtMinMax = null;

            var scheduleDayPros = createFullWeekList();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(scheduleDayPros)
                .Repeat.Any();

            IList<IScheduleDay> scheduleDays = createScheduleDayList();
            for (int i = 0; i < 7; i++)
            {
                Expect.Call(scheduleDayPros[i].Day).Return(new DateOnly(2010, 8, 2).AddDays(i)).Repeat.Any();
                Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 8, 2).AddDays(i))).Return(scheduleDayPros[i]).
                    Repeat.Any();
                Expect.Call(scheduleDayPros[i].DaySchedulePart()).Return(scheduleDays[i]).Repeat.Any();
                Expect.Call(_restrictionExctractor.Extract(scheduleDays[i])).Return(extractedRestrictionResult).Repeat.Once();
                Expect.Call(extractedRestrictionResult.CombinedRestriction(_schedulingOptions)).Return(
                    _extractedRestriction).Repeat.Once();
					 Expect.Call(_ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, new DateOnly(2010, 8, 2).AddDays(i),
                                                       _extractedRestriction)).Return(wtMinMax).Repeat.Once();
            }

            Expect.Call(_matrix.Person).Return(_person).Repeat.Any();

        }

        private IScheduleDayPro[] createFullWeekList()
        {
	        return new[]
	        {
		        _scheduleDayPro0,
		        _scheduleDayPro1,
		        _scheduleDayPro2,
		        _scheduleDayPro3,
		        _scheduleDayPro4,
		        _scheduleDayPro5,
		        _scheduleDayPro6
	        };
        }

        private IList<IScheduleDay> createScheduleDayList()
        {
            IList<IScheduleDay> ret = new List<IScheduleDay>
                                             {
                                                 _schedulePartEmpty,
                                                 _schedulePartEmpty,
                                                 _schedulePartEmpty,
                                                 _schedulePartEmpty,
                                                 _schedulePartEmpty,
                                                 _schedulePartEmpty,
                                                 _schedulePartEmpty
                                             };
            return ret;
        }
    }
}