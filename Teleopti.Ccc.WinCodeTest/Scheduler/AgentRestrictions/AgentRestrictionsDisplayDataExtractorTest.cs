﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDisplayDataExtractorTest
	{
		private MockRepository _mocks;
		private IAgentRestrictionsDisplayDataExtractor _target;
		private IScheduleMatrixPro _matrix;
		private ISchedulingOptions _schedulingOptions;
		private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private IPeriodScheduledAndRestrictionDaysOff _periodScheduledAndRestrictionDaysOff;
		private IRestrictionExtractor _restrictionExtractor;
		private ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		private IScheduleDayPro _scheduleDayPro;
		private ReadOnlyCollection<IScheduleDayPro> _effectiveDays;
		private IScheduleDay _scheduleDay;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;
			
		[SetUp]
		public void Setup()
		{
			
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
			_periodScheduledAndRestrictionDaysOff = _mocks.StrictMock<IPeriodScheduledAndRestrictionDaysOff>();
			_restrictionExtractor = _mocks.StrictMock<IRestrictionExtractor>();
			_schedulePeriodTargetTimeCalculator = _mocks.StrictMock<ISchedulePeriodTargetTimeCalculator>();
			_target = new AgentRestrictionsDisplayDataExtractor(_schedulePeriodTargetTimeCalculator, _workShiftMinMaxCalculator, _periodScheduledAndRestrictionDaysOff, _restrictionExtractor);
			_schedulingOptions = new SchedulingOptions();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_effectiveDays = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>{_scheduleDayPro});
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_projectionService = _mocks.StrictMock<IProjectionService>();
			_visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
		}

		[Test]
		public void ShouldExtractToDisplayData()
		{
			IAgentDisplayData data = new AgentRestrictionsDisplayRow(_matrix);
			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetTime(_matrix)).Return(TimeSpan.MinValue);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_matrix)).Return(new TimePeriod(TimeSpan.MinValue, TimeSpan.MinValue));
				Expect.Call(_periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(_matrix, true, true, true)).Return(5);
				Expect.Call(_matrix.EffectivePeriodDays).Return(_effectiveDays);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue);

				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_workShiftMinMaxCalculator.PossibleMinMaxTimeForPeriod(_matrix, _schedulingOptions)).Return(new MinMax<TimeSpan>());
				Expect.Call(_periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(_matrix, true, _schedulingOptions.UsePreferences, _schedulingOptions.UseRotations)).Return(33);
			}

			using(_mocks.Playback())
			{
				_target.ExtractTo(data, _schedulingOptions, true);
			}

			Assert.AreEqual(TimeSpan.MaxValue, data.ContractCurrentTime);
			Assert.AreEqual(TimeSpan.MinValue, data.ContractTargetTime);
			Assert.AreEqual(5, data.CurrentDaysOff);
			Assert.AreEqual(new TimePeriod(TimeSpan.MinValue, TimeSpan.MinValue), data.MinMaxTime);

			Assert.AreEqual(new MinMax<TimeSpan>().Minimum, data.MinimumPossibleTime);
			Assert.AreEqual(new MinMax<TimeSpan>().Maximum, data.MaximumPossibleTime);
			Assert.AreEqual(33, data.ScheduledAndRestrictionDaysOff);
		}
	}
}