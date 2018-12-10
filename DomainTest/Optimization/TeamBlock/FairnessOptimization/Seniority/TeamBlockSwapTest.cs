using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class TeamBlockSwapTest
	{
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private ISwapServiceNew _swapServiceNew;
		private TeamBlockSwap _target;
		private IScheduleDictionary _scheduleDictionary;
		private ITeamBlockSwapValidator _teamBlockSwapValidator;
		private ITeamBlockSwapDayValidator _teamBlockSwapDayValidator;
		private ISchedulePartModifyAndRollbackService _modifyAndRollbackService;
		private IPerson _person1;
		private IPerson _person2;
		private DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);
		private IBusinessRuleResponse _businessRuleResponse;
		private ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private IOptimizationPreferences _optimizationPreferences;
		private IDaysOffPreferences _daysOffPreferences;
		private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;

		[SetUp]
		public void SetUp()
		{
			_person1 = PersonFactory.CreatePerson("Person1");
			_person2 = PersonFactory.CreatePerson("Person2");

			_scheduleDay1 = ScheduleDayFactory.Create(_dateOnlyPeriod.StartDate, _person1);
			_scheduleDictionary = _scheduleDay1.Owner;
			_scheduleDay2 = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person2, _dateOnlyPeriod.StartDate, CurrentAuthorization.Make());

			_teamBlockInfo1 = new TeamBlockInfo(new TeamInfo(new Group(new List<IPerson> {_person1}, ""), new List<IList<IScheduleMatrixPro>>()),new BlockInfo(_dateOnlyPeriod));
			_teamBlockInfo2 = new TeamBlockInfo(new TeamInfo(new Group(new List<IPerson> {_person2}, ""), new List<IList<IScheduleMatrixPro>>()),new BlockInfo(_dateOnlyPeriod));
			_swapServiceNew = MockRepository.GenerateMock<ISwapServiceNew>();
			_teamBlockSwapValidator = MockRepository.GenerateMock<ITeamBlockSwapValidator>();
			_teamBlockSwapDayValidator = MockRepository.GenerateMock<ITeamBlockSwapDayValidator>();
			_modifyAndRollbackService = MockRepository.GenerateMock<ISchedulePartModifyAndRollbackService>();
			_businessRuleResponse = MockRepository.GenerateMock<IBusinessRuleResponse>();
			_teamBlockOptimizationLimits = MockRepository.GenerateMock<ITeamBlockOptimizationLimits>();
			_teamBlockShiftCategoryLimitationValidator =
				MockRepository.GenerateMock<ITeamBlockShiftCategoryLimitationValidator>();
			_optimizationPreferences = MockRepository.GenerateMock<IOptimizationPreferences>();
			_target = new TeamBlockSwap(_swapServiceNew, _teamBlockSwapValidator, _teamBlockSwapDayValidator,
				_teamBlockOptimizationLimits, _teamBlockShiftCategoryLimitationValidator);

			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(_daysOffPreferences);
		}

		[Test]
		public void ShouldSwap()
		{
			var swappedList = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};

			_teamBlockSwapValidator.Stub(x => x.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
			_teamBlockSwapDayValidator.Stub(x => x.ValidateSwapDays(_scheduleDay1, _scheduleDay2)).IgnoreArguments().Return(true);
			_swapServiceNew.Stub(x => x.Swap(new List<IScheduleDay> {_scheduleDay1, _scheduleDay2}, _scheduleDictionary)).IgnoreArguments().Return(swappedList);
			_modifyAndRollbackService.Stub(x => x.ModifyParts(swappedList)).Return(new List<IBusinessRuleResponse>());

			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo1)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo2)).Return(true);

			_teamBlockShiftCategoryLimitationValidator.Stub(x => x.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences)).Return(true);

			var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary,_dateOnlyPeriod, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldRollBackIfBusinessRulesBroken()
		{
			var swappedList = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};

			_teamBlockSwapValidator.Stub(x => x.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
			_teamBlockSwapDayValidator.Stub(x => x.ValidateSwapDays(_scheduleDay1, _scheduleDay2)).IgnoreArguments().Return(true);
			_swapServiceNew.Stub(x => x.Swap(new List<IScheduleDay> {_scheduleDay1, _scheduleDay2}, _scheduleDictionary)).IgnoreArguments().Return(swappedList);
			_modifyAndRollbackService.Stub(x => x.ModifyParts(swappedList)).Return(new List<IBusinessRuleResponse> {_businessRuleResponse});

			var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary,_dateOnlyPeriod, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);

			Assert.IsFalse(result);
			_modifyAndRollbackService.AssertWasCalled(x => x.Rollback());
		}

		[Test]
		public void ShouldRollBackIfRestrictionsOverLimit()
		{
			var swappedList = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};

			_teamBlockSwapValidator.Stub(x => x.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
			_teamBlockSwapDayValidator.Stub(x => x.ValidateSwapDays(_scheduleDay1, _scheduleDay2)).IgnoreArguments().Return(true);
			_swapServiceNew.Stub(x => x.Swap(new List<IScheduleDay> {_scheduleDay1, _scheduleDay2}, _scheduleDictionary)).IgnoreArguments().Return(swappedList);
			_modifyAndRollbackService.Stub(x => x.ModifyParts(swappedList)).Return(new List<IBusinessRuleResponse>());
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(false);

			var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary,_dateOnlyPeriod, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);

			Assert.IsFalse(result);
			_modifyAndRollbackService.AssertWasCalled(x => x.Rollback());
		}

		[Test]
		public void ShouldRollBackIfMinWorkTimePerWeekValidationFails()
		{
			var swappedList = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 };

			_teamBlockSwapValidator.Stub(x => x.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
			_teamBlockSwapDayValidator.Stub(x => x.ValidateSwapDays(_scheduleDay1, _scheduleDay2)).IgnoreArguments().Return(true);
			_swapServiceNew.Stub(x => x.Swap(new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 }, _scheduleDictionary)).IgnoreArguments().Return(swappedList);
			_modifyAndRollbackService.Stub(x => x.ModifyParts(swappedList)).Return(new List<IBusinessRuleResponse>());
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo1)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo2)).Return(false);
			_teamBlockShiftCategoryLimitationValidator.Stub(x => x.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences)).Return(true);

			var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary, _dateOnlyPeriod, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);

			Assert.IsFalse(result);
			_modifyAndRollbackService.AssertWasCalled(x => x.Rollback());
		}

		[Test]
		public void ShouldRollBackIfShiftCategoryOverLimit()
		{
			var swappedList = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};

			_teamBlockSwapValidator.Stub(x => x.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
			_teamBlockSwapDayValidator.Stub(x => x.ValidateSwapDays(_scheduleDay1, _scheduleDay2)).IgnoreArguments().Return(true);
			_swapServiceNew.Stub(x => x.Swap(new List<IScheduleDay> {_scheduleDay1, _scheduleDay2}, _scheduleDictionary)).IgnoreArguments().Return(swappedList);
			_modifyAndRollbackService.Stub(x => x.ModifyParts(swappedList)).Return(new List<IBusinessRuleResponse>());
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo1)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo2)).Return(true);

			_teamBlockShiftCategoryLimitationValidator.Stub(x => x.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences)).Return(false);

			var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary,_dateOnlyPeriod, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
			Assert.IsFalse(result);
			_modifyAndRollbackService.AssertWasCalled(x => x.Rollback());
		}

		[Test]
		public void ShouldNotSwapIfValidatorFails()
		{
			_teamBlockSwapValidator.Stub(x => x.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(false);

			var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary,_dateOnlyPeriod, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldNotSwapIfValidatorDayFails()
		{
			_teamBlockSwapValidator.Stub(x => x.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
			_teamBlockSwapDayValidator.Stub(x => x.ValidateSwapDays(_scheduleDay1, _scheduleDay2)).IgnoreArguments().Return(false);

			var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary,_dateOnlyPeriod, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);

			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldNotModifyPartsOutsideSelectedPeriod()
		{
			var selectedPeriod = new DateOnlyPeriod(_dateOnlyPeriod.EndDate.AddDays(1), _dateOnlyPeriod.EndDate.AddDays(1));

			_teamBlockSwapValidator.Stub(x => x.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
			_modifyAndRollbackService.Stub(x => x.ModifyParts(new List<IScheduleDay>())).Return(new List<IBusinessRuleResponse>());
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo1, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.Validate(_teamBlockInfo2, _optimizationPreferences, _dayOffOptimizationPreferenceProvider)).Return(true);

			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo1)).Return(true);
			_teamBlockOptimizationLimits.Stub(x => x.ValidateMinWorkTimePerWeek(_teamBlockInfo2)).Return(true);

			_teamBlockShiftCategoryLimitationValidator.Stub(x => x.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences)).Return(true);

			var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary,selectedPeriod, _optimizationPreferences, _dayOffOptimizationPreferenceProvider);
			Assert.IsTrue(result);
		}
	}
}
