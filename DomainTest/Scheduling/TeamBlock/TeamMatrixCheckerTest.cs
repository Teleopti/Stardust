using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;



namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamMatrixCheckerTest
	{
		private MockRepository _mocks;
		private TeamMatrixChecker _target;
		private IEnumerable<ITeamInfo> _teamInfoList;
		private DateOnlyPeriod _period;
		private ITeamInfo _teamInfo1;
		private ITeamInfo _teamInfo2;
		private IPerson _member1_1;
		private IPerson _member1_2;
		private IPerson _member2_1;
		private IScheduleMatrixPro _matrix;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new TeamMatrixChecker();
			_period = new DateOnlyPeriod();
			_teamInfo1 = _mocks.StrictMock<ITeamInfo>();
			_teamInfo2 = _mocks.StrictMock<ITeamInfo>();
			_teamInfoList = new List<ITeamInfo> {_teamInfo1, _teamInfo2};
			_member1_1 = new Person();
			_member1_2 = new Person();
			_member2_1 = new Person();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
		}

		[Test]
		public void ShouldFilterOutTeamsWithMembersWithNoMatrix()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamInfo1.GroupMembers).Return(new List<IPerson> { _member1_1, _member1_2 });
				Expect.Call(_teamInfo1.MatrixesForMemberAndPeriod(_member1_1, _period)).Return(new List<IScheduleMatrixPro>()); //empty list member2 will be skipped

				Expect.Call(_teamInfo2.GroupMembers).Return(new List<IPerson> { _member2_1});
				Expect.Call(_teamInfo2.MatrixesForMemberAndPeriod(_member2_1, _period)).Return(new List<IScheduleMatrixPro>{_matrix});
			}

			using (_mocks.Playback())
			{
				var result = _target.CheckTeamList(_teamInfoList, _period).Single();
				Assert.AreEqual(_teamInfo2, result);
			}
		}
	}
}