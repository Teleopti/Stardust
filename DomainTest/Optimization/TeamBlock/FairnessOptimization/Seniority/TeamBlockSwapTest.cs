using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class TeamBlockSwapTest
	{
		private MockRepository _mock;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private ISwapServiceNew _swapServiceNew;
		private TeamBlockSwap _target;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IScheduleDictionary _scheduleDictionary;
		private ITeamBlockSwapValidator _teamBlockSwapValidator;
		private ITeamBlockSwapDayValidator _teamBlockSwapDayValidator;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_scheduleDay1 = ScheduleDayFactory.Create(new DateOnly(2013, 1, 1));
			_scheduleDay2 = ScheduleDayFactory.Create(new DateOnly(2013, 1, 1));
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_swapServiceNew = _mock.StrictMock<ISwapServiceNew>();
			_scheduleMatrixPro1 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_teamBlockSwapValidator = _mock.StrictMock<ITeamBlockSwapValidator>();
			_teamBlockSwapDayValidator = _mock.StrictMock<ITeamBlockSwapDayValidator>();
			_target = new TeamBlockSwap(_swapServiceNew, _scheduleDictionary, _teamBlockSwapValidator, _teamBlockSwapDayValidator);
		}

		[Test]
		public void ShouldSwap()
		{
			var scheduleMatrixPros1 = new List<IScheduleMatrixPro> {_scheduleMatrixPro1};
			var scheduleMatrixPros2 = new List<IScheduleMatrixPro> {_scheduleMatrixPro2};
			var scheduleDayPros1 = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>{_scheduleDayPro1});
 			var scheduleDayPros2 = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>{_scheduleDayPro2});
			var swappedList = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};

			using (_mock.Record())
			{
				Expect.Call(_teamBlockSwapValidator.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockInfo1.MatrixesForGroupAndBlock()).Return(scheduleMatrixPros1);
				Expect.Call(_teamBlockInfo2.MatrixesForGroupAndBlock()).Return(scheduleMatrixPros2);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(scheduleDayPros1);
				Expect.Call(_scheduleMatrixPro2.EffectivePeriodDays).Return(scheduleDayPros2);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
				Expect.Call(_teamBlockSwapDayValidator.ValidateSwapDays(_scheduleDay1, _scheduleDay2)).Return(true);
				Expect.Call(_swapServiceNew.Swap(new List<IScheduleDay> {_scheduleDay1, _scheduleDay2}, _scheduleDictionary)).Return(swappedList);
			}

			using (_mock.Playback())
			{
				var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldNotSwapIfValidatorFails()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockSwapValidator.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(false);	
			}

			using (_mock.Playback())
			{
				var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);	
			}
		}

		[Test]
		public void ShouldNotSwapIfValidatorDayFails()
		{
			var scheduleMatrixPros1 = new List<IScheduleMatrixPro> { _scheduleMatrixPro1 };
			var scheduleMatrixPros2 = new List<IScheduleMatrixPro> { _scheduleMatrixPro2 };
			var scheduleDayPros1 = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro1 });
			var scheduleDayPros2 = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro2 });

			using (_mock.Record())
			{
				Expect.Call(_teamBlockSwapValidator.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockInfo1.MatrixesForGroupAndBlock()).Return(scheduleMatrixPros1);
				Expect.Call(_teamBlockInfo2.MatrixesForGroupAndBlock()).Return(scheduleMatrixPros2);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(scheduleDayPros1);
				Expect.Call(_scheduleMatrixPro2.EffectivePeriodDays).Return(scheduleDayPros2);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
				Expect.Call(_teamBlockSwapDayValidator.ValidateSwapDays(_scheduleDay1, _scheduleDay2)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}	
		}
	}
}
