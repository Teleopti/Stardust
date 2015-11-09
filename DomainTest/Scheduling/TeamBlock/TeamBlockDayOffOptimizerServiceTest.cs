using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
		private ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private IOptimizationPreferences _optimizationPreferences;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IScheduleMatrixPro _matrix;
		private IPerson _person;
		private IList<IScheduleMatrixPro> _matrixList;
		private Group _group;
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
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IAllTeamMembersInSelectionSpecification _allTeamMembersInSelectionSpecification;
		private ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private ITeamBlockDayOffsInPeriodValidator _teamBlockDayOffsInPeriodValidator;
		private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;
		private IDaysOffPreferences _daysOffPreferences;
		private IScheduleDayPro _scheduleDayPro;

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
			_teamBlockOptimizationLimits = _mocks.StrictMock<ITeamBlockOptimizationLimits>();
			_optimizationPreferences = new OptimizationPreferences();
			_optimizationPreferences.Extra.UseTeamSameDaysOff = true;
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockMaxSeatChecker = _mocks.StrictMock<ITeamBlockMaxSeatChecker>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			_matrixList = new List<IScheduleMatrixPro> {_matrix};
			_group = new Group(new List<IPerson>{_person}, "hej");
			_matrixes = new List<IList<IScheduleMatrixPro>>();
			_matrixes.Add(_matrixList);
			_originalArray = new LockableBitArray(2, false, false, null);
			_originalArray.Set(1, true);
			_workingArray = new LockableBitArray(2, false, false, null);
			_workingArray.Set(0, true);
			_teamInfo = new TeamInfo(_group, _matrixes);
			_schedulingOptions = new SchedulingOptions();
			_teamBlockInfo = new TeamBlockInfo(_teamInfo,
			                                   new BlockInfo(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1))));
			_selectedPersons = new List<IPerson> {_person};
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_teamBlockShiftCategoryLimitationValidator = _mocks.StrictMock<ITeamBlockShiftCategoryLimitationValidator>();
			_teamBlockDayOffsInPeriodValidator = _mocks.StrictMock<ITeamBlockDayOffsInPeriodValidator>();
			_allTeamMembersInSelectionSpecification = _mocks.StrictMock<IAllTeamMembersInSelectionSpecification>();
			_target = new TeamBlockDayOffOptimizerService(_teamInfoFactory, _lockableBitArrayFactory, _lockableBitArrayChangesTracker,
			                                              _teamBlockScheduler, _teamBlockInfoFactory,
			                                              _periodValueCalculatorForAllSkills,
			                                              _safeRollbackAndResourceCalculation, _teamDayOffModifier,
			                                              _teamTeamBlockSteadyStateValidator, _teamBlockClearer,
														  _teamBlockOptimizationLimits, _teamBlockMaxSeatChecker,
														  _teamBlockDaysOffMoveFinder, _teamBlockSchedulingOptions, _allTeamMembersInSelectionSpecification, 
														  _teamBlockShiftCategoryLimitationValidator,
														  _teamBlockDayOffsInPeriodValidator);
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();

			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(_daysOffPreferences);
			_scheduleDayPro = new ScheduleDayPro(new DateOnly(2015,1,1), _matrix);
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
				runOneMatrixMocks(false, false, false, false,false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));

				//round2
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));

			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList, 
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), 
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
			}
		}

		[Test]
		public void ShouldExcludePersonWhoAreNotInSelection()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList)).Return(_teamInfo);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(_matrix.Person).Return(_person ) ;
				Expect.Call(_matrix.Person).Return(PersonFactory.CreatePerson("test") ) ;
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(_daysOffPreferences.ConsiderWeekBefore,
					_daysOffPreferences.ConsiderWeekAfter, _matrix)).Return(_originalArray);
				Expect.Call(_teamBlockDaysOffMoveFinder.TryFindMoves(_matrix, _originalArray, _optimizationPreferences, _daysOffPreferences)).Return(_workingArray);
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_originalArray, _workingArray, _matrix,
					_daysOffPreferences.ConsiderWeekBefore))
					.IgnoreArguments()
					.Return(new List<DateOnly> { DateOnly.MinValue });
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_originalArray, _workingArray, _matrix,
					_daysOffPreferences.ConsiderWeekBefore))
					.IgnoreArguments()
					.Return(new List<DateOnly> { DateOnly.MinValue.AddDays(1) });
				Expect.Call(() => _teamDayOffModifier.RemoveDayOffForMember(_rollbackService, _person, DateOnly.MinValue.AddDays(1))).IgnoreArguments()  ;
				Expect.Call(() => _teamDayOffModifier.AddDayOffForMember(_rollbackService, _person, DateOnly.MinValue, _schedulingOptions.DayOffTemplate, true)).IgnoreArguments() ;
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1),
					_schedulingOptions.BlockFinderTypeForAdvanceScheduling, false)).Return(_teamBlockInfo);
				Expect.Call(_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).Return(false);
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, DateOnly.MinValue.AddDays(1),
					_schedulingOptions,
					_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue, _schedulingOptions)).Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue.AddDays(1), _schedulingOptions)).Return(true);
				Expect.Call(_teamBlockOptimizationLimits.Validate(_teamInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(_teamInfo)).Return(true);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1),_schedulingOptions.BlockFinderTypeForAdvanceScheduling, false)).Return(_teamBlockInfo);
				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo, null, _optimizationPreferences)).Return(true);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}

			using (_mocks.Playback())
			{
				_optimizationPreferences.Extra.UseTeamSameDaysOff  = false;
				_target.OptimizeDaysOff(_matrixList, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _selectedPersons, _optimizationPreferences, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
			}
		}

		[Test]
		public void ShouldExcludeTeamThatIsNotFullySelected()
		{
			var teamInfo = _mocks.StrictMock<ITeamInfo>();
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList)).Return(teamInfo);
				Expect.Call(_allTeamMembersInSelectionSpecification.IsSatifyBy(teamInfo, _selectedPersons)).Return(false);
			}

			using (_mocks.Playback())
			{
				IOptimizationPreferences optimizationPreferences = new OptimizationPreferences();
				optimizationPreferences.Extra.UseTeamBlockOption = true;
				optimizationPreferences.Extra.UseTeamSameDaysOff = true;
				_target.OptimizeDaysOff(_matrixList, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _selectedPersons, optimizationPreferences, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
			}
		}


		[Test]
		public void ShouldOptimizeWithFreeDayOffs()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList)).Return(_teamInfo);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(_matrix.Person).Return(_person).Repeat.Twice();
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(_daysOffPreferences.ConsiderWeekBefore,
					_daysOffPreferences.ConsiderWeekAfter, _matrix)).Return(_originalArray);
				Expect.Call(_teamBlockDaysOffMoveFinder.TryFindMoves(_matrix, _originalArray, _optimizationPreferences, _daysOffPreferences)).Return(_workingArray);
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_originalArray, _workingArray, _matrix,
					_daysOffPreferences.ConsiderWeekBefore))
					.IgnoreArguments()
					.Return(new List<DateOnly> {DateOnly.MinValue});
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_originalArray, _workingArray, _matrix,
					_daysOffPreferences.ConsiderWeekBefore))
					.IgnoreArguments()
					.Return(new List<DateOnly> {DateOnly.MinValue.AddDays(1)});
				Expect.Call(() =>_teamDayOffModifier.RemoveDayOffForMember(_rollbackService,_person,DateOnly.MinValue.AddDays(1)));
				Expect.Call(() =>_teamDayOffModifier.AddDayOffForMember(_rollbackService, _person, DateOnly.MinValue,_schedulingOptions.DayOffTemplate, true));
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1),
					_schedulingOptions.BlockFinderTypeForAdvanceScheduling, false)).Return(_teamBlockInfo);
				Expect.Call(_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).Return(false);
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, DateOnly.MinValue.AddDays(1),
					_schedulingOptions, 
					_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue, _schedulingOptions)).Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue.AddDays(1), _schedulingOptions)).Return(true);
				Expect.Call(_teamBlockOptimizationLimits.Validate(_teamInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(_teamInfo)).Return(true);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1),_schedulingOptions.BlockFinderTypeForAdvanceScheduling, false)).Return(_teamBlockInfo);
				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo, null, _optimizationPreferences)).Return(true);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}

			using (_mocks.Playback())
			{
				_optimizationPreferences.Extra.UseTeamSameDaysOff = false;
				_target.OptimizeDaysOff(_matrixList, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _selectedPersons, _optimizationPreferences, _rollbackService, _schedulingOptions, _resourceCalculateDelayer,_schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
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
				runOneMatrixMocks(false, false, false, false, false);
				
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
				
			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList, 
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), 
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
				_target.ReportProgress -= _target_ReportProgress;
			}
		}

		[Test]
		public void ShouldUserCancel()
		{
			_target.ReportProgress += _target_ReportProgress2;
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList)).Return(_teamInfo);

				//round1
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro}));

			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),_selectedPersons, _optimizationPreferences, _rollbackService,_schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
				_target.ReportProgress -= _target_ReportProgress2;
			}
		}

		void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}

		void _target_ReportProgress2(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.CancelAction();
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
				runOneMatrixMocks(true, false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));

				//round2
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);


				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));

			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
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
				runOneMatrixMocks(true, false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));

				//round2
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
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
				runOneMatrixMocks(false, true, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
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
				runOneMatrixMocks(false, true, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));

			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
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
				runOneMatrixMocks(false, false, true, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
			}
		}


		[Test]
		public void ShouldRunUntilEqualOrHigherPeriodValueButRollbackAndLockInMatrixesIfMinWorkTimePerWeekValidationFails()
		{

			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList))
					  .Return(_teamInfo);

				//round1
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, true, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));

				//round2
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				runOneMatrixMocks(false, false, false, false, false);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(2);

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));

			}

			using (_mocks.Playback())
			{
				_target.OptimizeDaysOff(_matrixList,
					new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)),
					_selectedPersons, _optimizationPreferences, _rollbackService,
					_schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
			}
		}

		[Test]
		public void ShouldRollBackAndLockWhenOptimizeWithFreeDayOffsAndMinWorktimePerWeekValidationFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList)).Return(_teamInfo);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.ClearModificationCollection()).Repeat.AtLeastOnce();
				Expect.Call(_matrix.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(_daysOffPreferences.ConsiderWeekBefore,_daysOffPreferences.ConsiderWeekAfter, _matrix)).Return(_originalArray).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockDaysOffMoveFinder.TryFindMoves(_matrix, _originalArray, _optimizationPreferences, _daysOffPreferences)).Return(_workingArray);
				Expect.Call(_teamBlockDaysOffMoveFinder.TryFindMoves(_matrix, _originalArray, _optimizationPreferences, _daysOffPreferences)).Return(_originalArray);

				Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_originalArray, _workingArray, _matrix,_daysOffPreferences.ConsiderWeekBefore)).IgnoreArguments().Return(new List<DateOnly> { DateOnly.MinValue });
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_originalArray, _workingArray, _matrix,_daysOffPreferences.ConsiderWeekBefore)).IgnoreArguments().Return(new List<DateOnly> { DateOnly.MinValue.AddDays(1) });
				Expect.Call(() => _teamDayOffModifier.RemoveDayOffForMember(_rollbackService, _person, DateOnly.MinValue.AddDays(1)));
				Expect.Call(() => _teamDayOffModifier.AddDayOffForMember(_rollbackService, _person, DateOnly.MinValue, _schedulingOptions.DayOffTemplate, true));
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1),_schedulingOptions.BlockFinderTypeForAdvanceScheduling, false)).Return(_teamBlockInfo);
				Expect.Call(_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).Return(false);
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, DateOnly.MinValue.AddDays(1),_schedulingOptions,_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue, _schedulingOptions)).Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue.AddDays(1), _schedulingOptions)).Return(true);
				Expect.Call(_teamBlockOptimizationLimits.Validate(_teamInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions)).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(_teamInfo)).Return(false);

				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)));
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));

				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro })).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_optimizationPreferences.Extra.UseTeamSameDaysOff = false;
				_target.OptimizeDaysOff(_matrixList, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _selectedPersons, _optimizationPreferences, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
			}
		}

		[Test] 
		public void ShouldRollBackAndLockWhenShiftCategoryLimitationsIsBroken()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList)).Return(_teamInfo);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.ClearModificationCollection()).Repeat.AtLeastOnce();
				Expect.Call(_matrix.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter, _matrix)).Return(_originalArray).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockDaysOffMoveFinder.TryFindMoves(_matrix, _originalArray, _optimizationPreferences, _daysOffPreferences)).Return(_workingArray);

				Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_originalArray, _workingArray, _matrix, _daysOffPreferences.ConsiderWeekBefore)).IgnoreArguments().Return(new List<DateOnly> { DateOnly.MinValue });
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_originalArray, _workingArray, _matrix, _daysOffPreferences.ConsiderWeekBefore)).IgnoreArguments().Return(new List<DateOnly> { DateOnly.MinValue.AddDays(1) });
				Expect.Call(() => _teamDayOffModifier.RemoveDayOffForMember(_rollbackService, _person, DateOnly.MinValue.AddDays(1)));
				Expect.Call(() => _teamDayOffModifier.AddDayOffForMember(_rollbackService, _person, DateOnly.MinValue, _schedulingOptions.DayOffTemplate, true));
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions.BlockFinderTypeForAdvanceScheduling, false)).Return(_teamBlockInfo);
				Expect.Call(_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).Return(false);
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue, _schedulingOptions)).Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue.AddDays(1), _schedulingOptions)).Return(true);
				Expect.Call(_teamBlockOptimizationLimits.Validate(_teamInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions)).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(_teamInfo)).Return(true);

				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions.BlockFinderTypeForAdvanceScheduling, false)).Return(_teamBlockInfo);
				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo, null, _optimizationPreferences)).Return(false);

				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1))).Repeat.AtLeastOnce();
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue.AddDays(1), DateOnly.MinValue.AddDays(1))));

				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}

			using (_mocks.Playback())
			{
				_optimizationPreferences.Extra.UseTeamSameDaysOff = false;
				_target.OptimizeDaysOff(_matrixList, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _selectedPersons, _optimizationPreferences, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
			}
		}

		[Test]
		public void ShouldRollBackAndLockWhenDaysOffOnPeriodIsBrokenOnBlock()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList)).Return(_teamInfo);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.ClearModificationCollection()).Repeat.AtLeastOnce();
				Expect.Call(_matrix.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter, _matrix)).Return(_originalArray).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockDaysOffMoveFinder.TryFindMoves(_matrix, _originalArray, _optimizationPreferences, _daysOffPreferences)).Return(_workingArray);
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_originalArray, _workingArray, _matrix, _daysOffPreferences.ConsiderWeekBefore)).IgnoreArguments().Return(new List<DateOnly> { DateOnly.MinValue });
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_originalArray, _workingArray, _matrix, _daysOffPreferences.ConsiderWeekBefore)).IgnoreArguments().Return(new List<DateOnly> { DateOnly.MinValue.AddDays(1) });
				Expect.Call(() => _teamDayOffModifier.RemoveDayOffForMember(_rollbackService, _person, DateOnly.MinValue.AddDays(1)));
				Expect.Call(() => _teamDayOffModifier.AddDayOffForMember(_rollbackService, _person, DateOnly.MinValue, _schedulingOptions.DayOffTemplate, true));
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions.BlockFinderTypeForAdvanceScheduling, false)).Return(_teamBlockInfo);
				Expect.Call(_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).Return(false);
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockDayOffsInPeriodValidator.Validate(_teamInfo, _schedulingResultStateHolder)).Return(false);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions)).Repeat.AtLeastOnce();
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1))).Repeat.AtLeastOnce();
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue.AddDays(1), DateOnly.MinValue.AddDays(1))));

				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}

			using (_mocks.Playback())
			{
				_optimizationPreferences.Extra.UseTeamSameDaysOff = false;
				_optimizationPreferences.General.OptimizationStepDaysOff = false;
				_optimizationPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime = true;
				_target.OptimizeDaysOff(_matrixList, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _selectedPersons, _optimizationPreferences, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
			}
		}

		[Test]
		public void ShouldRollBackAndLockWhenDaysOffOnPeriodIsBrokenOnTeam()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _matrixList)).Return(_teamInfo);
				Expect.Call(_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.ClearModificationCollection()).Repeat.AtLeastOnce();
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter, _matrix)).Return(_originalArray).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockDaysOffMoveFinder.TryFindMoves(_matrix, _originalArray, _optimizationPreferences, _daysOffPreferences)).Return(_workingArray);
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_originalArray, _workingArray, _matrix, _daysOffPreferences.ConsiderWeekBefore)).IgnoreArguments().Return(new List<DateOnly> { DateOnly.MinValue });
				Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_originalArray, _workingArray, _matrix,_daysOffPreferences.ConsiderWeekBefore)).IgnoreArguments().Return(new List<DateOnly> { DateOnly.MinValue.AddDays(1) });
				Expect.Call(() => _teamDayOffModifier.RemoveDayOffForTeam(_rollbackService, _teamInfo, DateOnly.MinValue.AddDays(1)));
				Expect.Call(() => _teamDayOffModifier.AddDayOffForTeamAndResourceCalculate(_rollbackService, _teamInfo, DateOnly.MinValue, _schedulingOptions.DayOffTemplate)).IgnoreArguments();
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions.BlockFinderTypeForAdvanceScheduling, false)).Return(_teamBlockInfo);
				Expect.Call(_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).Return(false);
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockDayOffsInPeriodValidator.Validate(_teamInfo, _schedulingResultStateHolder)).Return(false);
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions)).Repeat.AtLeastOnce();
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1))).Repeat.AtLeastOnce();
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue.AddDays(1), DateOnly.MinValue.AddDays(1))));

				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}

			using (_mocks.Playback())
			{
				_optimizationPreferences.General.OptimizationStepDaysOff = false;
				_optimizationPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime = true;
				_target.OptimizeDaysOff(_matrixList, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), _selectedPersons, _optimizationPreferences, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _schedulingResultStateHolder, _dayOffOptimizationPreferenceProvider);
			}
		}


		private void runOneMatrixMocks(bool failOnRestrictionOverLimitValidator, bool failOnRescheduling, bool failOnNoMoveFound, bool failOnMinWorkTimPerWeek, bool failOnShiftCategoryLimitation)
		{
			Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(_daysOffPreferences.ConsiderWeekBefore,
																	   _daysOffPreferences.ConsiderWeekAfter, _matrix))
					  .Return(_originalArray);

			ILockableBitArray arrayToReturn = _workingArray;
			if (failOnNoMoveFound)
				arrayToReturn = _originalArray;

			Expect.Call(_teamBlockDaysOffMoveFinder.TryFindMoves(_matrix, _originalArray, _optimizationPreferences, _daysOffPreferences))
				  .Return(arrayToReturn);

			if (failOnNoMoveFound)
				return;


			Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_originalArray, _workingArray, _matrix,
			                                                         _daysOffPreferences.ConsiderWeekBefore))
			      .IgnoreArguments()
			      .Return(new List<DateOnly> {DateOnly.MinValue});
			Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_originalArray, _workingArray, _matrix,
			                                                           _daysOffPreferences.ConsiderWeekBefore))
			      .IgnoreArguments()
				  .Return(new List<DateOnly> { DateOnly.MinValue.AddDays(1) });

			Expect.Call(
				() =>
				_teamDayOffModifier.RemoveDayOffForTeam(_rollbackService, _teamInfo, DateOnly.MinValue.AddDays(1)));
			Expect.Call(
				() =>
				_teamDayOffModifier.AddDayOffForTeamAndResourceCalculate(_rollbackService, _teamInfo, DateOnly.MinValue, _schedulingOptions.DayOffTemplate)).IgnoreArguments();
			Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1),
																  _schedulingOptions.BlockFinderTypeForAdvanceScheduling, false))
			      .Return(_teamBlockInfo);

			Expect.Call(_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).Return(false);
			Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
			Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, DateOnly.MinValue.AddDays(1), _schedulingOptions,
				_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective()))
				.IgnoreArguments()
				.Return(!failOnRescheduling);
			if (failOnRescheduling)
				return;

			

			Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue, _schedulingOptions)).Return(true);
			Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(DateOnly.MinValue.AddDays(1), _schedulingOptions)).Return(true);
			Expect.Call(_teamBlockOptimizationLimits.Validate(_teamInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(!failOnRestrictionOverLimitValidator);


			if (failOnRestrictionOverLimitValidator)
			{
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)));
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)));
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue.AddDays(1), DateOnly.MinValue.AddDays(1))));	
				return;
			}

			Expect.Call(_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(_teamInfo)).Return(!failOnMinWorkTimPerWeek);

			if (failOnMinWorkTimPerWeek)
			{
				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)));
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
				return;
			}

			Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, DateOnly.MinValue.AddDays(1),_schedulingOptions.BlockFinderTypeForAdvanceScheduling, false)).Return(_teamBlockInfo);
			Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo, null, _optimizationPreferences)).Return(!failOnShiftCategoryLimitation);

		}
	}
}