using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class TeamBlockLockValidatorTest
	{
		private TeamBlockLockValidator _validator;
		private MockRepository _mock;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private IBlockInfo _blockInfo1;
		private IBlockInfo _blockInfo2;
		private ITeamInfo _teamInfo1;
		private ITeamInfo _teamInfo2;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private DateOnly _dateOnly;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_dateOnly = new DateOnly(2013, 1, 1);
			_dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_blockInfo1 = new BlockInfo(_dateOnlyPeriod);
			_blockInfo2 = new BlockInfo(_dateOnlyPeriod);
			_teamInfo1 = _mock.StrictMock<ITeamInfo>();
			_teamInfo2 = _mock.StrictMock<ITeamInfo>();
			_scheduleMatrixPro1 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
			_validator = new TeamBlockLockValidator();	
		}

		[Test]
		public void ShouldReturnTrueWhenNoLocks()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo1);
				Expect.Call(_teamBlockInfo2.BlockInfo).Return(_blockInfo2);
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
				Expect.Call(_teamInfo1.MatrixesForGroupAndPeriod(_dateOnlyPeriod)).Return(new List<IScheduleMatrixPro> {_scheduleMatrixPro1});
				Expect.Call(_teamInfo2.MatrixesForGroupAndPeriod(_dateOnlyPeriod)).Return(new List<IScheduleMatrixPro> { _scheduleMatrixPro2 });
				Expect.Call(_scheduleMatrixPro1.UnlockedDays).Return(new HashSet<IScheduleDayPro> { _scheduleDayPro1});
				Expect.Call(_scheduleMatrixPro2.UnlockedDays).Return(new HashSet<IScheduleDayPro> { _scheduleDayPro2});
				Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly);
				Expect.Call(_scheduleDayPro2.Day).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				Assert.IsTrue(_validator.ValidateLocks(_teamBlockInfo1, _teamBlockInfo2));	
			}
		}

		[Test]
		public void ShouldReturnFalseWhenLocks()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo1);
				Expect.Call(_teamBlockInfo2.BlockInfo).Return(_blockInfo2);
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
				Expect.Call(_teamInfo1.MatrixesForGroupAndPeriod(_dateOnlyPeriod)).Return(new List<IScheduleMatrixPro> { _scheduleMatrixPro1 });
				Expect.Call(_teamInfo2.MatrixesForGroupAndPeriod(_dateOnlyPeriod)).Return(new List<IScheduleMatrixPro> { _scheduleMatrixPro2 });
				Expect.Call(_scheduleMatrixPro1.UnlockedDays).Return(new HashSet<IScheduleDayPro> { _scheduleDayPro1 });
				Expect.Call(_scheduleMatrixPro2.UnlockedDays).Return(new HashSet<IScheduleDayPro>());
				Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				Assert.IsFalse(_validator.ValidateLocks(_teamBlockInfo1, _teamBlockInfo2));
			}		
		}
	}
}
