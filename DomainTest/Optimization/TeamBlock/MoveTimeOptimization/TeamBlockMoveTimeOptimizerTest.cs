using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.MoveTimeOptimization
{
	[TestFixture]
	public class TeamBlockMoveTimeOptimizerTest
	{
		private MockRepository _mock;
		private ITeamBlockMoveTimeOptimizer _target;
		private IList<IScheduleMatrixPro> _matrixList;
		private IScheduleMatrixPro _matrix1;
		private SchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IOptimizationPreferences _optimizationPreferences;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private ITeamBlockMoveTimeDescisionMaker _decisionMaker;
		private IPeriodValueCalculator _periodValueCalculator;
		private DateOnly _today;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IProjectionService _projectionService1;
		private IProjectionService _projectionService2;
		private IVisualLayerCollection _visualLayerCollection1;
		private IVisualLayerCollection _visualLayerCollection2;
		private IScheduleDayPro _scheduleDayPro2;
		private ITeamBlockClearer _teamBlockClearer;
		private ITeamBlockInfoFactory _teamBlockInfoFactory;
		private ITeamBlockScheduler _teamBlockScheduler;
		private ITeamInfo _teamInfo;
		private IResourceCalculateDelayer _resourceCalulateDelayer;
		private ITeamBlockInfo _teamBlockInfo;
		private ShiftNudgeDirective _shiftNudgeDirective;
		private ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private IWorkShiftSelector _workShiftSelector;
		private readonly GroupPersonSkillAggregator groupPersonSkillAggregator = new GroupPersonSkillAggregator(new PersonalSkillsProvider());

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulingOptionsCreator = _mock.StrictMock<ISchedulingOptionsCreator>();
			_workShiftSelector = _mock.StrictMock<IWorkShiftSelector>();
			_safeRollbackAndResourceCalculation = _mock.StrictMock<ISafeRollbackAndResourceCalculation>();
			_decisionMaker = _mock.StrictMock<ITeamBlockMoveTimeDescisionMaker>();
			_teamBlockClearer = _mock.StrictMock<ITeamBlockClearer>();
			_teamBlockInfoFactory = _mock.StrictMock<ITeamBlockInfoFactory>();
			_teamBlockScheduler = _mock.StrictMock<ITeamBlockScheduler>();
			_teamBlockShiftCategoryLimitationValidator = _mock.StrictMock<ITeamBlockShiftCategoryLimitationValidator>();
			_target = new TeamBlockMoveTimeOptimizer(_schedulingOptionsCreator, _decisionMaker, _teamBlockClearer, _teamBlockInfoFactory, _teamBlockScheduler, _safeRollbackAndResourceCalculation, _teamBlockShiftCategoryLimitationValidator, _workShiftSelector, groupPersonSkillAggregator);
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixList = new List<IScheduleMatrixPro> { _matrix1 };
			_schedulingOptions = new SchedulingOptions();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingResultStateHolder = _mock.Stub<ISchedulingResultStateHolder>();
			_optimizationPreferences = new OptimizationPreferences();
			_periodValueCalculator = _mock.StrictMock<IPeriodValueCalculator>();
			_today = new DateOnly(2014, 05, 14);
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_projectionService1 = _mock.StrictMock<IProjectionService>();
			_projectionService2 = _mock.StrictMock<IProjectionService>();
			_visualLayerCollection1 = _mock.StrictMock<IVisualLayerCollection>();
			_visualLayerCollection2 = _mock.StrictMock<IVisualLayerCollection>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_shiftNudgeDirective = new ShiftNudgeDirective();
			_resourceCalulateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
		}

		[ Test]
		public void ShouldReturnFalseIfNoDatesFound()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(5);
				Expect.Call(_decisionMaker.Execute(_matrix1, _optimizationPreferences, _schedulingResultStateHolder)).Return(new List<DateOnly>());
			}
			using (_mock.Playback())
			{
				Assert.IsFalse(_target.OptimizeTeam(_optimizationPreferences, _teamInfo, _matrix1, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, _resourceCalulateDelayer));
			}
		}

		private void commonMocks(DateOnly date, IScheduleDayPro scheduleDayPro, IScheduleDay scheduleDay, IProjectionService projectionService, IVisualLayerCollection visualLayerCollection, TimeSpan contractTime)
		{
			Expect.Call(_matrix1.GetScheduleDayByKey(date)).Return(scheduleDayPro);
			Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
			
			Expect.Call(scheduleDay.ProjectionService()).Return(projectionService);
			Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
			Expect.Call(visualLayerCollection.ContractTime()).Return(contractTime);
		}

		[ Test]
		public void ShouldReturnFalseIfFoundDaysAreEqual()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(5);
				Expect.Call(_decisionMaker.Execute(_matrix1, _optimizationPreferences, _schedulingResultStateHolder)).Return(new List<DateOnly> { _today, _today });

				Expect.Call(() => _rollbackService.ClearModificationCollection());
				commonMocks(_today,_scheduleDayPro1,_scheduleDay1,_projectionService1,_visualLayerCollection1,new TimeSpan());
				commonMocks(_today, _scheduleDayPro1, _scheduleDay1, _projectionService1, _visualLayerCollection1, new TimeSpan());

			}
			using (_mock.Playback())
			{
				Assert.IsFalse(_target.OptimizeTeam(_optimizationPreferences, _teamInfo, _matrix1, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, _resourceCalulateDelayer));
			}
		}

		[ Test]
		public void ShouldReturnTrueIfHigherContractTime()
		{

				using (_mock.Record())
				{
					Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
					Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(5);
					Expect.Call(_decisionMaker.Execute(_matrix1, _optimizationPreferences, _schedulingResultStateHolder)).Return(new List<DateOnly> { _today, _today.AddDays(1) });

					Expect.Call(() => _rollbackService.ClearModificationCollection());
					commonMocks(_today, _scheduleDayPro1, _scheduleDay1, _projectionService1, _visualLayerCollection1, new TimeSpan(8));
					commonMocks(_today.AddDays(1), _scheduleDayPro2, _scheduleDay2, _projectionService2, _visualLayerCollection2, new TimeSpan(7));

					Expect.Call(_teamInfo.MatrixesForGroupAndDate(new DateOnly(2014,5,15))).Return(_matrixList);
					Expect.Call(() => _matrix1.LockDay(_today.AddDays(1)));
				}
				using (_mock.Playback())
				{
					Assert.IsTrue(_target.OptimizeTeam(_optimizationPreferences, _teamInfo, _matrix1, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, _resourceCalulateDelayer));
				}

		}

		[Test]
		public void ShouldOptimizeSuccessfully()
		{


				using (_mock.Record())
				{
					Expect.Call(() => _rollbackService.ClearModificationCollection());
					Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
					Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, new DateOnly(2014, 05, 14),
						_schedulingOptions.BlockFinder())).IgnoreArguments().Return(_teamBlockInfo);
					Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
					Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, new DateOnly(2014, 05, 15),
						_schedulingOptions.BlockFinder())).IgnoreArguments().Return(_teamBlockInfo);
					Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
					Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(5);
					Expect.Call(_decisionMaker.Execute(_matrix1, _optimizationPreferences, _schedulingResultStateHolder)).Return(new List<DateOnly> { _today, _today.AddDays(1) });
					Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, _teamBlockInfo, new DateOnly(2014, 5, 14), _schedulingOptions,
						_rollbackService, _resourceCalulateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator)).IgnoreArguments().Return(true);
					Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, _teamBlockInfo, new DateOnly(2014, 5, 15), _schedulingOptions,
						_rollbackService, _resourceCalulateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator)).IgnoreArguments().Return(true);
					
					commonMocks(_today, _scheduleDayPro1, _scheduleDay1, _projectionService1, _visualLayerCollection1, new TimeSpan(7));
					commonMocks(_today.AddDays(1), _scheduleDayPro2, _scheduleDay2, _projectionService2, _visualLayerCollection2, new TimeSpan(8));

					Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(4);
					Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo, _teamBlockInfo, _optimizationPreferences)).Return(true);
				}
				using (_mock.Playback())
				{
					Assert.IsTrue(_target.OptimizeTeam(_optimizationPreferences, _teamInfo, _matrix1, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, _resourceCalulateDelayer));
				}
		
		}

		[Test]
		public void ShouldReturnFalseIfShiftCategoryLimitationsIsBroken()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, new DateOnly(2014, 05, 14),
					_schedulingOptions.BlockFinder())).IgnoreArguments().Return(_teamBlockInfo);
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, new DateOnly(2014, 05, 15),
					_schedulingOptions.BlockFinder())).IgnoreArguments().Return(_teamBlockInfo);
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(5);
				Expect.Call(_decisionMaker.Execute(_matrix1, _optimizationPreferences, _schedulingResultStateHolder)).Return(new List<DateOnly> { _today, _today.AddDays(1) });

				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, _teamBlockInfo, new DateOnly(2014, 5, 14), _schedulingOptions,
					_rollbackService, _resourceCalulateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator)).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, _teamBlockInfo, new DateOnly(2014, 5, 15), _schedulingOptions,
					_rollbackService, _resourceCalulateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator)).IgnoreArguments().Return(true);

				commonMocks(_today, _scheduleDayPro1, _scheduleDay1, _projectionService1, _visualLayerCollection1, new TimeSpan(7));
				commonMocks(_today.AddDays(1), _scheduleDayPro2, _scheduleDay2, _projectionService2, _visualLayerCollection2, new TimeSpan(8));

				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo, _teamBlockInfo, _optimizationPreferences)).Return(false);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
			}
			using (_mock.Playback())
			{
				Assert.IsFalse(_target.OptimizeTeam(_optimizationPreferences, _teamInfo, _matrix1, _rollbackService, _periodValueCalculator,
				_schedulingResultStateHolder, _resourceCalulateDelayer));
			}
		}
	}
}
