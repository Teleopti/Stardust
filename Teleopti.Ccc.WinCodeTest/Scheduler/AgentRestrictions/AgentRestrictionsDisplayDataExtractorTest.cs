using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling;
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
		private RestrictionSchedulingOptions _schedulingOptions;
		private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private IPeriodScheduledAndRestrictionDaysOff _periodScheduledAndRestrictionDaysOff;
		private ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		private IScheduleDayPro _scheduleDayPro;
		private ReadOnlyCollection<IScheduleDayPro> _effectiveDays;
		private IScheduleDay _scheduleDay;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;
		private IAgentRestrictionsNoWorkShiftfFinder _agentRestrictionsNoWorkShiftfFinder;
			
		[SetUp]
		public void Setup()
		{
			
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
			_periodScheduledAndRestrictionDaysOff = _mocks.StrictMock<IPeriodScheduledAndRestrictionDaysOff>();
			_schedulePeriodTargetTimeCalculator = _mocks.StrictMock<ISchedulePeriodTargetTimeCalculator>();
			_schedulingOptions = new RestrictionSchedulingOptions(){UseScheduling = true};
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_effectiveDays = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>{_scheduleDayPro});
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_projectionService = _mocks.StrictMock<IProjectionService>();
			_visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
			_agentRestrictionsNoWorkShiftfFinder = _mocks.StrictMock<IAgentRestrictionsNoWorkShiftfFinder>();
			_target = new AgentRestrictionsDisplayDataExtractor(_schedulePeriodTargetTimeCalculator, _workShiftMinMaxCalculator, _periodScheduledAndRestrictionDaysOff, _agentRestrictionsNoWorkShiftfFinder);
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
				Expect.Call(_periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(_matrix, _schedulingOptions.UseScheduling, _schedulingOptions.UsePreferences, _schedulingOptions.UseRotations)).Return(33).Repeat.AtLeastOnce();
				Expect.Call(_agentRestrictionsNoWorkShiftfFinder.Find(_scheduleDay, _schedulingOptions)).Return(true);
			}

			using(_mocks.Playback())
			{
				_target.ExtractTo(data, _schedulingOptions);
			}

			Assert.AreEqual(TimeSpan.MaxValue, data.ContractCurrentTime);
			Assert.AreEqual(TimeSpan.MinValue, data.ContractTargetTime);
			Assert.AreEqual(5, data.CurrentDaysOff);
			Assert.AreEqual(new TimePeriod(TimeSpan.MinValue, TimeSpan.MinValue), data.MinMaxTime);

			Assert.AreEqual(new MinMax<TimeSpan>().Minimum, data.MinimumPossibleTime);
			Assert.AreEqual(new MinMax<TimeSpan>().Maximum, data.MaximumPossibleTime);
			Assert.AreEqual(33, data.ScheduledAndRestrictionDaysOff);
			Assert.IsTrue(data.NoWorkshiftFound);
		}
	}
}