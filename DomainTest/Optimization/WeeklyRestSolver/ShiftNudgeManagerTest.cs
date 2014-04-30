using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
	[TestFixture]
	public class ShiftNudgeManagerTest
	{
		private MockRepository _mocks;
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
		private List<IScheduleMatrixPro> _allPersonMatrixList;
		private DateOnlyPeriod _selectedPeriod;
		private List<IPerson> _selectedPersons;
		private IList<ITeamBlockInfo> _leftTeamBlockInfoList;
		private ITeamBlockInfo _leftTeamBlockInfo;
		private ITeamBlockInfo _rightTeamBlockInfo;
		private List<ITeamBlockInfo> _rightTeamBlockInfoList;
		private ITeamInfo _leftTeamInfo;
		private IScheduleMatrixPro _matrix;
		private IScheduleRange _range;
		private IScheduleDay _leftScheduleDay;
		private IScheduleDay _rightScheduleDay;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsPeriod;
		private ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;
		private IOptimizationPreferences _optimizationPreferences;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_shiftNudgeEarlier = _mocks.StrictMock<IShiftNudgeEarlier>();
			_shiftNudgeLater = _mocks.StrictMock<IShiftNudgeLater>();
			_ensureWeeklyRestRule = _mocks.StrictMock<IEnsureWeeklyRestRule>();
			_contractWeeklyRestForPersonWeek = new ContractWeeklyRestForPersonWeek();
			_teamBlockScheduleCloner = _mocks.StrictMock<ITeamBlockScheduleCloner>();
			_filterForTeamBlockInSelection = _mocks.StrictMock<IFilterForTeamBlockInSelection>();
			_teamBlockRestrictionOverLimitValidator = _mocks.StrictMock<ITeamBlockRestrictionOverLimitValidator>();
			_schedulingOptionsCreator = _mocks.StrictMock<ISchedulingOptionsCreator>();
			_target = new ShiftNudgeManager(_shiftNudgeEarlier, _shiftNudgeLater, _ensureWeeklyRestRule,
				_contractWeeklyRestForPersonWeek, _teamBlockScheduleCloner, _filterForTeamBlockInSelection,
				_teamBlockRestrictionOverLimitValidator, _schedulingOptionsCreator);
			_person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			_person.Period(DateOnly.MinValue).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(48), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			_personWeek = new PersonWeek(_person, new DateOnlyPeriod(2014, 03, 24, 2014, 03, 30));
			_teamBlockGenerator = _mocks.StrictMock<ITeamBlockGenerator>();
			_schedulingOptions = new SchedulingOptions();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_allPersonMatrixList = new List<IScheduleMatrixPro>();
			_selectedPeriod = new DateOnlyPeriod();
			_selectedPersons = new List<IPerson>();
			_leftTeamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_leftTeamBlockInfoList = new List<ITeamBlockInfo> {_leftTeamBlockInfo};
			_rightTeamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_rightTeamBlockInfoList = new List<ITeamBlockInfo> { _rightTeamBlockInfo };
			_leftTeamInfo = _mocks.StrictMock<ITeamInfo>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_range = _mocks.StrictMock<IScheduleRange>();
			_leftScheduleDay = _mocks.StrictMock<IScheduleDay>();
			_rightScheduleDay = _mocks.StrictMock<IScheduleDay>();
			_dateOnlyAsPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			_optimizationPreferences = new OptimizationPreferences();
			
		}

		[Test]
		public void ShouldReturnTrueIfSuccessful()
		{

			using (_mocks.Record())
			{
				initialMocks(false);
				Expect.Call(_filterForTeamBlockInSelection.Filter(
					new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo}, _selectedPersons, _selectedPeriod))
					.Return(new List<ITeamBlockInfo> {_leftTeamBlockInfo, _rightTeamBlockInfo});
				middleMocks();

				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false);
				Expect.Call(_shiftNudgeEarlier.Nudge(_leftScheduleDay, _rollbackService, _schedulingOptions,
					_resourceCalculateDelayer, _leftTeamBlockInfo, _schedulingResultStateHolder, _selectedPersons))
					.Return(true);
				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false);
				Expect.Call(_shiftNudgeLater.Nudge(_rightScheduleDay, _rollbackService, _schedulingOptions,
					_resourceCalculateDelayer, _rightTeamBlockInfo, _schedulingResultStateHolder, _selectedPersons))
					.Return(true);
				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true);

				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true);
			}

			using (_mocks.Playback())
			{
				bool result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator,
					_allPersonMatrixList, _rollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, _selectedPeriod, _selectedPersons, null,_schedulingOptions );
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfNudginFails()
		{
			using (_mocks.Record())
			{
				initialMocks(false);
				Expect.Call(_filterForTeamBlockInSelection.Filter(
					new List<ITeamBlockInfo> { _leftTeamBlockInfo, _rightTeamBlockInfo }, _selectedPersons, _selectedPeriod))
					.Return(new List<ITeamBlockInfo> { _leftTeamBlockInfo, _rightTeamBlockInfo });
				middleMocks();

				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false);
				Expect.Call(_shiftNudgeEarlier.Nudge(_leftScheduleDay, _rollbackService, _schedulingOptions,
					_resourceCalculateDelayer, _leftTeamBlockInfo, _schedulingResultStateHolder, _selectedPersons))
					.Return(false);
				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false);
				Expect.Call(_shiftNudgeLater.Nudge(_rightScheduleDay, _rollbackService, _schedulingOptions,
					_resourceCalculateDelayer, _rightTeamBlockInfo, _schedulingResultStateHolder, _selectedPersons))
					.Return(false);
				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false);

				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false);

				rollBackMocks();
			}

			using (_mocks.Playback())
			{
				bool result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator,
					_allPersonMatrixList, _rollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, _selectedPeriod, _selectedPersons, null,_schedulingOptions );
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfNotAllInSelection()
		{
			using (_mocks.Record())
			{
				initialMocks(false);
				Expect.Call(_filterForTeamBlockInSelection.Filter(
					new List<ITeamBlockInfo> { _leftTeamBlockInfo, _rightTeamBlockInfo }, _selectedPersons, _selectedPeriod))
					.Return(new List<ITeamBlockInfo> { _leftTeamBlockInfo });
				
			}

			using (_mocks.Playback())
			{
				bool result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator,
					_allPersonMatrixList, _rollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, _selectedPeriod, _selectedPersons, null,_schedulingOptions );
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfRestrictionValidatorFails()
		{

			using (_mocks.Record())
			{
				initialMocks(true);
				Expect.Call(_filterForTeamBlockInSelection.Filter(
					new List<ITeamBlockInfo> { _leftTeamBlockInfo, _rightTeamBlockInfo }, _selectedPersons, _selectedPeriod))
					.Return(new List<ITeamBlockInfo> { _leftTeamBlockInfo, _rightTeamBlockInfo });
				middleMocks();

				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false);
				Expect.Call(_shiftNudgeEarlier.Nudge(_leftScheduleDay, _rollbackService, _schedulingOptions,
					_resourceCalculateDelayer, _leftTeamBlockInfo, _schedulingResultStateHolder, _selectedPersons))
					.Return(true);
				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(false);
				Expect.Call(_shiftNudgeLater.Nudge(_rightScheduleDay, _rollbackService, _schedulingOptions,
					_resourceCalculateDelayer, _rightTeamBlockInfo, _schedulingResultStateHolder, _selectedPersons))
					.Return(true);
				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true);

				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _range, TimeSpan.FromHours(36))).Return(true);

				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_leftTeamBlockInfo, _optimizationPreferences))
					.Return(false);
				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_rightTeamBlockInfo, _optimizationPreferences))
					.Return(false);

				rollBackMocks();
			}

			using (_mocks.Playback())
			{
				bool result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator,
					_allPersonMatrixList, _rollbackService, _resourceCalculateDelayer,
                    _schedulingResultStateHolder, _selectedPeriod, _selectedPersons, _optimizationPreferences, null);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfBlockIsSchedulePeriod()
		{
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod;
			_schedulingOptions.UseTeamBlockPerOption = true;

			bool result = _target.TrySolveForDayOff(_personWeek, new DateOnly(2014, 03, 30), _teamBlockGenerator,
					_allPersonMatrixList, _rollbackService, _resourceCalculateDelayer,
                    _schedulingResultStateHolder, _selectedPeriod, _selectedPersons, null, _schedulingOptions);
			Assert.IsFalse(result);
		}

		private void rollBackMocks()
		{
			Expect.Call(() => _rollbackService.ModifyParts(new List<IScheduleDay>{_leftScheduleDay, _rightScheduleDay}));
			Expect.Call(() => _rollbackService.ClearModificationCollection());
			Expect.Call(_leftScheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsPeriod);
			Expect.Call(_dateOnlyAsPeriod.DateOnly).Return(new DateOnly(2014, 3, 29));
			Expect.Call(_rightScheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsPeriod);
			Expect.Call(_dateOnlyAsPeriod.DateOnly).Return(new DateOnly(2014, 3, 29));
			Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(2014, 3, 29), null)).Return(true);
			Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(2014, 3, 30), null)).Return(true);
		}

		private void initialMocks(bool useOptimizationPreferences)
		{
			if (useOptimizationPreferences)
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_teamBlockGenerator.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 29, 2014, 3, 29),
				new List<IPerson> { _person }, _schedulingOptions))
				.Return(_leftTeamBlockInfoList);
				Expect.Call(_teamBlockGenerator.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 31, 2014, 3, 31),
					new List<IPerson> { _person }, _schedulingOptions))
					.Return(_rightTeamBlockInfoList);
			}
			else
			{
				Expect.Call(_teamBlockGenerator.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 29, 2014, 3, 29),
					new List<IPerson> {_person}, _schedulingOptions))
					.Return(_leftTeamBlockInfoList);
				Expect.Call(_teamBlockGenerator.Generate(_allPersonMatrixList, new DateOnlyPeriod(2014, 3, 31, 2014, 3, 31),
					new List<IPerson> { _person }, _schedulingOptions))
					.Return(_rightTeamBlockInfoList);
			}
		}

		private void middleMocks()
		{
			Expect.Call(_teamBlockScheduleCloner.CloneSchedules(_leftTeamBlockInfo)).Return(new List<IScheduleDay>{_leftScheduleDay});
			Expect.Call(_teamBlockScheduleCloner.CloneSchedules(_rightTeamBlockInfo)).Return(new List<IScheduleDay>{_rightScheduleDay});
			Expect.Call(_leftTeamBlockInfo.TeamInfo).Return(_leftTeamInfo);
			Expect.Call(_leftTeamInfo.MatrixForMemberAndDate(_person, new DateOnly(2014, 3, 29))).Return(_matrix);
			Expect.Call(_matrix.ActiveScheduleRange).Return(_range);
			Expect.Call(_range.ScheduledDay(new DateOnly(2014, 3, 29))).Return(_leftScheduleDay);
			Expect.Call(_range.ScheduledDay(new DateOnly(2014, 3, 31))).Return(_rightScheduleDay);
		}
	}
}