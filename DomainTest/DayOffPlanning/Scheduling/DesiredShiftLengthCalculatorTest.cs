using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning.Scheduling
{
	[TestFixture]
	public class DesiredShiftLengthCalculatorTest
	{
		private IDesiredShiftLengthCalculator _target;
		private MockRepository _mocks;
		private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private IScheduleMatrixPro _matrix;
		private IVirtualSchedulePeriod _schedulePeriod;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private SchedulingOptions _schedulingOptions;
		private ISchedulePeriodTargetTimeCalculator _targetTimeCalculator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_targetTimeCalculator = _mocks.StrictMock<ISchedulePeriodTargetTimeCalculator>();
			_target = new DesiredShiftLengthCalculator(_targetTimeCalculator);
			_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_schedulingOptions = new SchedulingOptions();

		}

		[Test]
		public void ShouldReturnAverageWorkTimeWhenAllDaysAreSameAndIncludesCurrentAverage()
		{
			var minMaxDic = new Dictionary<DateOnly, MinMax<TimeSpan>>();
			minMaxDic.Add(new DateOnly(), new MinMax<TimeSpan>(TimeSpan.FromHours(7), TimeSpan.FromHours(8)));
			minMaxDic.Add(new DateOnly().AddDays(1), new MinMax<TimeSpan>(TimeSpan.FromHours(7), TimeSpan.FromHours(8)));

			using(_mocks.Record())
			{
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_workShiftMinMaxCalculator.PossibleMinMaxWorkShiftLengths(_matrix, _schedulingOptions, null)).Return(minMaxDic);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(), new DateOnly().AddDays(1))).
					Repeat.AtLeastOnce();
				Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly())).Return(_scheduleDayPro1);
				Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly().AddDays(1))).Return(_scheduleDayPro2);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();
				Expect.Call(_targetTimeCalculator.TargetTime(_matrix)).Return(TimeSpan.FromHours(16));
			}

			TimeSpan result;

			using(_mocks.Playback())
			{
				result = _target.FindAverageLength(_workShiftMinMaxCalculator, _matrix, _schedulingOptions, null);
			}

			Assert.AreEqual(new TimeSpan(8, 0, 0), result);
		}

		[Test]
		public void ShouldReturnAverageWorkTimeIfDifferentOpenHoursLow()
		{
			var minMaxDic = new Dictionary<DateOnly, MinMax<TimeSpan>>();
			minMaxDic.Add(new DateOnly(), new MinMax<TimeSpan>(TimeSpan.FromHours(7), TimeSpan.FromHours(7)));
			minMaxDic.Add(new DateOnly().AddDays(1), new MinMax<TimeSpan>(TimeSpan.FromHours(5), TimeSpan.FromHours(10)));

			using (_mocks.Record())
			{
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_workShiftMinMaxCalculator.PossibleMinMaxWorkShiftLengths(_matrix, _schedulingOptions, null)).Return(minMaxDic);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(), new DateOnly().AddDays(1))).
					Repeat.AtLeastOnce();
				Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly())).Return(_scheduleDayPro1).Repeat.AtLeastOnce();
				Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly().AddDays(1))).Return(_scheduleDayPro2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();
				Expect.Call(_targetTimeCalculator.TargetTime(_matrix)).Return(TimeSpan.FromHours(16));
			}

			TimeSpan result;

			using (_mocks.Playback())
			{
				result = _target.FindAverageLength(_workShiftMinMaxCalculator, _matrix, _schedulingOptions, null);
			}

			Assert.AreEqual(new TimeSpan(9, 0, 0), result);
		}

		[Test]
		public void ShouldReturnAverageWorkTimeIfDifferentOpenHoursHigh()
		{
			var minMaxDic = new Dictionary<DateOnly, MinMax<TimeSpan>>();
			minMaxDic.Add(new DateOnly(), new MinMax<TimeSpan>(TimeSpan.FromHours(9), TimeSpan.FromHours(9)));
			minMaxDic.Add(new DateOnly().AddDays(1), new MinMax<TimeSpan>(TimeSpan.FromHours(5), TimeSpan.FromHours(10)));

			using (_mocks.Record())
			{
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_workShiftMinMaxCalculator.PossibleMinMaxWorkShiftLengths(_matrix, _schedulingOptions, null)).Return(minMaxDic);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(), new DateOnly().AddDays(1))).
					Repeat.AtLeastOnce();
				Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly())).Return(_scheduleDayPro1).Repeat.AtLeastOnce();
				Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly().AddDays(1))).Return(_scheduleDayPro2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();
				Expect.Call(_targetTimeCalculator.TargetTime(_matrix)).Return(TimeSpan.FromHours(16));
			}

			TimeSpan result;

			using (_mocks.Playback())
			{
				result = _target.FindAverageLength(_workShiftMinMaxCalculator, _matrix, _schedulingOptions, null);
			}

			Assert.AreEqual(new TimeSpan(7, 0, 0), result);
		}

		[Test]
		public void ShouldReturnAverageWorkTimeIfPartlyScheduled()
		{
			var minMaxDic = new Dictionary<DateOnly, MinMax<TimeSpan>>();
			minMaxDic.Add(new DateOnly(), new MinMax<TimeSpan>(TimeSpan.FromHours(5), TimeSpan.FromHours(10)));
			minMaxDic.Add(new DateOnly().AddDays(1), new MinMax<TimeSpan>(TimeSpan.FromHours(5), TimeSpan.FromHours(10)));

			IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
			IVisualLayerCollection layers = _mocks.StrictMock<IVisualLayerCollection>();

			using (_mocks.Record())
			{
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_workShiftMinMaxCalculator.PossibleMinMaxWorkShiftLengths(_matrix, _schedulingOptions, null)).Return(minMaxDic);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(), new DateOnly().AddDays(1))).
					Repeat.AtLeastOnce();
				Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly())).Return(_scheduleDayPro1).Repeat.AtLeastOnce();
				Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly().AddDays(1))).Return(_scheduleDayPro2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
				Expect.Call(projectionService.CreateProjection()).Return(layers).Repeat.AtLeastOnce();
				Expect.Call(layers.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.AtLeastOnce();
				Expect.Call(_targetTimeCalculator.TargetTime(_matrix)).Return(TimeSpan.FromHours(16));
			}

			TimeSpan result;

			using (_mocks.Playback())
			{
				result = _target.FindAverageLength(_workShiftMinMaxCalculator, _matrix, _schedulingOptions, null);
			}

			Assert.AreEqual(new TimeSpan(9, 0, 0), result);
		}
	}
}