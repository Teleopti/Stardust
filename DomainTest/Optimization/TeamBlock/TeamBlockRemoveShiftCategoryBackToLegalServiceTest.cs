﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockRemoveShiftCategoryBackToLegalServiceTest
	{
		private MockRepository _mock;
		private ITeamBlockScheduler _teamBlockScheduler;
		private ITeamInfoFactory _teamInfoFactory;
		private ITeamBlockInfoFactory _teamBlockInfoFactory;
		private ITeamBlockClearer _teamBlockClearer;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private IShiftCategoryWeekRemover _shiftCategoryWeekRemover;
		private IShiftCategoryPeriodRemover _shiftCategoryPeriodRemover;
		private ISchedulingOptions _schedulingOptions;
		private IVirtualSchedulePeriod _virtualSchedulePeriod;
		private TeamBlockRemoveShiftCategoryBackToLegalService _target;
		private IShiftCategoryLimitation _shiftCategoryLimitation;
		private IShiftCategory _shiftCategory;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IList<IScheduleMatrixPro> _scheduleMatrixPros; 
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private IScheduleDayPro _scheduleDayPro;
		private IList<IScheduleDayPro> _scheduleDayPros;
		private DateOnly _dateOnly;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IPerson _person;
		private ITeamInfo _teamInfo;
		private ITeamBlockInfo _teamBlockInfo;
		private ShiftNudgeDirective _shiftNudgeDirective;
		private ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private IOptimizationPreferences _optimizationPreferences;
		private IShiftCategoryLimitCounter _shiftCategoryLimitCounter;
		private IScheduleDay _scheduleDay;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamBlockScheduler = _mock.StrictMock<ITeamBlockScheduler>();
			_teamInfoFactory = _mock.StrictMock<ITeamInfoFactory>();
			_teamBlockInfoFactory = _mock.StrictMock<ITeamBlockInfoFactory>();
			_teamBlockClearer = _mock.StrictMock<ITeamBlockClearer>();
			_teamBlockSchedulingOptions = new TeamBlockSchedulingOptions();
			_shiftCategoryWeekRemover = _mock.StrictMock<IShiftCategoryWeekRemover>();
			_shiftCategoryPeriodRemover = _mock.StrictMock<IShiftCategoryPeriodRemover>();
			_schedulingOptions = new SchedulingOptions();
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff;
			_schedulingOptions.UseBlock = true;
			_virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
			_shiftCategory = new ShiftCategory("shiftCategory");
			_shiftCategoryLimitation = new ShiftCategoryLimitation(_shiftCategory);
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPros = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
			_schedulingResultStateHolder = _mock.Stub<ISchedulingResultStateHolder>();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPros = new List<IScheduleDayPro> { _scheduleDayPro };
			_dateOnly = new DateOnly(2015, 1, 1);
			_person = PersonFactory.CreatePerson();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_shiftNudgeDirective = new ShiftNudgeDirective();
			_schedulingOptions.NotAllowedShiftCategories.Add(_shiftCategory);
			_safeRollbackAndResourceCalculation = _mock.StrictMock<ISafeRollbackAndResourceCalculation>();
			_optimizationPreferences = new OptimizationPreferences();
			_shiftCategoryLimitCounter = _mock.StrictMock<IShiftCategoryLimitCounter>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();

			_target = new TeamBlockRemoveShiftCategoryBackToLegalService(_teamBlockScheduler,_teamInfoFactory,_teamBlockInfoFactory,_teamBlockClearer,_teamBlockSchedulingOptions,_shiftCategoryWeekRemover,_shiftCategoryPeriodRemover, _safeRollbackAndResourceCalculation, _shiftCategoryLimitCounter, null);	
		}

		[Test]
		public void ShouldExecuteOnWeekRestriction()
		{
			_shiftCategoryLimitation.Weekly = true;
			var shiftCategoryLimitations = new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation>{_shiftCategoryLimitation});
			
			using (_mock.Record())
			{
				Expect.Call(_virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(shiftCategoryLimitations).Repeat.AtLeastOnce();

				Expect.Call(_shiftCategoryWeekRemover.Remove(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixPro, _optimizationPreferences)).Return(_scheduleDayPros);
				
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(false);

				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, new BetweenDayOffBlockFinder(), true)).IgnoreArguments().Return(_teamBlockInfo);

				Expect.Call(_shiftCategoryLimitCounter.HaveMaxOfShiftCategory(_shiftCategoryLimitation, _teamInfo, _dateOnly)).Return(false);

				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder))).IgnoreArguments().Return(true);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
			}

			using (_mock.Playback())
			{
				_schedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight("test", GroupPageType.SingleAgent);
				_target.Execute(_schedulingOptions, _scheduleMatrixPro, _schedulingResultStateHolder, _rollbackService, _resourceCalculateDelayer, _scheduleMatrixPros, _shiftNudgeDirective, _optimizationPreferences);
				Assert.IsNotEmpty(_schedulingOptions.NotAllowedShiftCategories);
			}
		}

		[Test]
		public void ShouldExecuteOnPeriodRestriction()
		{
			_shiftCategoryLimitation.Weekly = false;
			var shiftCategoryLimitations = new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation> { _shiftCategoryLimitation });

			using (_mock.Record())
			{
				Expect.Call(_virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(shiftCategoryLimitations).Repeat.AtLeastOnce();
				Expect.Call(_shiftCategoryPeriodRemover.RemoveShiftCategoryOnPeriod(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixPro, _optimizationPreferences)).Return(_scheduleDayPros);

				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, new BetweenDayOffBlockFinder(), true)).IgnoreArguments().Return(_teamBlockInfo);


				Expect.Call(_shiftCategoryLimitCounter.HaveMaxOfShiftCategory(_shiftCategoryLimitation, _teamInfo, _dateOnly)).Return(false);
				
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder))).IgnoreArguments().Return(true);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
			}

			using (_mock.Playback())
			{
				_schedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight("test", GroupPageType.SingleAgent);
				_target.Execute(_schedulingOptions, _scheduleMatrixPro, _schedulingResultStateHolder, _rollbackService, _resourceCalculateDelayer, _scheduleMatrixPros, _shiftNudgeDirective, _optimizationPreferences);
				Assert.IsNotEmpty(_schedulingOptions.NotAllowedShiftCategories);
			}	
		}

		[Test]
		public void ShouldClearTeamBlockAndRetryIfNoSuccesOnOneDay()
		{
			_shiftCategoryLimitation.Weekly = true;
			var shiftCategoryLimitations = new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation> { _shiftCategoryLimitation });

			using (_mock.Record())
			{
				Expect.Call(_virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(shiftCategoryLimitations).Repeat.AtLeastOnce();
				Expect.Call(_shiftCategoryWeekRemover.Remove(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixPro, _optimizationPreferences)).Return(_scheduleDayPros);

				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, new BetweenDayOffBlockFinder(), true)).IgnoreArguments().Return(_teamBlockInfo);

				Expect.Call(_shiftCategoryLimitCounter.HaveMaxOfShiftCategory(_shiftCategoryLimitation, _teamInfo, _dateOnly)).Return(false);
				
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder))).IgnoreArguments().Return(false);

				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder))).IgnoreArguments().Return(true);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
			}

			using (_mock.Playback())
			{
				_schedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight("test", GroupPageType.SingleAgent);
				_target.Execute(_schedulingOptions, _scheduleMatrixPro, _schedulingResultStateHolder, _rollbackService, _resourceCalculateDelayer, _scheduleMatrixPros, _shiftNudgeDirective, _optimizationPreferences);
				Assert.IsNotEmpty(_schedulingOptions.NotAllowedShiftCategories);
			}	
		}

		[Test]
		public void ShouldRollbackIfNotPossibleToSchedule()
		{
			_shiftCategoryLimitation.Weekly = true;
			var shiftCategoryLimitations = new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation> { _shiftCategoryLimitation });

			using (_mock.Record())
			{
				Expect.Call(_virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(shiftCategoryLimitations).Repeat.AtLeastOnce();
				Expect.Call(_shiftCategoryWeekRemover.Remove(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixPro, _optimizationPreferences)).Return(_scheduleDayPros);

				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, new BetweenDayOffBlockFinder(), true)).IgnoreArguments().Return(_teamBlockInfo);

				Expect.Call(_shiftCategoryLimitCounter.HaveMaxOfShiftCategory(_shiftCategoryLimitation, _teamInfo, _dateOnly)).Return(false);

				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder))).IgnoreArguments().Return(false);

				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder))).IgnoreArguments().Return(false);

				Expect.Call(() => _safeRollbackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));

				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				
			}

			using (_mock.Playback())
			{
				_schedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight("test", GroupPageType.SingleAgent);
				_target.Execute(_schedulingOptions, _scheduleMatrixPro, _schedulingResultStateHolder, _rollbackService, _resourceCalculateDelayer, _scheduleMatrixPros, _shiftNudgeDirective, _optimizationPreferences);
				Assert.IsNotEmpty(_schedulingOptions.NotAllowedShiftCategories);
			}		
		}

		[Test]
		public void ShouldAddNotAllowedShiftCategoryWhenOnMax()
		{
			var shiftCategoryLimitationOther = new ShiftCategoryLimitation(new ShiftCategory("shiftCategoryOther"));
			shiftCategoryLimitationOther.Weekly = true;

			_shiftCategoryLimitation.Weekly = true;
			var shiftCategoryLimitations = new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation> { _shiftCategoryLimitation, shiftCategoryLimitationOther });

			using (_mock.Record())
			{
				Expect.Call(_virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(shiftCategoryLimitations).Repeat.AtLeastOnce();

				Expect.Call(_shiftCategoryWeekRemover.Remove(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixPro, _optimizationPreferences)).Return(_scheduleDayPros);
				Expect.Call(_shiftCategoryWeekRemover.Remove(shiftCategoryLimitationOther, _schedulingOptions, _scheduleMatrixPro, _optimizationPreferences)).Return(new List<IScheduleDayPro>());

				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(false);

				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, new BetweenDayOffBlockFinder(), true)).IgnoreArguments().Return(_teamBlockInfo);

				Expect.Call(_shiftCategoryLimitCounter.HaveMaxOfShiftCategory(_shiftCategoryLimitation, _teamInfo, _dateOnly)).Return(false).IgnoreArguments();
				Expect.Call(_shiftCategoryLimitCounter.HaveMaxOfShiftCategory(shiftCategoryLimitationOther, _teamInfo, _dateOnly)).Return(true).IgnoreArguments();

				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder))).IgnoreArguments().Return(true);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
			}

			using (_mock.Playback())
			{
				_schedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight("test", GroupPageType.SingleAgent);
				_target.Execute(_schedulingOptions, _scheduleMatrixPro, _schedulingResultStateHolder, _rollbackService, _resourceCalculateDelayer, _scheduleMatrixPros, _shiftNudgeDirective, _optimizationPreferences);
				Assert.AreEqual(2, _schedulingOptions.NotAllowedShiftCategories.Count);
			}
		}	
	}
}
