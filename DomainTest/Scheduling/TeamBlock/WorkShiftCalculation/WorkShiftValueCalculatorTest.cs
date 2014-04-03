using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
    [TestFixture]
    public class WorkShiftValueCalculatorTest
    {
        private MockRepository _mocks;
        private IWorkShiftPeriodValueCalculator _workShiftPeriodValueCalculator;
        private IWorkShiftLengthValueCalculator _workShiftLengthValueCalculator;
        private IWorkShiftValueCalculator _target;
        private DateTimePeriod _period1;
        private DateTime _start;
        private DateTime _end;
		private Activity _phoneActivity;
        private ISkillIntervalData _data;
        private IDictionary<TimeSpan, ISkillIntervalData> _dic;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _workShiftPeriodValueCalculator = _mocks.StrictMock<IWorkShiftPeriodValueCalculator>();
            _workShiftLengthValueCalculator = _mocks.StrictMock<IWorkShiftLengthValueCalculator>();
            _target = new WorkShiftValueCalculator(_workShiftPeriodValueCalculator, _workShiftLengthValueCalculator);
            _start = new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2009, 02, 02, 8, 30, 0, DateTimeKind.Utc);
            _phoneActivity = new Activity("phone");
            _phoneActivity.RequiresSkill = true;
            _period1 = new DateTimePeriod(_start, _end);
            _data = new SkillIntervalData(toBasePeriod(_period1), 5, -5, 0, null, null);
            _dic = new Dictionary<TimeSpan, ISkillIntervalData>();
            _dic.Add(_start.TimeOfDay, _data);
        }

        [Test]
        public void ShouldHandleClosedDayOnDifferentSkillActivity()
        {
            IActivity activity = new Activity("bo");
            var visualLayerCollection = new MainShiftLayer(activity, _period1).CreateProjection();

            double? result = _target.CalculateShiftValue(visualLayerCollection, new Activity("phone"),
                                        new Dictionary<TimeSpan, ISkillIntervalData>(), WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void ShouldHandleClosedDayOnSameSkillActivity()
        {
            IActivity activity = new Activity("bo");
            var visualLayerCollection = new MainShiftLayer(activity, _period1).CreateProjection();

            double? result = _target.CalculateShiftValue(visualLayerCollection, activity,
                                        new Dictionary<TimeSpan, ISkillIntervalData>(), WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void ShouldHandleClosedDayOnSameSkillActivityThatRequiresSkill()
        {
			var activity = new Activity("bo");
            activity.RequiresSkill = true;
            var visualLayerCollection = new MainShiftLayer(activity, _period1).CreateProjection();

            double? result = _target.CalculateShiftValue(visualLayerCollection, activity,
                                        new Dictionary<TimeSpan, ISkillIntervalData>(), WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
            Assert.IsNull(result);
        }

        [Test]
        public void ShouldHandleLayerStartingAndEndingInsideInterval()
        {
            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(5), _end.AddMinutes(-5));
            var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();

            using (_mocks.Record())
            {
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 20, false, false)).Return(50);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(50, 20,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(50);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
                                        _dic, WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
                Assert.AreEqual(50, result);
            }
        }

        [Test]
        public void ShouldHandleLayerStartingAndEndingOnExactInterval()
        {
            DateTimePeriod period2 = new DateTimePeriod(_start, _end);
            var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();

            using (_mocks.Record())
            {
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 30, false, false)).Return(50);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(50, 30,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(50);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
                                        _dic, WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
                Assert.AreEqual(50, result);
            }
        }

        [Test]
        public void ShouldHandleLayerStartingOutsideOpenHoursAndEndingInsideInterval()
        {
            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(-5), _end);
            var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();

            using (_mocks.Record())
            { }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
                                        _dic, WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
                Assert.IsNull(result);
            }
        }

        [Test]
        public void ShouldHandleLayerStartingInsideIntervalAndEndingOutsideOpenHours()
        {
            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(5), _end.AddMinutes(5));
            var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();

            using (_mocks.Record())
            {
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 25, false, false)).Return(50);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
                                        _dic, WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
                Assert.IsNull(result);
            }
        }

        [Test]
        public void ShouldHandleTwoShortLayersWithinTheInterval()
        {
            DateTimePeriod period1 = new DateTimePeriod(_start.AddMinutes(1), _end.AddMinutes(-28));
            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(27), _end.AddMinutes(-1));

            var visualLayerCollection = new[]
				{
					new MainShiftLayer(_phoneActivity, period1), 
					new MainShiftLayer(_phoneActivity, period2)
				}.CreateProjection();

            using (_mocks.Record())
            {
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 1, false, false)).Return(5);
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 2, false, false)).Return(7);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(12, 3,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(50);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
                                        _dic, WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
                Assert.AreEqual(50, result);
            }
        }

        [Test]
        public void ShouldHandleLayerStartingInOneIntervalAndEndingInAnother()
        {
            var period1 = new DateTimePeriod(_start.AddMinutes(30), _end.AddMinutes(30));
            ISkillIntervalData data2 = new SkillIntervalData(toBasePeriod(period1), 5, -5, 0, null, null);
            _dic.Add(period1.StartDateTime.TimeOfDay, data2);

            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(15), _end.AddMinutes(20));
            var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();

            using (_mocks.Record())
            {
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 15, false, false)).Return(50);
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data2, 20, false, false)).Return(50);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(100, 35,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(100);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
                                        _dic, WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
                Assert.AreEqual(100, result);
            }
        }

        [Test]
        public void ActivityThatNotRequiresSkillCanExistOutsideOpenHours()
        {
			var otherActivity = new Activity("other");
            otherActivity.RequiresSkill = false;
            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(-15), _end.AddMinutes(15));
            DateTimePeriod period1 = new DateTimePeriod(_start.AddMinutes(10), _end.AddMinutes(-10));

            var visualLayerCollection = new[]
				{
					new MainShiftLayer(otherActivity, period2), 
					new MainShiftLayer(_phoneActivity, period1)
				}.CreateProjection();


            using (_mocks.Record())
            {
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 10, false, false)).Return(50);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(50, 10,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(100);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
                                        _dic, WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
                Assert.AreEqual(100, result);
            }
        }

        [Test]
        public void LayerThatIsNotSkillActivityShouldCalculateAsZero()
        {
			var otherActivity = new Activity("other");
            otherActivity.RequiresSkill = true;
            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(5), _end.AddMinutes(-5));
            var visualLayerCollection = new MainShiftLayer(otherActivity, period2).CreateProjection();

            using (_mocks.Record())
            {
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(0, 0,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(0);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
                                        _dic, WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
                Assert.AreEqual(0, result);
            }
        }

        [Test]
        public void LayerThatIsNotSkillActivityShouldCalculateAsZeroEvenIfItIsOutsideOpenHours()
        {
			var otherActivity = new Activity("other");
            otherActivity.RequiresSkill = true;
            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(-5), _end.AddMinutes(-5));
            var visualLayerCollection = new MainShiftLayer(otherActivity, period2).CreateProjection();

            using (_mocks.Record())
            {
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(0, 0,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(0);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
                                        _dic, WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
                Assert.AreEqual(0, result);
            }
        }

        [Test]
        public void ShouldCalculateLayerStartingAfterMidnight()
        {
            _start = new DateTime(2009, 02, 02, 23, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2009, 02, 03, 0, 0, 0, DateTimeKind.Utc);
            DateTimePeriod period1 = new DateTimePeriod(_start, _end);
            DateTimePeriod period2 = new DateTimePeriod(_start.AddHours(1), _end.AddHours(1));

            var visualLayerCollection = new[]
				{
					new MainShiftLayer(_phoneActivity, period1), 
					new MainShiftLayer(_phoneActivity, period2)
				}.CreateProjection();

            var data1 = new SkillIntervalData(toBasePeriod(period1), 5, -5, 0, null, null);
            var data2 = new SkillIntervalData(toBasePeriod(period2), 5, -5, 0, null, null);
            _dic.Clear();
            _dic.Add(new TimeSpan(23, 0, 0), data1);
            _dic.Add(new TimeSpan(1, 0, 0, 0), data2);

            using (_mocks.Record())
            {
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data1, 60, false, false)).Return(5);
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data2, 60, false, false)).Return(7);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(12, 120,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(50);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
                                        _dic, WorkShiftLengthHintOption.AverageWorkTime,
                                        false, false);
                Assert.AreEqual(50, result);
            }
        }

        private static DateTimePeriod toBasePeriod(DateTimePeriod period)
        {
            var movedStart =
                new DateTime(SkillDayTemplate.BaseDate.Date.Ticks, DateTimeKind.Utc).Add(period.StartDateTime.TimeOfDay);
            var movedEnd =
                new DateTime(SkillDayTemplate.BaseDate.Date.Ticks, DateTimeKind.Utc).Add(period.EndDateTime.TimeOfDay);
            if (movedStart > movedEnd)
                movedEnd = movedEnd.AddDays(1);

            return new DateTimePeriod(movedStart, movedEnd);
        }
    }
}