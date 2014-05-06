using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
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
		private ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
		private ITeamBlockScheduler _teamBlockScheduler;
		private ITeamBlockInfoFactory _teamBlockInfoFactory;
		private IPeriodValueCalculator _periodValueCalculatorForAllSkills;
		private ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private ITeamDayOffModifier _teamDayOffModifier;
		private ITeamBlockSteadyStateValidator _teamTeamBlockSteadyStateValidator;
		private ITeamBlockClearer _teamBlockClearer;
		private ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private IOptimizationPreferences _optimizationPreferences;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IScheduleMatrixPro _matrix;
		private IPerson _person;
		private IList<IScheduleMatrixPro> _matrixList;
		private IGroupPerson _groupPerson;
		private IList<IList<IScheduleMatrixPro>> _matrixes;
		private ILockableBitArray _originalArray;
		private ILockableBitArray _workingArray;
		private TeamInfo _teamInfo;
		private ISchedulingOptions _schedulingOptions;
		private ITeamBlockInfo _teamBlockInfo;
		private List<IPerson> _selectedPersons;
		private IVirtualSchedulePeriod _schedulePeriod;
		private ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private ITeamBlockDaysOffMoveFinder _teamBlockDaysOffMoveFinder;
	    private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
            _teamBlockSchedulingOptions = new TeamBlockSchedulingOptions();
			_teamBlockDaysOffMoveFinder = _mocks.StrictMock<ITeamBlockDaysOffMoveFinder>();
			_teamInfoFactory = _mocks.StrictMock<ITeamInfoFactory>();
			_lockableBitArrayFactory = _mocks.StrictMock<ILockableBitArrayFactory>();
			_lockableBitArrayChangesTracker = _mocks.StrictMock<ILockableBitArrayChangesTracker>();
			_teamBlockScheduler = _mocks.StrictMock<ITeamBlockScheduler>();
			_teamBlockInfoFactory = _mocks.StrictMock<ITeamBlockInfoFactory>();
			_periodValueCalculatorForAllSkills = _mocks.StrictMock<IPeriodValueCalculator>();
			_safeRollbackAndResourceCalculation = _mocks.StrictMock<ISafeRollbackAndResourceCalculation>();
			_teamDayOffModifier = _mocks.StrictMock<ITeamDayOffModifier>();
			_teamTeamBlockSteadyStateValidator = _mocks.StrictMock<ITeamBlockSteadyStateValidator>();
			_teamBlockClearer = _mocks.StrictMock<ITeamBlockClearer>();
			_restrictionOverLimitValidator = _mocks.StrictMock<ITeamBlockRestrictionOverLimitValidator>();
			_optimizationPreferences = new OptimizationPreferences();
			_optimizationPreferences.Extra.UseTeamSameDaysOff = true;
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockMaxSeatChecker = _mocks.StrictMock<ITeamBlockMaxSeatChecker>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			_matrixList = new List<IScheduleMatrixPro> {_matrix};
			_groupPerson = new GroupPerson(new List<IPerson>{_person}, DateOnly.MinValue, "hej", null);
			_matrixes = new List<IList<IScheduleMatrixPro>>();
			_matrixes.Add(_matrixList);
			_originalArray = new LockableBitArray(2, false, false, null);
			_originalArray.Set(1, true);
			_workingArray = new LockableBitArray(2, false, false, null);
			_workingArray.Set(0, true);
			_teamInfo = new TeamInfo(_groupPerson, _matrixes);
			_schedulingOptions = new SchedulingOptions();
			_teamBlockInfo = new TeamBlockInfo(_teamInfo,
			                                   new BlockInfo(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1))));
			_selectedPersons = new List<IPerson> {_person};
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			_target = new TeamBlockDayOffOptimizerService(_teamInfoFactory, _lockableBitArrayFactory, _lockableBitArrayChangesTracker,
			                                              _teamBlockScheduler, _teamBlockInfoFactory,
			                                              _periodValueCalculatorForAllSkills,
			                                              _safeRollbackAndResourceCalculation, _teamDayOffModifier,
			                                              _teamTeamBlockSteadyStateValidator, _teamBlockClearer,
														  _restrictionOverLimitValidator, _teamBlockMaxSeatChecker,
                                                          _teamBlockDaysOffMoveFinder, _teamBlockSchedulingOptions);
		}

		[Test]
		public void ShouldRunUntilEqualOrHigherPeriodValue()
		{

			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList))
				      .Return(_teamInfo);
				
				//round1
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				//round2
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList, 
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), 
					_selectedPersons, _optimizationPreferences, _rollbackService, 
					_schedulingOptions);
			}
		}


		[Test]
		public void ShouldOptimizeWithFreeDayOffs()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList)).Return(_teamInfo);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(_optimizationPreferences.DaysOff.ConsiderWeekBefore, _optimizationPreferences.DaysOff.ConsiderWeekAfter, _matrix)).Return(_originalArray);
				Expect.Call(_teamBlockDaysOffMoveFinder.TryFindMoves(_matrix, _originalArray, _optimizationPreferences)).Return(_workingArray);
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_originalArray, _workingArray, _matrix,_optimizationPreferences.DaysOff.ConsiderWeekBefore)).IgnoreArguments().Return(new List<DateOnly> { DateOnly.MinValue });
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_originalArray, _workingArray, _matrix,_optimizationPreferences.DaysOff.ConsiderWeekBefore)).IgnoreArguments().Return(new List<DateOnly> { DateOnly.MinValue.AddDays(1) });
				Expect.Call(() =>_teamDayOffModifier.RemoveDayOffForMember(_rollbackService,_person,DateOnly.MinValue.AddDays(1)));
				Expect.Call(() =>_teamDayOffModifier.AddDayOffForMember(_rollbackService, _person, DateOnly.MinValue,_schedulingOptions.DayOffTemplate, true));
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1),_schedulingOptions.BlockFinderTypeForAdvanceScheduling, false, _matrixList)).Return(_teamBlockInfo);
				Expect.Call(_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).Return(false);
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions,new DateOnlyPeriod(DateOnly.MinValue,DateOnly.MinValue.AddDays(1)),_selectedPersons, _rollbackService)).Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue, _schedulingOptions)).Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue.AddDays(1), _schedulingOptions)).Return(true);
				Expect.Call(_restrictionOverLimitValidator.Validate(_teamInfo, _optimizationPreferences)).Return(true);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
			}

			using (_mocks.Playback())
			{
				_optimizationPreferences.Extra.UseTeamSameDaysOff = false;
				_target.OptimizeDaysOff(_matrixList, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _selectedPersons, _optimizationPreferences, _rollbackService, _schedulingOptions);
			}
		}

		[Test]
		public void ShouldCancelOptimization()
		{
			_target.ReportProgress += _target_ReportProgress;
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList))
					  .Return(_teamInfo);

				//round1
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, true);
				
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
				
			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList, 
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), 
					_selectedPersons, _optimizationPreferences, _rollbackService, 
					_schedulingOptions);
				_target.ReportProgress -= _target_ReportProgress;
			}
		}

		void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}
		
		[Test]
		public void ShouldRunUntilEqualOrHigherPeriodValueButRollbackAndLockInMatrixesIfRestrictionValidatorFails()
		{

			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList))
					  .Return(_teamInfo);

				//round1
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(true, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				//round2
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions);
			}
		}

		[Test]
		public void ShouldClearBlockIfInconsistent()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList))
					  .Return(_teamInfo);

				//round1
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(true, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				//round2
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions);
			}
		}

		[Test]
		public void ShouldRollbackIfReschedulingFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList))
					  .Return(_teamInfo);

				//round1
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, true, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions);
			}
		}

		[Test]
		public void ShouldPutDaysOffBackToLegalIfDecisionMakerFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList))
					  .Return(_teamInfo);

				//round1
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, true, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions);
			}
		}

		[Test]
		public void ShouldBailOutIfDecisionMakerFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList))
					  .Return(_teamInfo);

				//round1
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, true, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions);
			}
		}


		private void runOneMatrixMocks(bool failOnRestrictionOverLimitValidator, bool failOnRescheduling, bool failOnNoMoveFound, bool failOnCancel)
		{
			Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(_optimizationPreferences.DaysOff.ConsiderWeekBefore,
																	   _optimizationPreferences.DaysOff.ConsiderWeekAfter, _matrix))
					  .Return(_originalArray);

			ILockableBitArray arrayToReturn = _workingArray;
			if (failOnNoMoveFound)
				arrayToReturn = _originalArray;

			Expect.Call(_teamBlockDaysOffMoveFinder.TryFindMoves(_matrix, _originalArray, _optimizationPreferences))
				  .Return(arrayToReturn);

			if (failOnNoMoveFound)
				return;


			//Expect.Call(_matrix.Person).Return(_person);

			Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_originalArray, _workingArray, _matrix,
			                                                         _optimizationPreferences.DaysOff.ConsiderWeekBefore))
			      .IgnoreArguments()
			      .Return(new List<DateOnly> {DateOnly.MinValue});
			Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_originalArray, _workingArray, _matrix,
			                                                           _optimizationPreferences.DaysOff.ConsiderWeekBefore))
			      .IgnoreArguments()
				  .Return(new List<DateOnly> { DateOnly.MinValue.AddDays(1) });


			

			Expect.Call(
				() =>
				_teamDayOffModifier.RemoveDayOffForTeam(_rollbackService, _teamInfo, DateOnly.MinValue.AddDays(1)));
			Expect.Call(
				() =>
				_teamDayOffModifier.AddDayOffForTeamAndResourceCalculate(_rollbackService, _teamInfo, DateOnly.MinValue, _schedulingOptions.DayOffTemplate)).IgnoreArguments();
			Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1),
																  _schedulingOptions.BlockFinderTypeForAdvanceScheduling, false, _matrixList))
			      .Return(_teamBlockInfo);

			Expect.Call(_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).Return(false);
			Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
			Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions,
			                                                     new DateOnlyPeriod(DateOnly.MinValue,
			                                                                        DateOnly.MinValue.AddDays(1)),
																 _selectedPersons, _rollbackService)).Return(!failOnRescheduling);
			if (failOnRescheduling)
				return;

			

			Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue, _schedulingOptions)).Return(true);
			Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue.AddDays(1), _schedulingOptions)).Return(true);

			Expect.Call(_restrictionOverLimitValidator.Validate(_teamInfo, _optimizationPreferences)).Return(!failOnRestrictionOverLimitValidator);

			if (failOnCancel)
				return;

			if (!failOnRestrictionOverLimitValidator)
				return;
			
			Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));

			Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
			Expect.Call(_schedulePeriod.DateOnlyPeriod)
			      .Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)));
			Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
			Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
			Expect.Call(_schedulePeriod.DateOnlyPeriod)
				  .Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)));
			Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue.AddDays(1), DateOnly.MinValue.AddDays(1))));
		}
	}
}