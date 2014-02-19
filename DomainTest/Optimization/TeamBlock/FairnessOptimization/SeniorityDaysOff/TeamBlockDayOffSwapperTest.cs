using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	[TestFixture]
	public class TeamBlockDayOffSwapTest
	{
		private MockRepository _mocks;
		private ITeamBlockDayOffSwapper _target;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private ISwapServiceNew _swapServiceNew;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IPerson _personSenior;
		private IPerson _personJunior;
		private Group _groupSenior;
		private Group _groupJunior;
		private IScheduleMatrixPro _matrixSenior;
		private TeamInfo _teamSenior;
		private TeamInfo _teamJunior;
		private TeamBlockInfo _teamBlockInfoSenior;
		private TeamBlockInfo _teamBlockInfoJunior;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_swapServiceNew = _mocks.StrictMock<ISwapServiceNew>();
			_target = new TeamBlockDayOffSwapper(_swapServiceNew);

			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_personSenior = PersonFactory.CreatePerson();
			_personJunior = PersonFactory.CreatePerson();
			_groupSenior = new Group(new List<IPerson> { _personSenior }, "Senior");
			_groupJunior = new Group(new List<IPerson> { _personJunior }, "Junior");
			_matrixSenior = _mocks.StrictMock<IScheduleMatrixPro>();
			IList<IList<IScheduleMatrixPro>> groupMatrixesSenior = new List<IList<IScheduleMatrixPro>>();
			groupMatrixesSenior.Add(new List<IScheduleMatrixPro> { _matrixSenior });
			IList<IList<IScheduleMatrixPro>> groupMatrixesJunior = new List<IList<IScheduleMatrixPro>>();
			groupMatrixesJunior.Add(new List<IScheduleMatrixPro> { _matrixSenior });
			_teamSenior = new TeamInfo(_groupSenior, groupMatrixesSenior);
			_teamJunior = new TeamInfo(_groupJunior, groupMatrixesJunior);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 1, 27, 2014, 1, 27));
			_teamBlockInfoSenior = new TeamBlockInfo(_teamSenior, blockInfo);
			_teamBlockInfoJunior = new TeamBlockInfo(_teamJunior, blockInfo);
			_range1 = _mocks.StrictMock<IScheduleRange>();
			_range2 = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
		}

		[Test]
		public void ShouldNotSwapIfNeitherIsDayOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_personSenior]).Return(_range1);
				Expect.Call(_range1.ScheduledDay(new DateOnly(2014, 1, 27))).Return(_scheduleDay1);
				Expect.Call(_scheduleDictionary[_personJunior]).Return(_range2);
				Expect.Call(_range2.ScheduledDay(new DateOnly(2014, 1, 27))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_rollbackService.ModifyParts(new List<IScheduleDay>())).Return(new List<IBusinessRuleResponse>());
				Expect.Call(() => _rollbackService.ClearModificationCollection());
			}

			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService, _scheduleDictionary);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldNotSwapIfBothAreDayOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_personSenior]).Return(_range1);
				Expect.Call(_range1.ScheduledDay(new DateOnly(2014, 1, 27))).Return(_scheduleDay1);
				Expect.Call(_scheduleDictionary[_personJunior]).Return(_range2);
				Expect.Call(_range2.ScheduledDay(new DateOnly(2014, 1, 27))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_rollbackService.ModifyParts(new List<IScheduleDay>())).Return(new List<IBusinessRuleResponse>());
				Expect.Call(() => _rollbackService.ClearModificationCollection());
			}

			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService, _scheduleDictionary);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldSwapIfOneOfThemIsDayOff()
		{
			var swapList = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 };
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_personSenior]).Return(_range1);
				Expect.Call(_range1.ScheduledDay(new DateOnly(2014, 1, 27))).Return(_scheduleDay1);
				Expect.Call(_scheduleDictionary[_personJunior]).Return(_range2);
				Expect.Call(_range2.ScheduledDay(new DateOnly(2014, 1, 27))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_swapServiceNew.Swap(swapList, _scheduleDictionary)).Return(swapList);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_rollbackService.ModifyParts(swapList)).Return(new List<IBusinessRuleResponse>());
				Expect.Call(() => _rollbackService.ClearModificationCollection());
			}

			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService, _scheduleDictionary);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldNotSwapIfAnyBusinessRuleResponse()
		{
			var swapList = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 };
			IBusinessRuleResponse response = new BusinessRuleResponse(typeof(NewDayOffRule), "", true, false,
																	  new DateTimePeriod(), _personSenior,
																	  new DateOnlyPeriod());
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDictionary[_personSenior]).Return(_range1);
				Expect.Call(_range1.ScheduledDay(new DateOnly(2014, 1, 27))).Return(_scheduleDay1);
				Expect.Call(_scheduleDictionary[_personJunior]).Return(_range2);
				Expect.Call(_range2.ScheduledDay(new DateOnly(2014, 1, 27))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_swapServiceNew.Swap(swapList, _scheduleDictionary)).Return(swapList);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_rollbackService.ModifyParts(swapList)).Return(new List<IBusinessRuleResponse> { response });
				Expect.Call(() => _rollbackService.Rollback());
			}

			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_teamBlockInfoSenior, _teamBlockInfoJunior, _rollbackService, _scheduleDictionary);
				Assert.IsFalse(result);
			}
		}
	}
}
