using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
		private IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculatorPro;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IList<IScheduleMatrixPro> _scheduleMatrixPros; 
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private IScheduleDayPro _scheduleDayPro;
		private IList<IScheduleDayPro> _scheduleDayPros;
		private DateOnly _dateOnly;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IScheduleDay _scheduleDay;
		private IPerson _person;
		private ITeamInfo _teamInfo;
		private ITeamBlockInfo _teamBlockInfo;
		private ShiftNudgeDirective _shiftNudgeDirective;
		private IList<DateOnly> _unlockedDates;
		private IBlockInfo _blockInfo;
		
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
			_scheduleMatrixValueCalculatorPro = _mock.StrictMock<IScheduleMatrixValueCalculatorPro>();
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPros = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPros = new List<IScheduleDayPro> { _scheduleDayPro };
			_dateOnly = new DateOnly(2015, 1, 1);
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_person = PersonFactory.CreatePerson();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_shiftNudgeDirective = new ShiftNudgeDirective();
			_unlockedDates = new List<DateOnly> {_dateOnly};
			_blockInfo = _mock.StrictMock<IBlockInfo>();
			_schedulingOptions.NotAllowedShiftCategories.Add(_shiftCategory);

			_target = new TeamBlockRemoveShiftCategoryBackToLegalService(_teamBlockScheduler,_teamInfoFactory,_teamBlockInfoFactory,_teamBlockClearer,_teamBlockSchedulingOptions,_shiftCategoryWeekRemover,_shiftCategoryPeriodRemover);	
		}

		[Test]
		public void ShouldExecuteOnWeekRestriction()
		{
			_shiftCategoryLimitation.Weekly = true;
			var shiftCategoryLimitations = new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation>{_shiftCategoryLimitation});
			
			using (_mock.Record())
			{
				Expect.Call(_virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(shiftCategoryLimitations);
				Expect.Call(_shiftCategoryWeekRemover.Remove(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro)).Return(_scheduleDayPros);
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, BlockFinderType.BetweenDayOff, false)).Return(_teamBlockInfo);
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Execute(_virtualSchedulePeriod, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro, _schedulingResultStateHolder, _rollbackService, _resourceCalculateDelayer, _scheduleMatrixPros, _shiftNudgeDirective);
				Assert.IsTrue(result);
				Assert.IsEmpty(_schedulingOptions.NotAllowedShiftCategories);
			}
		}

		[Test]
		public void ShouldExecuteOnPeriodRestriction()
		{
			_shiftCategoryLimitation.Weekly = false;
			var shiftCategoryLimitations = new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation> { _shiftCategoryLimitation });

			using (_mock.Record())
			{
				Expect.Call(_virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(shiftCategoryLimitations);
				Expect.Call(_shiftCategoryPeriodRemover.RemoveShiftCategoryOnPeriod(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro)).Return(_scheduleDayPros);
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, BlockFinderType.BetweenDayOff, false)).Return(_teamBlockInfo);
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Execute(_virtualSchedulePeriod, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro, _schedulingResultStateHolder, _rollbackService, _resourceCalculateDelayer, _scheduleMatrixPros, _shiftNudgeDirective);
				Assert.IsTrue(result);
				Assert.IsEmpty(_schedulingOptions.NotAllowedShiftCategories);
			}	
		}

		[Test]
		public void ShouldClearTeamBlockAndRetryIfNoSuccesOnOneDay()
		{
			_shiftCategoryLimitation.Weekly = true;
			var shiftCategoryLimitations = new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation> { _shiftCategoryLimitation });

			using (_mock.Record())
			{
				Expect.Call(_virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(shiftCategoryLimitations);
				Expect.Call(_shiftCategoryWeekRemover.Remove(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro)).Return(_scheduleDayPros);
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, BlockFinderType.BetweenDayOff, false)).Return(_teamBlockInfo);
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective)).Return(false);

				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Execute(_virtualSchedulePeriod, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro, _schedulingResultStateHolder, _rollbackService, _resourceCalculateDelayer, _scheduleMatrixPros, _shiftNudgeDirective);
				Assert.IsTrue(result);
				Assert.IsEmpty(_schedulingOptions.NotAllowedShiftCategories);
			}	
		}

		[Test]
		public void ShouldRollbackIfNotPossibleToSchedule()
		{
			_shiftCategoryLimitation.Weekly = true;
			var shiftCategoryLimitations = new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation> { _shiftCategoryLimitation });

			using (_mock.Record())
			{
				Expect.Call(_virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(shiftCategoryLimitations);
				Expect.Call(_shiftCategoryWeekRemover.Remove(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro)).Return(_scheduleDayPros);
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, BlockFinderType.BetweenDayOff, false)).Return(_teamBlockInfo);
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective)).Return(false);

				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective)).Return(false);

				Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.UnLockedDates()).Return(_unlockedDates);
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Execute(_virtualSchedulePeriod, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro, _schedulingResultStateHolder, _rollbackService, _resourceCalculateDelayer, _scheduleMatrixPros, _shiftNudgeDirective);
				Assert.IsFalse(result);
				Assert.IsEmpty(_schedulingOptions.NotAllowedShiftCategories);
			}		
		}

		[Test]
		public void ShouldNotClearNotAllowedShiftCategoriesWhenTeamScheduling()
		{
			_schedulingOptions.UseTeam = true;
			_shiftCategoryLimitation.Weekly = true;
			var shiftCategoryLimitations = new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation> { _shiftCategoryLimitation });

			using (_mock.Record())
			{
				Expect.Call(_virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(shiftCategoryLimitations);
				Expect.Call(_shiftCategoryWeekRemover.Remove(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro)).Return(_scheduleDayPros);
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, BlockFinderType.BetweenDayOff, false)).Return(_teamBlockInfo);
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Execute(_virtualSchedulePeriod, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro, _schedulingResultStateHolder, _rollbackService, _resourceCalculateDelayer, _scheduleMatrixPros, _shiftNudgeDirective);
				Assert.IsTrue(result);
				Assert.IsNotEmpty(_schedulingOptions.NotAllowedShiftCategories);
			}
		}
	}
}
