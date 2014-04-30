using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockIntradayOptimizationServiceTest
	{
		private ITeamBlockScheduler _teamBlockScheduler;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private ITeamBlockClearer _teamBlockClearer;
		private ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private MockRepository _mocks;
		private ITeamBlockIntradayOptimizationService _target;
		private ITeamBlockGenerator _teamBlockGenerator;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
	    private IDailyTargetValueCalculatorForTeamBlock _dailyTargetValueCalculatorForTeamBlock;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private int _reportedProgress;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamBlockGenerator = _mocks.StrictMock<ITeamBlockGenerator>();
			_teamBlockScheduler = _mocks.StrictMock<ITeamBlockScheduler>();
			_schedulingOptionsCreator = _mocks.StrictMock<ISchedulingOptionsCreator>();
			_safeRollbackAndResourceCalculation = _mocks.StrictMock<ISafeRollbackAndResourceCalculation>();
			_teamBlockIntradayDecisionMaker = _mocks.StrictMock<ITeamBlockIntradayDecisionMaker>();
			_teamBlockClearer = _mocks.StrictMock<ITeamBlockClearer>();
			_restrictionOverLimitValidator = _mocks.StrictMock<ITeamBlockRestrictionOverLimitValidator>();
			_schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockMaxSeatChecker = _mocks.StrictMock<ITeamBlockMaxSeatChecker>();
	        _dailyTargetValueCalculatorForTeamBlock = _mocks.StrictMock<IDailyTargetValueCalculatorForTeamBlock>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_target = new TeamBlockIntradayOptimizationService(_teamBlockGenerator, _teamBlockScheduler,
			                                                   _schedulingOptionsCreator, 
			                                                   _safeRollbackAndResourceCalculation,
			                                                   _teamBlockIntradayDecisionMaker, _restrictionOverLimitValidator,
			                                                   _teamBlockClearer, _teamBlockMaxSeatChecker,_dailyTargetValueCalculatorForTeamBlock);
		    _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
		    _reportedProgress = 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldOptimizeTeamBlockIntraday()
		{
			var dateOnly = new DateOnly();
			var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> {matrix1, matrix2};
			var selectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson("Bill");
			var persons = new List<IPerson> {person};
			var schedulingOptions = new SchedulingOptions();
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> {matrixes};
			var group = new Group(new List<IPerson> {person}, "Hej");
			var teaminfo = new TeamInfo(group, groupMatrixList);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			var optimizationPreferences = new OptimizationPreferences();
			var teamBlocks = new List<ITeamBlockInfo> {teamBlockInfo};
			using (_mocks.Record())
			{
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences)).Return(schedulingOptions);
				Expect.Call(_teamBlockGenerator.Generate(matrixes, selectedPeriod, persons, schedulingOptions))
				      .Return(teamBlocks);
				Expect.Call(_teamBlockIntradayDecisionMaker.Decide(teamBlocks, optimizationPreferences,
				                                                   schedulingOptions)).Return(teamBlocks);
				Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection());
				Expect.Call(
					() => _teamBlockClearer.ClearTeamBlock(schedulingOptions, _schedulePartModifyAndRollbackService, teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions,
					persons, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective()))
					.IgnoreArguments()
					.Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(dateOnly, schedulingOptions)).Return(true);
				Expect.Call(_restrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences))
				      .Return(true);
			    Expect.Call(_dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced))
			          .Return(0.5).Repeat.Twice() ;
                Expect.Call(()=>_safeRollbackAndResourceCalculation.Execute(_schedulePartModifyAndRollbackService, schedulingOptions));
			}
			using (_mocks.Playback())
			{
				_target.Optimize(matrixes, selectedPeriod, persons, optimizationPreferences,
								 _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldShouldReschedulingFailed()
		{
			var dateOnly = new DateOnly();
            var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
            var matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> {matrix1, matrix2};
			var selectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson("Bill");
			var persons = new List<IPerson> {person};
			var schedulingOptions = new SchedulingOptions();
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> {matrixes};
			var group = new Group(new List<IPerson> { person }, "Hej");
			var teaminfo = new TeamInfo(group, groupMatrixList);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			var optimizationPreferences = new OptimizationPreferences();
			var teamBlocks = new List<ITeamBlockInfo> {teamBlockInfo};
			using (_mocks.Record())
			{
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences)).Return(schedulingOptions);
				Expect.Call(_teamBlockGenerator.Generate(matrixes, selectedPeriod, persons, schedulingOptions))
				      .Return(teamBlocks);
				Expect.Call(_teamBlockIntradayDecisionMaker.Decide(teamBlocks, optimizationPreferences,
				                                                   schedulingOptions)).Return(teamBlocks);
				Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection());
				Expect.Call(
					() => _teamBlockClearer.ClearTeamBlock(schedulingOptions, _schedulePartModifyAndRollbackService, teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions,
					persons, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective()))
					.IgnoreArguments()
					.Return(false);
                Expect.Call(_dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced))
                      .Return(5.0);
                Expect.Call(()=>_safeRollbackAndResourceCalculation.Execute(_schedulePartModifyAndRollbackService, schedulingOptions));
			}
			using (_mocks.Playback())
			{
				_target.Optimize(matrixes, selectedPeriod, persons, optimizationPreferences,
								 _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}
		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldRollbackIfValidationFailed()
		{
			var dateOnly = new DateOnly();
			var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { matrix1, matrix2 };
			var selectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson("Bill");
			var persons = new List<IPerson> { person };
			var schedulingOptions = new SchedulingOptions();
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			var group = new Group(new List<IPerson> { person }, "Hej");
			var teaminfo = new TeamInfo(group, groupMatrixList);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			var optimizationPreferences = new OptimizationPreferences();
			var teamBlocks = new List<ITeamBlockInfo> { teamBlockInfo };
			using (_mocks.Record())
			{
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences)).Return(schedulingOptions);
				Expect.Call(_teamBlockGenerator.Generate(matrixes, selectedPeriod, persons, schedulingOptions))
					  .Return(teamBlocks);
				Expect.Call(_teamBlockIntradayDecisionMaker.Decide(teamBlocks, optimizationPreferences,
																   schedulingOptions)).Return(teamBlocks);
				Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection());
				Expect.Call(
					() => _teamBlockClearer.ClearTeamBlock(schedulingOptions, _schedulePartModifyAndRollbackService, teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions,
					persons, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective()))
					.IgnoreArguments()
					.Return(true);
				Expect.Call(_restrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences))
					  .Return(false);
                Expect.Call(_dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced))
                      .Return(5.0);
                Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(dateOnly, schedulingOptions)).Return(true);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_schedulePartModifyAndRollbackService, schedulingOptions));
			}
			using (_mocks.Playback())
			{
				_target.Optimize(matrixes, selectedPeriod, persons, optimizationPreferences,
								 _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReportProgress()
		{
			var dateOnly = new DateOnly();
			var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { matrix1, matrix2 };
			var selectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson("Bill");
			var persons = new List<IPerson> { person };
			var schedulingOptions = new SchedulingOptions();
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			var group = new Group(new List<IPerson> { person }, "Hej");
			var teaminfo = new TeamInfo(group, groupMatrixList);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			var optimizationPreferences = new OptimizationPreferences();
			var teamBlocks = new List<ITeamBlockInfo> { teamBlockInfo };
			_target.ReportProgress += targetReportProgress;
			using (_mocks.Record())
			{
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences)).Return(schedulingOptions);
				Expect.Call(_teamBlockGenerator.Generate(matrixes, selectedPeriod, persons, schedulingOptions))
					  .Return(teamBlocks);
			}
			using (_mocks.Playback())
			{
				_target.Optimize(matrixes, selectedPeriod, persons, optimizationPreferences,
								 _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
				_target.ReportProgress -= targetReportProgress;
			}
		}

		[Test]
		public void ShouldReportProgressWhenNotSuccessful()
		{
			var dateOnly = new DateOnly();
			var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { matrix1, matrix2 };
			var selectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson("Bill");
			var persons = new List<IPerson> { person };
			var schedulingOptions = new SchedulingOptions();
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			var group = new Group(new List<IPerson> { person }, "Hej");
			var teaminfo = new TeamInfo(group, groupMatrixList);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			var optimizationPreferences = new OptimizationPreferences();
			var teamBlocks = new List<ITeamBlockInfo> { teamBlockInfo };
			
			using (_mocks.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences)).Return(schedulingOptions);
				Expect.Call(_teamBlockGenerator.Generate(matrixes, selectedPeriod, persons, schedulingOptions)).Return(teamBlocks);
				Expect.Call(_teamBlockIntradayDecisionMaker.Decide(teamBlocks, optimizationPreferences,schedulingOptions)).Return(teamBlocks);
				Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection());
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(schedulingOptions, _schedulePartModifyAndRollbackService, teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions,
					persons, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder,
					new ShiftNudgeDirective())).IgnoreArguments().Return(true);
				Expect.Call(_restrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences)).Return(false);
				Expect.Call(_dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced)).Return(5.0);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(dateOnly, schedulingOptions)).Return(true);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_schedulePartModifyAndRollbackService, schedulingOptions));
			}
			using (_mocks.Playback())
			{
				_target.ReportProgress += targetReportProgressNotSuccessful;
				_target.Optimize(matrixes, selectedPeriod, persons, optimizationPreferences, _schedulePartModifyAndRollbackService,
					_resourceCalculateDelayer, _schedulingResultStateHolder);
				_target.ReportProgress -= targetReportProgressNotSuccessful;
				Assert.AreEqual(2, _reportedProgress);
			}
		}

		void targetReportProgressNotSuccessful(object sender, ResourceOptimizerProgressEventArgs e)
		{
			_reportedProgress++;
		}

		void targetReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}

	}
}
