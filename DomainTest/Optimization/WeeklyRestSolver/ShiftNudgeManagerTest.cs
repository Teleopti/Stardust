using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
	[TestFixture]
	public class ShiftNudgeManagerTest
	{
		private IShiftNudgeManager _target;
		private IShiftNudgeEarlier _shiftNudgeEarlier;
		private IShiftNudgeLater _shiftNudgeLater;
		private IEnsureWeeklyRestRule _ensureWeeklyRestRule;
		private IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
		private ITeamBlockScheduleCloner _teamBlockScheduleCloner;
		private IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private PersonWeek _personWeek;
		private IPerson _person;
		private ITeamBlockGenerator _teamBlockGenerator;
		private ISchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly DateOnlyPeriod _selectedPeriod = new DateOnlyPeriod(2014, 03, 24, 2014, 03, 31);
		private List<IPerson> _selectedPersons;
		private IList<ITeamBlockInfo> _leftTeamBlockInfoList;
		private ITeamBlockInfo _leftTeamBlockInfo;
		private ITeamBlockInfo _rightTeamBlockInfo;
		private List<ITeamBlockInfo> _rightTeamBlockInfoList;
		private IScheduleMatrixPro _matrix;
		private IScheduleRange _range;
		private IScheduleDay _leftScheduleDay;
		private IScheduleDay _rightScheduleDay;
		private ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private IOptimizationPreferences _optimizationPreferences;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private List<IScheduleMatrixPro> _allPersonMatrixList;
		private ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private IScheduleDayIsLockedSpecification _scheduleDayIsLockedSpecification;
		private IDaysOffPreferences _daysOffPreferences;
		private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;

		[SetUp]
		public void Setup()
		{
			_shiftNudgeEarlier = MockRepository.GenerateMock<IShiftNudgeEarlier>();
			_shiftNudgeLater = MockRepository.GenerateMock<IShiftNudgeLater>();
			_ensureWeeklyRestRule = MockRepository.GenerateMock<IEnsureWeeklyRestRule>();
			_contractWeeklyRestForPersonWeek = new ContractWeeklyRestForPersonWeek();
			_teamBlockScheduleCloner = MockRepository.GenerateMock<ITeamBlockScheduleCloner>();
			_filterForTeamBlockInSelection = MockRepository.GenerateMock<IFilterForTeamBlockInSelection>();
			_schedulingOptionsCreator = MockRepository.GenerateMock<ISchedulingOptionsCreator>();
			_teamBlockOptimizationLimits = MockRepository.GenerateMock<ITeamBlockOptimizationLimits>();
			_teamBlockSteadyStateValidator = MockRepository.GenerateMock<ITeamBlockSteadyStateValidator>();
			_target = new ShiftNudgeManager(_shiftNudgeEarlier, _shiftNudgeLater, 
				_ensureWeeklyRestRule, _contractWeeklyRestForPersonWeek, 
				_teamBlockScheduleCloner, _filterForTeamBlockInSelection, 
				_teamBlockOptimizationLimits, _schedulingOptionsCreator, 
				_teamBlockSteadyStateValidator, _scheduleDayIsLockedSpecification);
			_person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			_person.AddSchedulePeriod(new SchedulePeriod(_selectedPeriod.StartDate, SchedulePeriodType.Month, 1));
			_person.Period(DateOnly.MinValue).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(48), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			_personWeek = new PersonWeek(_person, _selectedPeriod);
			_teamBlockGenerator = MockRepository.GenerateMock<ITeamBlockGenerator>();
			_schedulingOptions = new SchedulingOptions();
			_rollbackService = MockRepository.GenerateMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = MockRepository.GenerateMock<IResourceCalculateDelayer>();
			_schedulingResultStateHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
			_allPersonMatrixList = new List<IScheduleMatrixPro>();
			_scheduleDayIsLockedSpecification = MockRepository.GenerateMock<IScheduleDayIsLockedSpecification>();

			_selectedPersons = new List<IPerson> {_person};
			_leftTeamBlockInfo = new TeamBlockInfo(new TeamInfo(new Group(new List<IPerson> {_person}, ""), new List<IList<IScheduleMatrixPro>> {_allPersonMatrixList}), new BlockInfo(new DateOnlyPeriod(2014, 3, 29, 2014, 3, 29)));
			_leftTeamBlockInfoList = new List<ITeamBlockInfo> {_leftTeamBlockInfo};
			_rightTeamBlockInfo = new TeamBlockInfo(new TeamInfo(new Group(new List<IPerson> {_person}, ""), new List<IList<IScheduleMatrixPro>> {_allPersonMatrixList}), new BlockInfo(new DateOnlyPeriod(2014, 3, 31, 2014, 3, 31)));
			_rightTeamBlockInfoList = new List<ITeamBlockInfo> {_rightTeamBlockInfo};
			_matrix = MockRepository.GenerateMock<IScheduleMatrixPro>();
			_range = MockRepository.GenerateMock<IScheduleRange>();
			_leftScheduleDay = ScheduleDayFactory.Create(_selectedPeriod.StartDate, _person);
			_rightScheduleDay = ScheduleDayFactory.Create(_selectedPeriod.StartDate, _person);
			_optimizationPreferences = new OptimizationPreferences();
			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(_daysOffPreferences);
		}

		[Test]
		public void ShouldReturnTrueIfSuccessful()
		{
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 29, 2014, 3, 29), new List<IPerson> {_person}, _schedulingOptions)).Return(_leftTeamBlockInfoList);
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 31, 2014, 3, 31), new List<IPerson> {_person}, _schedulingOptions)).Return(_rightTeamBlockInfoList);
			_filterForTeamBlockInSelection.Stub(x => x.Filter(new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo}, _selectedPersons, _selectedPeriod)).Return(new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo});

			_teamBlockScheduleCloner.Stub(x => x.CloneSchedules(_leftTeamBlockInfo)).Return(new List<IScheduleDay> {_leftScheduleDay});
			_teamBlockScheduleCloner.Stub(x => x.CloneSchedules(_rightTeamBlockInfo)).Return(new List<IScheduleDay> {_rightScheduleDay});
			_allPersonMatrixList.Add(_matrix);
			_matrix.Stub(x => x.Person).Return(_person);
			_matrix.Stub(x => x.SchedulePeriod).Return(new VirtualSchedulePeriod(_person, new DateOnly(2014, 3, 29), new VirtualSchedulePeriodSplitChecker(_person)));
			_matrix.Stub(x => x.ActiveScheduleRange).Return(_range);
			_range.Stub(x => x.ScheduledDay(new DateOnly(2014, 3, 29))).Return(_leftScheduleDay);
			_range.Stub(x => x.ScheduledDay(new DateOnly(2014, 3, 31))).Return(_rightScheduleDay);

			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Once();
			_shiftNudgeEarlier.Stub(x => x.Nudge(_leftScheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _leftTeamBlockInfo, _schedulingResultStateHolder, null)).Return(true);
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Once();
			_shiftNudgeLater.Stub(x => x.Nudge(_rightScheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _rightTeamBlockInfo, _schedulingResultStateHolder, null)).Return(true);
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true).Repeat.Once();
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true).Repeat.Once();
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_leftTeamBlockInfo)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_rightTeamBlockInfo)).Return(true);
			_teamBlockSteadyStateValidator.Stub(x => x.IsTeamBlockInSteadyState(_leftTeamBlockInfo, _schedulingOptions))
				.Return(true);
			_teamBlockSteadyStateValidator.Stub(x => x.IsTeamBlockInSteadyState(_rightTeamBlockInfo, _schedulingOptions))
				.Return(true);

			var result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator, _allPersonMatrixList, _rollbackService, _resourceCalculateDelayer, 
													_schedulingResultStateHolder, _selectedPeriod, _selectedPersons, null, _schedulingOptions, _dayOffOptimizationPreferenceProvider);
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldReturnFalseIfNudginFails()
		{
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 29, 2014, 3, 29), new List<IPerson> {_person}, _schedulingOptions)).Return(_leftTeamBlockInfoList);
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 31, 2014, 3, 31), new List<IPerson> {_person}, _schedulingOptions)).Return(_rightTeamBlockInfoList);
			_filterForTeamBlockInSelection.Stub(x => x.Filter(new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo}, _selectedPersons, _selectedPeriod)).Return(new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo});

			_teamBlockScheduleCloner.Stub(x => x.CloneSchedules(_leftTeamBlockInfo)).Return(new List<IScheduleDay> {_leftScheduleDay});
			_teamBlockScheduleCloner.Stub(x => x.CloneSchedules(_rightTeamBlockInfo)).Return(new List<IScheduleDay> {_rightScheduleDay});
			_allPersonMatrixList.Add(_matrix);
			_matrix.Stub(x => x.Person).Return(_person);
			_matrix.Stub(x => x.SchedulePeriod).Return(new VirtualSchedulePeriod(_person, new DateOnly(2014, 3, 29), new VirtualSchedulePeriodSplitChecker(_person)));
			_matrix.Stub(x => x.ActiveScheduleRange).Return(_range);
			_range.Stub(x => x.ScheduledDay(new DateOnly(2014, 3, 29))).Return(_leftScheduleDay);
			_range.Stub(x => x.ScheduledDay(new DateOnly(2014, 3, 31))).Return(_rightScheduleDay);

			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Once();
			_shiftNudgeEarlier.Stub(x => x.Nudge(_leftScheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _leftTeamBlockInfo, _schedulingResultStateHolder, null)).Return(false);
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Once();
			_shiftNudgeLater.Stub(x => x.Nudge(_rightScheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _rightTeamBlockInfo, _schedulingResultStateHolder, null)).Return(false);
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Once();
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Once();

			_resourceCalculateDelayer.Stub(x => x.CalculateIfNeeded(new DateOnly(2014, 3, 29), null)).Return(true);
			_resourceCalculateDelayer.Stub(x => x.CalculateIfNeeded(new DateOnly(2014, 3, 30), null)).Return(true);

			_teamBlockSteadyStateValidator.Stub(x => x.IsTeamBlockInSteadyState(_leftTeamBlockInfo, _schedulingOptions))
				.Return(true);
			_teamBlockSteadyStateValidator.Stub(x => x.IsTeamBlockInSteadyState(_rightTeamBlockInfo, _schedulingOptions))
				.Return(true);

			var result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator, _allPersonMatrixList, _rollbackService, 
													_resourceCalculateDelayer, _schedulingResultStateHolder, _selectedPeriod, 
													_selectedPersons, null, _schedulingOptions, _dayOffOptimizationPreferenceProvider);
			Assert.IsFalse(result);
			_rollbackService.AssertWasCalled(x => x.ModifyParts(new List<IScheduleDay> {_leftScheduleDay, _rightScheduleDay}));
		}

		[Test]
		public void ShouldReturnFalseIfNotAllInSelection()
		{
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 29, 2014, 3, 29), new List<IPerson> {_person}, _schedulingOptions)).Return(_leftTeamBlockInfoList);
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 31, 2014, 3, 31), new List<IPerson> {_person}, _schedulingOptions)).Return(_rightTeamBlockInfoList);

			_filterForTeamBlockInSelection.Stub(x => x.Filter(new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo}, _selectedPersons, _selectedPeriod)).Return(new List<ITeamBlockInfo> {_leftTeamBlockInfo});
			var result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator, _allPersonMatrixList, _rollbackService, 
													_resourceCalculateDelayer, _schedulingResultStateHolder, _selectedPeriod, 
													_selectedPersons, null, _schedulingOptions, _dayOffOptimizationPreferenceProvider);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseIfRestrictionValidatorFails()
		{
			_schedulingOptionsCreator.Stub(x => x.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);

			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 29, 2014, 3, 29), new List<IPerson> {_person}, _schedulingOptions)).Return(_leftTeamBlockInfoList);
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 31, 2014, 3, 31), new List<IPerson> {_person}, _schedulingOptions)).Return(_rightTeamBlockInfoList);
			_filterForTeamBlockInSelection.Stub(x => x.Filter(new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo}, _selectedPersons, _selectedPeriod)).Return(new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo});

			_teamBlockScheduleCloner.Stub(x => x.CloneSchedules(_leftTeamBlockInfo)).Return(new List<IScheduleDay> {_leftScheduleDay});
			_teamBlockScheduleCloner.Stub(x => x.CloneSchedules(_rightTeamBlockInfo)).Return(new List<IScheduleDay> {_rightScheduleDay});
			_allPersonMatrixList.Add(_matrix);
			_matrix.Stub(x => x.Person).Return(_person);
			_matrix.Stub(x => x.SchedulePeriod).Return(new VirtualSchedulePeriod(_person, new DateOnly(2014, 3, 29), new VirtualSchedulePeriodSplitChecker(_person)));
			_matrix.Stub(x => x.ActiveScheduleRange).Return(_range);
			_range.Stub(x => x.ScheduledDay(new DateOnly(2014, 3, 29))).Return(_leftScheduleDay);
			_range.Stub(x => x.ScheduledDay(new DateOnly(2014, 3, 31))).Return(_rightScheduleDay);

			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Once();
			_shiftNudgeEarlier.Stub(x => x.Nudge(_leftScheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _leftTeamBlockInfo, _schedulingResultStateHolder, null)).Return(true);
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Once();
			_shiftNudgeLater.Stub(x => x.Nudge(_rightScheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _rightTeamBlockInfo, _schedulingResultStateHolder, null)).Return(true);
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true).Repeat.Once();
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true).Repeat.Once();
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_leftTeamBlockInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(false);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_rightTeamBlockInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(false);

			_resourceCalculateDelayer.Stub(x => x.CalculateIfNeeded(new DateOnly(2014, 3, 29), null)).Return(true);
			_resourceCalculateDelayer.Stub(x => x.CalculateIfNeeded(new DateOnly(2014, 3, 30), null)).Return(true);

			_teamBlockSteadyStateValidator.Stub(x => x.IsTeamBlockInSteadyState(_leftTeamBlockInfo, _schedulingOptions))
				.Return(true);
			_teamBlockSteadyStateValidator.Stub(x => x.IsTeamBlockInSteadyState(_rightTeamBlockInfo, _schedulingOptions))
				.Return(true);

			var result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator, _allPersonMatrixList, _rollbackService, 
													_resourceCalculateDelayer, _schedulingResultStateHolder, _selectedPeriod, 
													_selectedPersons, _optimizationPreferences, null, _dayOffOptimizationPreferenceProvider);
			Assert.IsFalse(result);
			_rollbackService.AssertWasCalled(x => x.ModifyParts(new List<IScheduleDay> {_leftScheduleDay, _rightScheduleDay}));
		}

		[Test]
		public void ShouldReturnFalseIfOneOfTheteamBlockIsNull()
		{
			_schedulingOptionsCreator.Stub(x => x.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 29, 2014, 3, 29), new List<IPerson> {_person}, _schedulingOptions)).Return(_leftTeamBlockInfoList);
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 31, 2014, 3, 31), new List<IPerson> {_person}, _schedulingOptions)).Return(new List<ITeamBlockInfo>());

			var result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator, _allPersonMatrixList, _rollbackService, 
													_resourceCalculateDelayer, _schedulingResultStateHolder, _selectedPeriod, 
													_selectedPersons, _optimizationPreferences, null, _dayOffOptimizationPreferenceProvider);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseIfBlockIsSchedulePeriod()
		{
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod;
			_schedulingOptions.UseBlock = true;

			bool result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator, _allPersonMatrixList, _rollbackService, 
													_resourceCalculateDelayer, _schedulingResultStateHolder, _selectedPeriod, 
													_selectedPersons, null, _schedulingOptions, _dayOffOptimizationPreferenceProvider);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseIfMinWorkTimePerWeekValidationFails()
		{
			_schedulingOptionsCreator.Stub(x => x.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 29, 2014, 3, 29), new List<IPerson> {_person}, _schedulingOptions)).Return(_leftTeamBlockInfoList);
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 31, 2014, 3, 31), new List<IPerson> {_person}, _schedulingOptions)).Return(_rightTeamBlockInfoList);
			_filterForTeamBlockInSelection.Stub(x => x.Filter(new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo}, _selectedPersons, _selectedPeriod)).Return(new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo});

			_teamBlockScheduleCloner.Stub(x => x.CloneSchedules(_leftTeamBlockInfo)).Return(new List<IScheduleDay> {_leftScheduleDay});
			_teamBlockScheduleCloner.Stub(x => x.CloneSchedules(_rightTeamBlockInfo)).Return(new List<IScheduleDay> {_rightScheduleDay});
			_allPersonMatrixList.Add(_matrix);
			_matrix.Stub(x => x.Person).Return(_person);
			_matrix.Stub(x => x.SchedulePeriod).Return(new VirtualSchedulePeriod(_person, new DateOnly(2014, 3, 29), new VirtualSchedulePeriodSplitChecker(_person)));
			_matrix.Stub(x => x.ActiveScheduleRange).Return(_range);
			_range.Stub(x => x.ScheduledDay(new DateOnly(2014, 3, 29))).Return(_leftScheduleDay);
			_range.Stub(x => x.ScheduledDay(new DateOnly(2014, 3, 31))).Return(_rightScheduleDay);

			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Twice();
			_shiftNudgeEarlier.Stub(x => x.Nudge(_leftScheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _leftTeamBlockInfo, _schedulingResultStateHolder, null)).Return(true);
			_shiftNudgeLater.Stub(x => x.Nudge(_rightScheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _rightTeamBlockInfo, _schedulingResultStateHolder, null)).Return(true);
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true).Repeat.Twice();
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_leftTeamBlockInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_rightTeamBlockInfo, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_leftTeamBlockInfo)).Return(false);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_rightTeamBlockInfo)).Return(false);

			_resourceCalculateDelayer.Stub(x => x.CalculateIfNeeded(new DateOnly(2014, 3, 29), null)).Return(true);
			_resourceCalculateDelayer.Stub(x => x.CalculateIfNeeded(new DateOnly(2014, 3, 30), null)).Return(true);

			_teamBlockSteadyStateValidator.Stub(x => x.IsTeamBlockInSteadyState(_leftTeamBlockInfo, _schedulingOptions))
				.Return(true);
			_teamBlockSteadyStateValidator.Stub(x => x.IsTeamBlockInSteadyState(_rightTeamBlockInfo, _schedulingOptions))
				.Return(true);

			var result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator, _allPersonMatrixList, _rollbackService, 
													_resourceCalculateDelayer, _schedulingResultStateHolder, _selectedPeriod, 
													_selectedPersons, _optimizationPreferences, null, _dayOffOptimizationPreferenceProvider);
			Assert.IsFalse(result);
			_rollbackService.AssertWasCalled(x => x.ModifyParts(new List<IScheduleDay> {_leftScheduleDay, _rightScheduleDay}));
		}

		[Test]
		public void ShouldReturnFalseIfTeamNotInSteadyStateWhenStart()
		{
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 29, 2014, 3, 29), new List<IPerson> { _person }, _schedulingOptions)).Return(_leftTeamBlockInfoList);
			_teamBlockGenerator.Stub(x => x.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 31, 2014, 3, 31), new List<IPerson> { _person }, _schedulingOptions)).Return(_rightTeamBlockInfoList);
			_filterForTeamBlockInSelection.Stub(x => x.Filter(new List<ITeamBlockInfo> { _leftTeamBlockInfo, _rightTeamBlockInfo }, _selectedPersons, _selectedPeriod)).Return(new List<ITeamBlockInfo> { _leftTeamBlockInfo, _rightTeamBlockInfo });

			_teamBlockScheduleCloner.Stub(x => x.CloneSchedules(_leftTeamBlockInfo)).Return(new List<IScheduleDay> { _leftScheduleDay });
			_teamBlockScheduleCloner.Stub(x => x.CloneSchedules(_rightTeamBlockInfo)).Return(new List<IScheduleDay> { _rightScheduleDay });
			_allPersonMatrixList.Add(_matrix);
			_matrix.Stub(x => x.Person).Return(_person);
			_matrix.Stub(x => x.SchedulePeriod).Return(new VirtualSchedulePeriod(_person, new DateOnly(2014, 3, 29), new VirtualSchedulePeriodSplitChecker(_person)));
			_matrix.Stub(x => x.ActiveScheduleRange).Return(_range);
			_range.Stub(x => x.ScheduledDay(new DateOnly(2014, 3, 29))).Return(_leftScheduleDay);
			_range.Stub(x => x.ScheduledDay(new DateOnly(2014, 3, 31))).Return(_rightScheduleDay);

			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Once();
			_shiftNudgeEarlier.Stub(x => x.Nudge(_leftScheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _leftTeamBlockInfo, _schedulingResultStateHolder, null)).Return(true);
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false).Repeat.Once();
			_shiftNudgeLater.Stub(x => x.Nudge(_rightScheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer, _rightTeamBlockInfo, _schedulingResultStateHolder, null)).Return(true);
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true).Repeat.Once();
			_ensureWeeklyRestRule.Stub(x => x.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true).Repeat.Once();
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_leftTeamBlockInfo)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_rightTeamBlockInfo)).Return(true);

			// here check we steady state result
			_teamBlockSteadyStateValidator.Stub(x => x.IsTeamBlockInSteadyState(_leftTeamBlockInfo, _schedulingOptions))
				.Return(false);
			_teamBlockSteadyStateValidator.Stub(x => x.IsTeamBlockInSteadyState(_rightTeamBlockInfo, _schedulingOptions))
				.Return(false);

			var result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator, _allPersonMatrixList, _rollbackService, 
												_resourceCalculateDelayer, _schedulingResultStateHolder, _selectedPeriod, 
												_selectedPersons, null, _schedulingOptions, _dayOffOptimizationPreferenceProvider);
			Assert.IsFalse(result);
		}
	}
}