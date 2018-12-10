using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[TestFixture]
	public class WorkShiftValueCalculatorTest
	{
		private IWorkShiftPeriodValueCalculator _workShiftPeriodValueCalculator;
		private IWorkShiftLengthValueCalculator _workShiftLengthValueCalculator;
		private WorkShiftValueCalculator _target;
		private DateTimePeriod _period1;
		private readonly DateTime _start = new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc);
		private readonly DateTime _end = new DateTime(2009, 02, 02, 8, 30, 0, DateTimeKind.Utc);
		private readonly IActivity _phoneActivity = new Activity("phone") {RequiresSkill = true, RequiresSeat = true};
		private ISkillIntervalData _data;
		private IDictionary<DateTime, ISkillIntervalData> _dic;
		private PeriodValueCalculationParameters _periodValueCalculationParameters;

		[SetUp]
		public void Setup()
		{
			_workShiftPeriodValueCalculator = MockRepository.GenerateMock<IWorkShiftPeriodValueCalculator>();
			_workShiftLengthValueCalculator = MockRepository.GenerateMock<IWorkShiftLengthValueCalculator>();
			_target = new WorkShiftValueCalculator(_workShiftPeriodValueCalculator, _workShiftLengthValueCalculator);

			_period1 = new DateTimePeriod(_start, _end);
			_data = new SkillIntervalData(_period1, 5, -5, 0, null, null);
			_dic = new Dictionary<DateTime, ISkillIntervalData> {{_start, _data}};
			_periodValueCalculationParameters = new PeriodValueCalculationParameters(WorkShiftLengthHintOption.AverageWorkTime,false, false);
		}

		[Test]
		public void ShouldHandleClosedDayOnDifferentSkillActivity()
		{
			IActivity activity = new Activity("bo");
			var visualLayerCollection = new MainShiftLayer(activity, _period1).CreateProjection();
			double? result = _target.CalculateShiftValue(visualLayerCollection, new Activity("phone"),
				new Dictionary<DateTime, ISkillIntervalData>(), _periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.IsTrue(result.HasValue);
		}

		[Test]
		public void ShouldHandleClosedDayOnSameSkillActivity()
		{
			IActivity activity = new Activity("bo");
			var visualLayerCollection = new MainShiftLayer(activity, _period1).CreateProjection();

			double? result = _target.CalculateShiftValue(visualLayerCollection, activity,
				new Dictionary<DateTime, ISkillIntervalData>(), _periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.IsFalse(result.HasValue);
		}

		[Test]
		public void ShouldHandleClosedDayOnSameSkillActivityThatRequiresSkill()
		{
			var activity = new Activity("bo");
			activity.RequiresSkill = true;
			var visualLayerCollection = new MainShiftLayer(activity, _period1).CreateProjection();

			double? result = _target.CalculateShiftValue(visualLayerCollection, activity,
				new Dictionary<DateTime, ISkillIntervalData>(), _periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldHandleLayerStartingAndEndingInsideInterval()
		{
			var period2 = new DateTimePeriod(_start.AddMinutes(5), _end.AddMinutes(-5));
			var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();
			var periodValueCalculationParameters = new PeriodValueCalculationParameters(WorkShiftLengthHintOption.AverageWorkTime,false, false);

			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(_data, 20, false, false)).Return(50);
			_workShiftLengthValueCalculator.Stub(x => x.CalculateShiftValueForPeriod(50, 20,
				WorkShiftLengthHintOption.AverageWorkTime))
				.Return(50);

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.AreEqual(50, result);
		}

		[Test]
		public void ShouldHandleLayerStartingAndEndingOnExactInterval()
		{
			var period2 = new DateTimePeriod(_start, _end);
			var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();
			var periodValueCalculationParameters = new PeriodValueCalculationParameters(WorkShiftLengthHintOption.AverageWorkTime,false, false);

			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(_data, 30, false, false)).Return(50);
			_workShiftLengthValueCalculator.Stub(x => x.CalculateShiftValueForPeriod(50, 30,
				WorkShiftLengthHintOption.AverageWorkTime))
				.Return(50);

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.AreEqual(50, result);
		}

		[Test]
		public void ShouldHandleLayerEndingOnDaylightSavings()
		{
			var period2 = new DateTimePeriod(new DateTime(2015, 10, 25, 0, 15, 0, DateTimeKind.Utc),
				new DateTime(2015, 10, 25, 1, 0, 0, DateTimeKind.Utc));
			var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();
			var periodValueCalculationParameters = new PeriodValueCalculationParameters(WorkShiftLengthHintOption.AverageWorkTime,false, false);
			var localLayerTimeStart = new DateTime(2015, 10, 25, 2, 15, 0, DateTimeKind.Utc);

			_data = new SkillIntervalData(new DateTimePeriod(localLayerTimeStart, localLayerTimeStart.AddMinutes(15)), 5, -5, 0,
				null, null);
			_dic.Clear();
			_dic.Add(localLayerTimeStart, _data);

			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(_data, 15, false, false)).Return(50);

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, periodValueCalculationParameters, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldHandleLayerStartingOutsideOpenHoursAndEndingInsideInterval()
		{
			var period2 = new DateTimePeriod(_start.AddMinutes(-5), _end);
			var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, _periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldHandleLayerStartingInsideIntervalAndEndingOutsideOpenHours()
		{
			var period2 = new DateTimePeriod(_start.AddMinutes(5), _end.AddMinutes(5));
			var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();
			var periodValueCalculationParameters = new PeriodValueCalculationParameters(WorkShiftLengthHintOption.AverageWorkTime,false, false);

			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(_data, 25, false, false)).Return(50);

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldHandleTwoShortLayersWithinTheInterval()
		{
			var period1 = new DateTimePeriod(_start.AddMinutes(1), _end.AddMinutes(-28));
			var period2 = new DateTimePeriod(_start.AddMinutes(27), _end.AddMinutes(-1));
			var periodValueCalculationParameters = new PeriodValueCalculationParameters(WorkShiftLengthHintOption.AverageWorkTime,false, false);
			var visualLayerCollection = new[]
			{
				new MainShiftLayer(_phoneActivity, period1),
				new MainShiftLayer(_phoneActivity, period2)
			}.CreateProjection();

			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(_data, 1, false, false)).Return(5);
			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(_data, 2, false, false)).Return(7);

			_workShiftLengthValueCalculator.Stub(x => x.CalculateShiftValueForPeriod(12, 3,
				WorkShiftLengthHintOption.AverageWorkTime))
				.Return(50);

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.AreEqual(50, result);
		}

		[Test]
		public void ShouldHandleLayerStartingInOneIntervalAndEndingInAnother()
		{
			var periodValueCalculationParameters = new PeriodValueCalculationParameters(WorkShiftLengthHintOption.AverageWorkTime,false, false);
			var period1 = new DateTimePeriod(_start.AddMinutes(30), _end.AddMinutes(30));
			var data2 = new SkillIntervalData(period1, 5, -5, 0, null, null);
			_dic.Add(period1.StartDateTime, data2);

			var period2 = new DateTimePeriod(_start.AddMinutes(15), _end.AddMinutes(20));
			var visualLayerCollection = new MainShiftLayer(_phoneActivity, period2).CreateProjection();

			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(_data, 15, false, false)).Return(50);
			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(data2, 20, false, false)).Return(50);
			_workShiftLengthValueCalculator.Stub(x => x.CalculateShiftValueForPeriod(100, 35,
				WorkShiftLengthHintOption.AverageWorkTime))
				.Return(100);

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.AreEqual(100, result);
		}

		[Test]
		public void ActivityThatNotRequiresSkillCanExistOutsideOpenHours()
		{
			var otherActivity = new Activity("other") {RequiresSkill = false};
			var period2 = new DateTimePeriod(_start.AddMinutes(-15), _end.AddMinutes(15));
			var period1 = new DateTimePeriod(_start.AddMinutes(10), _end.AddMinutes(-10));
			var periodValueCalculationParameters = new PeriodValueCalculationParameters(
				WorkShiftLengthHintOption.AverageWorkTime,
				false, false);
			var visualLayerCollection = new[]
			{
				new MainShiftLayer(otherActivity, period2),
				new MainShiftLayer(_phoneActivity, period1)
			}.CreateProjection();

			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(_data, 10, false, false)).Return(50);
			_workShiftLengthValueCalculator.Stub(x => x.CalculateShiftValueForPeriod(50, 10,
				WorkShiftLengthHintOption.AverageWorkTime))
				.Return(100);

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.AreEqual(100, result);
		}

		[Test]
		public void LayerThatIsNotSkillActivityShouldCalculateAsZero()
		{
			var otherActivity = new Activity("other") {RequiresSkill = true};
			var period2 = new DateTimePeriod(_start.AddMinutes(5), _end.AddMinutes(-5));
			var visualLayerCollection = new MainShiftLayer(otherActivity, period2).CreateProjection();

			_workShiftLengthValueCalculator.Stub(x => x.CalculateShiftValueForPeriod(0, 0,
				WorkShiftLengthHintOption.AverageWorkTime))
				.Return(0);

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, _periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.AreEqual(0, result);
		}

		[Test]
		public void LayerThatIsNotSkillActivityShouldCalculateAsZeroEvenIfItIsOutsideOpenHours()
		{
			var otherActivity = new Activity("other") {RequiresSkill = true};
			var period2 = new DateTimePeriod(_start.AddMinutes(-5), _end.AddMinutes(-5));
			var visualLayerCollection = new MainShiftLayer(otherActivity, period2).CreateProjection();

			_workShiftLengthValueCalculator.Stub(x => x.CalculateShiftValueForPeriod(0, 0,
				WorkShiftLengthHintOption.AverageWorkTime))
				.Return(0);

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, _periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.AreEqual(0, result);
		}

		[Test]
		public void ShouldCalculateLayerStartingAfterMidnight()
		{
			var start = new DateTime(2009, 02, 02, 23, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2009, 02, 03, 0, 0, 0, DateTimeKind.Utc);
			var period1 = new DateTimePeriod(start, end);
			var period2 = new DateTimePeriod(start.AddHours(1), end.AddHours(1));
			var periodValueCalculationParameters = new PeriodValueCalculationParameters(WorkShiftLengthHintOption.AverageWorkTime,false, false);
			var visualLayerCollection = new[]
			{
				new MainShiftLayer(_phoneActivity, period1),
				new MainShiftLayer(_phoneActivity, period2)
			}.CreateProjection();

			var data1 = new SkillIntervalData(period1, 5, -5, 0, null, null);
			var data2 = new SkillIntervalData(period2, 5, -5, 0, null, null);
			_dic.Clear();
			_dic.Add(start, data1);
			_dic.Add(start.AddHours(1), data2);

			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(data1, 60, false, false)).Return(5);
			_workShiftPeriodValueCalculator.Stub(x => x.PeriodValue(data2, 60, false, false)).Return(7);
			_workShiftLengthValueCalculator.Stub(x => x.CalculateShiftValueForPeriod(12, 120,
				WorkShiftLengthHintOption.AverageWorkTime))
				.Return(50);

			double? result = _target.CalculateShiftValue(visualLayerCollection, _phoneActivity,
				_dic, periodValueCalculationParameters, TimeZoneInfo.Utc);
			Assert.AreEqual(50, result);
		}
	}
}