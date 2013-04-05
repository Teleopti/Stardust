﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
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
			_target = new TeamBlockIntradayOptimizationService(_teamBlockGenerator, _teamBlockScheduler,
			                                                   _schedulingOptionsCreator, 
			                                                   _safeRollbackAndResourceCalculation,
			                                                   _teamBlockIntradayDecisionMaker, _restrictionOverLimitValidator,
			                                                   _teamBlockClearer);
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
			var groupPerson = new GroupPerson(new List<IPerson> {person}, DateOnly.MinValue, "Hej", null);
			var teaminfo = new TeamInfo(groupPerson, groupMatrixList);
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
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, selectedPeriod,
				                                                     persons))
				      .Return(true);
				Expect.Call(_restrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences))
				      .Return(true);
				Expect.Call(_teamBlockIntradayDecisionMaker.RecalculateTeamBlock(teamBlockInfo, optimizationPreferences,
				                                                                 schedulingOptions)).Return(teamBlockInfo);
				Expect.Call(()=>_safeRollbackAndResourceCalculation.Execute(_schedulePartModifyAndRollbackService, schedulingOptions));
			}
			using (_mocks.Playback())
			{
				_target.Optimize(matrixes, selectedPeriod, persons, optimizationPreferences,
				                 _schedulePartModifyAndRollbackService);
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
			var groupPerson = new GroupPerson(new List<IPerson> {person}, DateOnly.MinValue, "Hej", null);
			var teaminfo = new TeamInfo(groupPerson, groupMatrixList);
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
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, selectedPeriod,
				                                                     persons))
				      .Return(false);
				Expect.Call(()=>_safeRollbackAndResourceCalculation.Execute(_schedulePartModifyAndRollbackService, schedulingOptions));
			}
			using (_mocks.Playback())
			{
				_target.Optimize(matrixes, selectedPeriod, persons, optimizationPreferences,
				                 _schedulePartModifyAndRollbackService);
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
			var groupPerson = new GroupPerson(new List<IPerson> { person }, DateOnly.MinValue, "Hej", null);
			var teaminfo = new TeamInfo(groupPerson, groupMatrixList);
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
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, selectedPeriod,
																	 persons))
					  .Return(true);
				Expect.Call(_restrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences))
					  .Return(false);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_schedulePartModifyAndRollbackService, schedulingOptions));
			}
			using (_mocks.Playback())
			{
				_target.Optimize(matrixes, selectedPeriod, persons, optimizationPreferences,
								 _schedulePartModifyAndRollbackService);
			}
		}

		[Test]
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
			var groupPerson = new GroupPerson(new List<IPerson> { person }, DateOnly.MinValue, "Hej", null);
			var teaminfo = new TeamInfo(groupPerson, groupMatrixList);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			var optimizationPreferences = new OptimizationPreferences();
			var teamBlocks = new List<ITeamBlockInfo> { teamBlockInfo };
			_target.ReportProgress += _target_ReportProgress;
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
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, selectedPeriod,
																	 persons))
					  .Return(true);
				Expect.Call(_restrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences))
					  .Return(true);
				Expect.Call(_teamBlockIntradayDecisionMaker.RecalculateTeamBlock(teamBlockInfo, optimizationPreferences,
																				 schedulingOptions)).Return(teamBlockInfo);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_schedulePartModifyAndRollbackService, schedulingOptions));
			}
			using (_mocks.Playback())
			{
				_target.Optimize(matrixes, selectedPeriod, persons, optimizationPreferences,
								 _schedulePartModifyAndRollbackService);
				_target.ReportProgress -= _target_ReportProgress;
			}
		}

		void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}
	}
}
