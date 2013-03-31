﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockDayOffOptimizerServiceTest
	{
		private MockRepository _mocks;
		private ITeamBlockDayOffOptimizerService _target;
		private ITeamInfoFactory _teamInfoFactory;
		private ILockableBitArrayFactory _lockableBitArrayFactory;
		private IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private ISmartDayOffBackToLegalStateService _daysOffBackToLegal;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
		private ITeamBlockScheduler _teamBlockScheduler;
		private ITeamBlockInfoFactory _teamBlockInfoFactory;
		private IPeriodValueCalculator _periodValueCalculatorForAllSkills;
		private IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private ITeamDayOffModifier _teamDayOffModifier;
		private IBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private ITeamBlockClearer _teamBlockClearer;
		private ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private IOptimizationPreferences _optimizationPreferences;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IDayOffTemplate _dayOffTemplate;
		private IScheduleMatrixPro _matrix;
		private IPerson _person;
		private IList<IScheduleMatrixPro> _matrixList;
		private IGroupPerson _groupPerson;
		private IList<IList<IScheduleMatrixPro>> _matrixes;
		private ILockableBitArray _originalArray;
		private IScheduleResultDataExtractor _dataExtractor;
		private IDayOffDecisionMaker _dayOffDecisionMaker;
		private TeamInfo _teamInfo;
		private ISchedulingOptions _schedulingOptions;
		private ITeamBlockInfo _teamBlockInfo;
		private List<IPerson> _selectedPersons;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamInfoFactory = _mocks.StrictMock<ITeamInfoFactory>();
			_lockableBitArrayFactory = _mocks.StrictMock<ILockableBitArrayFactory>();
			_scheduleResultDataExtractorProvider = _mocks.StrictMock<IScheduleResultDataExtractorProvider>();
			_daysOffBackToLegal = _mocks.StrictMock<ISmartDayOffBackToLegalStateService>();
			_schedulingOptionsCreator = _mocks.StrictMock<ISchedulingOptionsCreator>();
			_lockableBitArrayChangesTracker = _mocks.StrictMock<ILockableBitArrayChangesTracker>();
			_teamBlockScheduler = _mocks.StrictMock<ITeamBlockScheduler>();
			_teamBlockInfoFactory = _mocks.StrictMock<ITeamBlockInfoFactory>();
			_periodValueCalculatorForAllSkills = _mocks.StrictMock<IPeriodValueCalculator>();
			_dayOffOptimizationDecisionMakerFactory = _mocks.StrictMock<IDayOffOptimizationDecisionMakerFactory>();
			_safeRollbackAndResourceCalculation = _mocks.StrictMock<ISafeRollbackAndResourceCalculation>();
			_teamDayOffModifier = _mocks.StrictMock<ITeamDayOffModifier>();
			_teamBlockSteadyStateValidator = _mocks.StrictMock<IBlockSteadyStateValidator>();
			_teamBlockClearer = _mocks.StrictMock<ITeamBlockClearer>();
			_restrictionOverLimitValidator = _mocks.StrictMock<ITeamBlockRestrictionOverLimitValidator>();
			_optimizationPreferences = new OptimizationPreferences();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_dayOffTemplate = new DayOffTemplate(new Description("hej"));
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			_matrixList = new List<IScheduleMatrixPro> {_matrix};
			_groupPerson = new GroupPerson(new List<IPerson>{_person}, DateOnly.MinValue, "hej", null);
			_matrixes = new List<IList<IScheduleMatrixPro>>();
			_matrixes.Add(_matrixList);
			_originalArray = new LockableBitArray(2, false, false, null);
			_originalArray.Set(1, true);
			_dataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
			_dayOffDecisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
			_teamInfo = new TeamInfo(_groupPerson, _matrixes);
			_schedulingOptions = new SchedulingOptions();
			_teamBlockInfo = new TeamBlockInfo(_teamInfo,
			                                   new BlockInfo(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1))));
			_selectedPersons = new List<IPerson> {_person};

			_target = new TeamBlockDayOffOptimizerService(_teamInfoFactory, _lockableBitArrayFactory,
			                                              _scheduleResultDataExtractorProvider, _daysOffBackToLegal,
			                                              _schedulingOptionsCreator, _lockableBitArrayChangesTracker,
			                                              _teamBlockScheduler, _teamBlockInfoFactory,
			                                              _periodValueCalculatorForAllSkills,
			                                              _dayOffOptimizationDecisionMakerFactory,
			                                              _safeRollbackAndResourceCalculation, _teamDayOffModifier,
			                                              _teamBlockSteadyStateValidator, _teamBlockClearer,
			                                              _restrictionOverLimitValidator);
		}

		[Test]
		public void ShouldRunUntilEqualOrHigherPeriodValue()
		{

			using (_mocks.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList))
				      .Return(_teamInfo);
				
				//round1
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks();
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				//round2
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks();
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList, 
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), 
					_selectedPersons, _optimizationPreferences, _rollbackService, 
					_dayOffTemplate);
			}
      
      

		}

		private void runOneMatrixMocks()
		{
			Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(_optimizationPreferences.DaysOff.ConsiderWeekBefore,
																	   _optimizationPreferences.DaysOff.ConsiderWeekAfter, _matrix))
					  .Return(_originalArray);

			tryFindMoveMocks();
			Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_originalArray, _originalArray, _matrix,
			                                                         _optimizationPreferences.DaysOff.ConsiderWeekBefore))
			      .IgnoreArguments()
			      .Return(new List<DateOnly> {DateOnly.MinValue});
			Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_originalArray, _originalArray, _matrix,
			                                                           _optimizationPreferences.DaysOff.ConsiderWeekBefore))
			      .IgnoreArguments()
				  .Return(new List<DateOnly> { DateOnly.MinValue.AddDays(1) });

			Expect.Call(
				() =>
				_teamDayOffModifier.RemoveDayOffAndResourceCalculate(_rollbackService, _teamInfo, DateOnly.MinValue.AddDays(1),
				                                                     _schedulingOptions));
			Expect.Call(
				() =>
				_teamDayOffModifier.AddDayOffAndResourceCalculate(_rollbackService, _teamInfo, DateOnly.MinValue,
																	 _schedulingOptions));
			Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1),
			                                                      _schedulingOptions.BlockFinderTypeForAdvanceScheduling))
			      .Return(_teamBlockInfo);

			Expect.Call(_teamBlockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).Return(true);
			Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions,
			                                                     new DateOnlyPeriod(DateOnly.MinValue,
			                                                                        DateOnly.MinValue.AddDays(1)),
			                                                     _selectedPersons)).Return(true);

		}

		private void tryFindMoveMocks()
		{
			Expect.Call(_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(_matrix,
																								  _optimizationPreferences.Advanced))
					  .Return(_dataExtractor);
			Expect.Call(_dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(_originalArray, _optimizationPreferences))
				  .Return(new List<IDayOffDecisionMaker> { _dayOffDecisionMaker });
			IList<double?> dataExtractorValues = new List<double?>();
			Expect.Call(_dataExtractor.Values()).Return(dataExtractorValues);
			ILockableBitArray workingArray = (ILockableBitArray)_originalArray.Clone();
			Expect.Call(_dayOffDecisionMaker.Execute(workingArray, dataExtractorValues)).IgnoreArguments().Return(true);
			List<IDayOffBackToLegalStateSolver> solverList = new List<IDayOffBackToLegalStateSolver>();
			Expect.Call(_daysOffBackToLegal.BuildSolverList(workingArray)).IgnoreArguments().Return(solverList);
			Expect.Call(_daysOffBackToLegal.Execute(solverList, 100)).Return(true);

		}
	}
}