using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
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
		private IDictionary<DateTime, ISkillIntervalData> _dic;
	    private IMaxSeatsCalculationForTeamBlock _maxSeatsCalculationForTeamBlock;
	    private PeriodValueCalculationParameters _periodValueCalculationParameters;
	    private IDictionary<DateTime ,bool  > _maxSeatsPerIntervalDictionary;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _workShiftPeriodValueCalculator = _mocks.StrictMock<IWorkShiftPeriodValueCalculator>();
            _workShiftLengthValueCalculator = _mocks.StrictMock<IWorkShiftLengthValueCalculator>();
		      _maxSeatsCalculationForTeamBlock = _mocks.StrictMock<IMaxSeatsCalculationForTeamBlock>();
            _target = new WorkShiftValueCalculator(_workShiftPeriodValueCalculator, _workShiftLengthValueCalculator, _maxSeatsCalculationForTeamBlock);
            _start = new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc);
            _end = new DateTime(2009, 02, 02, 8, 30, 0, DateTimeKind.Utc);
            _phoneActivity = new Activity("phone");
            _phoneActivity.RequiresSkill = true;
		    _phoneActivity.RequiresSeat = true;
            _period1 = new DateTimePeriod(_start, _end);
            _data = new SkillIntervalData(_period1, 5, -5, 0, null, null);
            _dic = new Dictionary<DateTime, ISkillIntervalData>();
            _dic.Add(_start, _data);
				_maxSeatsPerIntervalDictionary = new Dictionary<DateTime, bool>();
			 _maxSeatsPerIntervalDictionary.Add(_start,false);
			   _periodValueCalculationParameters = new PeriodValueCalculationParameters(WorkShiftLengthHintOption.AverageWorkTime,
													 false, false, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak);
        }

        [Test]
        public void ShouldHandleClosedDayOnDifferentSkillActivity()
        {
            IActivity activity = new Activity("bo");
            var visualLayerCollection = new MainShiftLayer(activity, _period1).CreateProjection();
	        double? result = _target.CalculateShiftValue(visualLayerCollection, new Activity("phone"),
													 new Dictionary<DateTime, ISkillIntervalData>(), _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
            Assert.IsTrue(result.HasValue);
        }

        [Test]
        public void ShouldHandleClosedDayOnSameSkillActivity()
        {
            IActivity activity = new Activity("bo");
            var visualLayerCollection = new MainShiftLayer(activity, _period1).CreateProjection();

            double? result = _target.CalculateShiftValue(visualLayerCollection, activity,
										new Dictionary<DateTime, ISkillIntervalData>(), _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
			Assert.IsFalse(result.HasValue);
        }

        [Test]
        public void ShouldHandleClosedDayOnSameSkillActivityThatRequiresSkill()
        {
			var activity = new Activity("bo");
            activity.RequiresSkill = true;
            var visualLayerCollection = new MainShiftLayer(activity, _period1).CreateProjection();

            double? result = _target.CalculateShiftValue(visualLayerCollection, activity,
													 new Dictionary<DateTime, ISkillIntervalData>(), _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
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
					 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(50, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start] , true)).Return(50);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(50, 20,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(50);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
													 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
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
					 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(50, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start], true)).Return(50);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(50, 30,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(50);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
													 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
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
													 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
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
					 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(50, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start], true)).Return(50);
			}

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
													 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
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
					 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(5, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start], true)).Return(5);
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 2, false, false)).Return(7);
					 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(7, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start], true)).Return(7);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(12, 3,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(50);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
													 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
                Assert.AreEqual(50, result);
            }
        }

        [Test]
        public void ShouldHandleLayerStartingInOneIntervalAndEndingInAnother()
        {
            var period1 = new DateTimePeriod(_start.AddMinutes(30), _end.AddMinutes(30));
            ISkillIntervalData data2 = new SkillIntervalData(period1, 5, -5, 0, null, null);
            _dic.Add(period1.StartDateTime, data2);
			  _maxSeatsPerIntervalDictionary.Clear();
			  _maxSeatsPerIntervalDictionary.Add(_period1.StartDateTime ,false );

            DateTimePeriod period2 = new DateTimePeriod(_start.AddMinutes(15), _end.AddMinutes(20));
            var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();

            using (_mocks.Record())
            {
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(_data, 15, false, false)).Return(50);
					 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(50, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_period1.StartDateTime], true)).Return(50);
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data2, 20, false, false)).Return(50);
					 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(50, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_period1.StartDateTime], true)).Return(50);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(100, 35,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(100);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
													 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
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
					 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(50, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start ], true)).Return(50);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(50, 10,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(100);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
													 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
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
													 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
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
													 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
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

            var data1 = new SkillIntervalData(period1, 5, -5, 0, null, null);
            var data2 = new SkillIntervalData(period2, 5, -5, 0, null, null);
            _dic.Clear();
			_dic.Add(_start, data1);
			_dic.Add(_start.AddHours(1), data2);
			_maxSeatsPerIntervalDictionary.Clear();
			_maxSeatsPerIntervalDictionary.Add(_start, false);
			_maxSeatsPerIntervalDictionary.Add(_start.AddHours(1), false);
            using (_mocks.Record())
            {
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data1, 60, false, false)).Return(5);
					 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(5, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start], true)).Return(5);
                Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data2, 60, false, false)).Return(7);
					 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(7, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start.AddHours(1)], true)).Return(7);
                Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(12, 120,
                                                                                         WorkShiftLengthHintOption.AverageWorkTime))
                    .Return(50);
            }

            using (_mocks.Playback())
            {
                double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
													 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
                Assert.AreEqual(50, result);
            }
        }

	    [Test]
	    public void ShouldReturnNullIMaxSeatsCaculatorReturnsNull()
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

			 var data1 = new SkillIntervalData(period1, 5, -5, 0, null, null);
			 var data2 = new SkillIntervalData(period2, 5, -5, 0, null, null);
			 _dic.Clear();
			 _dic.Add(_start, data1);
			 _dic.Add(_start.AddHours(1), data2);
			 _maxSeatsPerIntervalDictionary.Clear();
			 _maxSeatsPerIntervalDictionary.Add(_start, false);
			 _maxSeatsPerIntervalDictionary.Add(_start.AddHours(1), false);

			 using (_mocks.Record())
			 {
				 Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data1, 60, false, false)).Return(5);
				 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(5, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start], true)).Return(null);
			 }

			 using (_mocks.Playback())
			 {
				 double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
												 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
				 Assert.IsNull(result);
				
			 }
			 
	    }

	    [Test]
	    public void RerurnSamePeriodValueIfAllIntervalHasNotReachedMaxSeats()
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

			 var data1 = new SkillIntervalData(period1, 5, -5, 0, null, null);
			 var data2 = new SkillIntervalData(period2, 5, -5, 0, null, null);
			 _dic.Clear();
			 _dic.Add(_start, data1);
			 _dic.Add(_start.AddHours(1), data2);
			 _maxSeatsPerIntervalDictionary.Clear();
			 _maxSeatsPerIntervalDictionary.Add(_start,false );
			 _maxSeatsPerIntervalDictionary.Add(_start.AddHours(1), false);

			 using (_mocks.Record())
			 {
				 Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data1, 60, false, false)).Return(5);
				 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(5, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start], true)).Return(null);
			 }

			 using (_mocks.Playback())
			 {
				 double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
												 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary );
				 Assert.IsNull(result);

			 }
	    }

		 [Test]
		 public void RerurnNullIfAllIntervalAnyReachedMaxSeats()
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

			 var data1 = new SkillIntervalData(period1, 5, -5, 0, null, null);
			 var data2 = new SkillIntervalData(period2, 5, -5, 0, null, null);
			 _dic.Clear();
			 _dic.Add(_start, data1);
			 _dic.Add(_start.AddHours(1), data2);
			 _maxSeatsPerIntervalDictionary.Clear();
			 _maxSeatsPerIntervalDictionary.Add(_start, true);
			 _maxSeatsPerIntervalDictionary.Add(_start.AddHours(1), true);
			 using (_mocks.Record())
			 {
				 Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data1, 60, false, false)).Return(5);
				 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(5, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start], true)).Return(5);
				 Expect.Call(_workShiftPeriodValueCalculator.PeriodValue(data2, 60, false, false)).Return(7);
				 Expect.Call(_maxSeatsCalculationForTeamBlock.PeriodValue(7, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, _maxSeatsPerIntervalDictionary[_start.AddHours(1)], true)).Return(7);
				 Expect.Call(_workShiftLengthValueCalculator.CalculateShiftValueForPeriod(12, 120,
																												  WorkShiftLengthHintOption.AverageWorkTime))
					  .Return(50);
			 }

			 using (_mocks.Playback())
			 {
				 double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
												 _dic, _periodValueCalculationParameters, TimeZoneInfo.Utc, _maxSeatsPerIntervalDictionary);
				 Assert.AreEqual(50, result);
			 }
		 }


    }


}