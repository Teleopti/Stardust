using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
		private ISchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IOptimizationPreferences _optimizationPreferences;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private ITeamBlockMoveTimeDescisionMaker _decisionMaker;
		private IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private IConstructAndScheduleSingleDayTeamBlock _constructAndScheduleSingleDayTeamBlock;
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

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulingOptionsCreator = _mock.StrictMock<ISchedulingOptionsCreator>();
			_decisionMaker = _mock.StrictMock<ITeamBlockMoveTimeDescisionMaker>();
			_deleteAndResourceCalculateService = _mock.StrictMock<IDeleteAndResourceCalculateService>();
			_resourceOptimizationHelper = _mock.StrictMock<IResourceOptimizationHelper>();
			_constructAndScheduleSingleDayTeamBlock = _mock.StrictMock<IConstructAndScheduleSingleDayTeamBlock>();
			_target = new TeamBlockMoveTimeOptimizer(_schedulingOptionsCreator, _decisionMaker, _deleteAndResourceCalculateService, _resourceOptimizationHelper, _constructAndScheduleSingleDayTeamBlock);
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixList = new List<IScheduleMatrixPro> { _matrix1 };
			_schedulingOptions = new SchedulingOptions();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
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
		}

		[ Test]
		public void ShouldReturnFalseIfNoDatesFound()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(5);
				Expect.Call(_decisionMaker.Execute(_matrix1, _optimizationPreferences)).Return(new List<DateOnly>());
			}
			using (_mock.Playback())
			{
				Assert.IsFalse(_target.OptimizeMatrix(_optimizationPreferences, _matrixList, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, _matrix1));
			}
		}

		private void commonMocks(DateOnly date, IScheduleDayPro scheduleDayPro, IScheduleDay scheduleDay, IProjectionService projectionService, IVisualLayerCollection visualLayerCollection, TimeSpan contractTime)
		{
			Expect.Call(_matrix1.GetScheduleDayByKey(date)).Return(scheduleDayPro);
			Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
			Expect.Call(scheduleDay.Clone()).Return(scheduleDay);
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
				Expect.Call(_decisionMaker.Execute(_matrix1, _optimizationPreferences)).Return(new List<DateOnly> { _today, _today });

				Expect.Call(() => _rollbackService.ClearModificationCollection());
				commonMocks(_today,_scheduleDayPro1,_scheduleDay1,_projectionService1,_visualLayerCollection1,new TimeSpan());
				commonMocks(_today, _scheduleDayPro1, _scheduleDay1, _projectionService1, _visualLayerCollection1, new TimeSpan());

			}
			using (_mock.Playback())
			{
				Assert.IsFalse(_target.OptimizeMatrix(_optimizationPreferences, _matrixList, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, _matrix1));
			}
		}

		[ Test]
		public void ShouldReturnTrueIfHigherContractTime()
		{
			using (_mock.Record())
			{
				using (_mock.Record())
				{
					Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
					Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(5);
					Expect.Call(_decisionMaker.Execute(_matrix1, _optimizationPreferences)).Return(new List<DateOnly> { _today, _today.AddDays(1) });

					Expect.Call(() => _rollbackService.ClearModificationCollection());
					commonMocks(_today, _scheduleDayPro1, _scheduleDay1, _projectionService1, _visualLayerCollection1, new TimeSpan(8));
					commonMocks(_today.AddDays(1), _scheduleDayPro2, _scheduleDay2, _projectionService2, _visualLayerCollection2, new TimeSpan(7));

					Expect.Call(() => _matrix1.LockPeriod(new DateOnlyPeriod(_today.AddDays(1), _today.AddDays(1))));
				}
				using (_mock.Playback())
				{
					Assert.IsTrue(_target.OptimizeMatrix(_optimizationPreferences, _matrixList, _rollbackService, _periodValueCalculator,
						_schedulingResultStateHolder, _matrix1));
				}
			}
			using (_mock.Playback())
			{

			}
		}

		[Test]
		public void ShouldOptimizeSuccessfully()
		{
			using (_mock.Record())
			{
				using (_mock.Record())
				{
					Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
					Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(5);
					Expect.Call(_decisionMaker.Execute(_matrix1, _optimizationPreferences)).Return(new List<DateOnly> { _today, _today.AddDays(1) });

					Expect.Call(() => _rollbackService.ClearModificationCollection());
					commonMocks(_today, _scheduleDayPro1, _scheduleDay1, _projectionService1, _visualLayerCollection1, new TimeSpan(7));
					commonMocks(_today.AddDays(1), _scheduleDayPro2, _scheduleDay2, _projectionService2, _visualLayerCollection2, new TimeSpan(8));

					Expect.Call(
						_deleteAndResourceCalculateService.DeleteWithResourceCalculation(
							new List<IScheduleDay> {_scheduleDay1, _scheduleDay2}, _rollbackService, true));
					Expect.Call(_constructAndScheduleSingleDayTeamBlock.Schedule(_matrixList, _today, _matrix1, _schedulingOptions,
						_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _optimizationPreferences)).IgnoreArguments() .Return(true);
					Expect.Call(_constructAndScheduleSingleDayTeamBlock.Schedule(_matrixList, _today.AddDays(1), _matrix1, _schedulingOptions,
						_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _optimizationPreferences)).IgnoreArguments().Return(true);
					Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(4);
				}
				using (_mock.Playback())
				{
					Assert.IsTrue(_target.OptimizeMatrix(_optimizationPreferences, _matrixList, _rollbackService, _periodValueCalculator,
						_schedulingResultStateHolder, _matrix1));
				}
			}
			using (_mock.Playback())
			{

			}
		}
	}
}
