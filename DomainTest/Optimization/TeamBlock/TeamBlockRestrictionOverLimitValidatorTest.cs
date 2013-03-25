using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
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
			_target = new TeamBlockRestrictionOverLimitValidator(_scheduleDayEquator, _safeRollbackAndResourceCalculation,
			                                                     _scheduleMatrixOriginalStateContainerFactory,
			                                                     _optimizationOverLimitByRestrictionDeciderFactory);
		}

		[Test]
		public void ShouldRollbackIfTeamBlockExceedsMoveMaxDayLimit()
		{
			var dateOnly = new DateOnly();
			var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var matrixList = new List<IScheduleMatrixPro> {scheduleMatrixPro1 };
		    var groupMatrixList = new List<IList<IScheduleMatrixPro>> {matrixList};

			var person = PersonFactory.CreatePerson();
			var groupPerson = new GroupPerson(new List<IPerson>{person}, DateOnly.MinValue, "Hej", null);
			var teaminfo = new TeamInfo(groupPerson, groupMatrixList);
            var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
            var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

			var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			var restrictionChecker = new RestrictionChecker();

			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateOnly, dateOnly));
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
				var result = _target.Validate(teamBlockInfo, _optimizerPreferences, _schedulingOptions,
											  _schedulePartModifyAndRollbackService, restrictionChecker);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldValidateIfTeamBlockNotExceedsMoveMaxDayLimit()
		{
			var dateOnly = new DateOnly();
			var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1 };
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixList };

			var person = PersonFactory.CreatePerson();
			var groupPerson = new GroupPerson(new List<IPerson> { person }, DateOnly.MinValue, "Hej", null);
			var teaminfo = new TeamInfo(groupPerson, groupMatrixList);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

			var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			var restrictionChecker = new RestrictionChecker();

			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateOnly, dateOnly));
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
				var result = _target.Validate(teamBlockInfo, _optimizerPreferences, _schedulingOptions,
											  _schedulePartModifyAndRollbackService, restrictionChecker);
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
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixList };

			var person = PersonFactory.CreatePerson();
			var groupPerson = new GroupPerson(new List<IPerson> { person }, DateOnly.MinValue, "Hej", null);
			var teaminfo = new TeamInfo(groupPerson, groupMatrixList);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

			var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			var restrictionChecker = new RestrictionChecker();

			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateOnly, dateOnly));
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
				var result = _target.Validate(teamBlockInfo, _optimizerPreferences, _schedulingOptions,
											  _schedulePartModifyAndRollbackService, restrictionChecker);
				Assert.IsFalse(result);
			}
		}
		
		[Test]
		public void ShouldValidateIfTeamBlockNoOverLimit()
		{
			var dateOnly = new DateOnly();
			var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1 };
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixList };

			var person = PersonFactory.CreatePerson();
			var groupPerson = new GroupPerson(new List<IPerson> { person }, DateOnly.MinValue, "Hej", null);
			var teaminfo = new TeamInfo(groupPerson, groupMatrixList);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

			var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			var restrictionChecker = new RestrictionChecker();

			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro1.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateOnly, dateOnly));
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
				var result = _target.Validate(teamBlockInfo, _optimizerPreferences, _schedulingOptions,
											  _schedulePartModifyAndRollbackService, restrictionChecker);
				Assert.True(result);
			}
		}
	}
}
