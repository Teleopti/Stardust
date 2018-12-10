using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;



namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockScheduleClonerTest
	{
		private MockRepository _mocks;
		private TeamBlockScheduleCloner _target;
		private ITeamBlockInfo _teamBlockInfo;
		private IBlockInfo _blockInfo;
		private ITeamInfo _teamInfo;
		private IPerson _person;
		private IScheduleRange _range;
		private IScheduleMatrixPro _matrix;
		private IScheduleDay _scheduleDay;
		private IPerson _personWithoutSchedulePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new TeamBlockScheduleCloner();
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_blockInfo = _mocks.StrictMock<IBlockInfo>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_person = PersonFactory.CreatePerson();
			_personWithoutSchedulePeriod = PersonFactory.CreatePerson();
			_range = _mocks.StrictMock<IScheduleRange>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
		}

		[Test]
		public void ShouldClone()
		{

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo).Repeat.AtLeastOnce();
				Expect.Call(_blockInfo.BlockPeriod).Return(new DateOnlyPeriod(2014, 3, 24, 2014, 3, 24)).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> {_person});
				Expect.Call(_teamInfo.MatrixForMemberAndDate(_person, new DateOnly(2014, 3, 24))).Return(_matrix);
				Expect.Call(_matrix.ActiveScheduleRange).Return(_range);
				Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2014, 3, 24,2014,3,24))).Return(new [] { _scheduleDay});
				Expect.Call(_scheduleDay.Clone()).Return(_scheduleDay);
			}

			using (_mocks.Playback())
			{
				var result = _target.CloneSchedules(_teamBlockInfo);
				Assert.AreEqual(1, result.Count());
			}
		}

		[Test]
		public void ShouldIgnoreTeamMemeberWithourSchedulePeriod()
		{

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo).Repeat.AtLeastOnce();
				Expect.Call(_blockInfo.BlockPeriod).Return(new DateOnlyPeriod(2014, 3, 24, 2014, 3, 24)).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person,_personWithoutSchedulePeriod });
				Expect.Call(_teamInfo.MatrixForMemberAndDate(_person, new DateOnly(2014, 3, 24))).Return(_matrix);
				Expect.Call(_teamInfo.MatrixForMemberAndDate(_personWithoutSchedulePeriod, new DateOnly(2014, 3, 24))).Return(null);
				Expect.Call(_matrix.ActiveScheduleRange).Return(_range);
				Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2014, 3, 24, 2014, 3, 24))).Return(new[] { _scheduleDay });
				Expect.Call(_scheduleDay.Clone()).Return(_scheduleDay);
			}

			using (_mocks.Playback())
			{
				var result = _target.CloneSchedules(_teamBlockInfo);
				Assert.AreEqual(1, result.Count());
			}
		}

	}
}