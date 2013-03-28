using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockRestrictionOverLimitValidatorTest
	{
		private ITeamBlockRestrictionOverLimitValidator _target;
		private MockRepository _mocks;
		private IScheduleDayEquator _scheduleDayEquator;
		private ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private OptimizationPreferences _optimizerPreferences;
		private SchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private IScheduleMatrixOriginalStateContainerFactory _scheduleMatrixOriginalStateContainerFactory;
		private IOptimizationOverLimitByRestrictionDeciderFactory _optimizationOverLimitByRestrictionDeciderFactory;
		private IRestrictionOverLimitDecider _restrictionOverLimitDecider;
		private ITeamBlockInfo _teamBlockInfo;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleDayEquator = _mocks.StrictMock<IScheduleDayEquator>();
			_safeRollbackAndResourceCalculation = _mocks.StrictMock<ISafeRollbackAndResourceCalculation>();
			_optimizerPreferences = new OptimizationPreferences();
			_schedulingOptions = new SchedulingOptions();
			_schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleMatrixOriginalStateContainerFactory = _mocks.StrictMock<IScheduleMatrixOriginalStateContainerFactory>();
			_optimizationOverLimitByRestrictionDeciderFactory = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDeciderFactory>();
			_restrictionOverLimitDecider = _mocks.StrictMock<IRestrictionOverLimitDecider>();
			_target = new TeamBlockRestrictionOverLimitValidator(_restrictionOverLimitDecider, new Dictionary<IPerson, IScheduleRange>(), _scheduleDayEquator);
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
		}

		[Test]
		public void ShouldRollbackIfTeamBlockExceedsMoveMaxDayLimit()
		{
			var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixList = new List<IScheduleMatrixPro> {scheduleMatrixPro1 };

			var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			var restrictionChecker = new RestrictionChecker();

			using (_mocks.Record())
			{
				Expect.Call(
					_scheduleMatrixOriginalStateContainerFactory.CreateScheduleMatrixOriginalStateContainer(scheduleMatrixPro1,
					                                                                                        _scheduleDayEquator))
				      .Return(originalStateContainer);
				Expect.Call(
					_optimizationOverLimitByRestrictionDeciderFactory.CreateOptimizationOverLimitByRestrictionDecider(
						scheduleMatrixPro1, restrictionChecker, _optimizerPreferences, originalStateContainer))
				      .Return(optimizationOverLimitByRestrictionDecider);
				Expect.Call(optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(true);
				Expect.Call(()=> _safeRollbackAndResourceCalculation.Execute(_schedulePartModifyAndRollbackService, _schedulingOptions));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldValidateIfTeamBlockNotExceedsMoveMaxDayLimit()
		{
			var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1 };
			var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			var restrictionChecker = new RestrictionChecker();

			using (_mocks.Record())
			{
				Expect.Call(
					_scheduleMatrixOriginalStateContainerFactory.CreateScheduleMatrixOriginalStateContainer(scheduleMatrixPro1,
																											_scheduleDayEquator))
					  .Return(originalStateContainer);
				Expect.Call(
					_optimizationOverLimitByRestrictionDeciderFactory.CreateOptimizationOverLimitByRestrictionDecider(
						scheduleMatrixPro1, restrictionChecker, _optimizerPreferences, originalStateContainer))
					  .Return(optimizationOverLimitByRestrictionDecider);
				Expect.Call(optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false);
				Expect.Call(optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>());
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldRollbackIfTeamBlockOverLimit()
		{
			var dateOnly = new DateOnly();
			var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1 };

			var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			var restrictionChecker = new RestrictionChecker();

			using (_mocks.Record())
			{
				Expect.Call(
					_scheduleMatrixOriginalStateContainerFactory.CreateScheduleMatrixOriginalStateContainer(scheduleMatrixPro1,
																											_scheduleDayEquator))
					  .Return(originalStateContainer);
				Expect.Call(
					_optimizationOverLimitByRestrictionDeciderFactory.CreateOptimizationOverLimitByRestrictionDecider(
						scheduleMatrixPro1, restrictionChecker, _optimizerPreferences, originalStateContainer))
					  .Return(optimizationOverLimitByRestrictionDecider);
				Expect.Call(optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false);
				Expect.Call(optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>{dateOnly});
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_schedulePartModifyAndRollbackService, _schedulingOptions));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences);
				Assert.IsFalse(result);
			}
		}
		
		[Test]
		public void ShouldValidateIfTeamBlockNoOverLimit()
		{
			var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1 };

			var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			var restrictionChecker = new RestrictionChecker();

			using (_mocks.Record())
			{
				Expect.Call(
					_scheduleMatrixOriginalStateContainerFactory.CreateScheduleMatrixOriginalStateContainer(scheduleMatrixPro1,
																											_scheduleDayEquator))
					  .Return(originalStateContainer);
				Expect.Call(
					_optimizationOverLimitByRestrictionDeciderFactory.CreateOptimizationOverLimitByRestrictionDecider(
						scheduleMatrixPro1, restrictionChecker, _optimizerPreferences, originalStateContainer))
					  .Return(optimizationOverLimitByRestrictionDecider);
				Expect.Call(optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false);
				Expect.Call(optimizationOverLimitByRestrictionDecider.OverLimit()).Return(new List<DateOnly>());
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences);
				Assert.True(result);
			}
		}
	}
}
