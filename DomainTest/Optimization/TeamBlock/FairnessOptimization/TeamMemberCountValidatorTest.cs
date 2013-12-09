using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class TeamMemberCountValidatorTest
	{
		private TeamMemberCountValidator _target;
		private MockRepository _mock;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private ITeamInfo _teamInfo1;
		private ITeamInfo _teamInfo2;
		private IGroupPerson _groupPerson1;
		private IGroupPerson _groupPerson2;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_teamInfo1 = _mock.StrictMock<ITeamInfo>();
			_teamInfo2 = _mock.StrictMock<ITeamInfo>();
			_groupPerson1 = _mock.StrictMock<IGroupPerson>();
			_groupPerson2 = _mock.StrictMock<IGroupPerson>();
			_person = PersonFactory.CreatePerson("Person", "Person");
			_target = new TeamMemberCountValidator();	
		}

		[Test]
		public void ShouldReturnTrueWhenSameMemberCount()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
				Expect.Call(_teamInfo1.GroupPerson).Return(_groupPerson1);
				Expect.Call(_teamInfo2.GroupPerson).Return(_groupPerson2);
				Expect.Call(_groupPerson1.GroupMembers).Return(new List<IPerson>());
				Expect.Call(_groupPerson2.GroupMembers).Return(new List<IPerson>());
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateMemberCount(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsTrue(result);
			}	
		}

		[Test]
		public void ShouldReturnFalseWenNotSameMemberCount()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
				Expect.Call(_teamInfo1.GroupPerson).Return(_groupPerson1);
				Expect.Call(_teamInfo2.GroupPerson).Return(_groupPerson2);
				Expect.Call(_groupPerson1.GroupMembers).Return(new List<IPerson>());
				Expect.Call(_groupPerson2.GroupMembers).Return(new List<IPerson>{_person});
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateMemberCount(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}
		}
	}
}
