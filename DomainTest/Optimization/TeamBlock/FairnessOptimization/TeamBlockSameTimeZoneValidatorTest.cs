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
	public class TeamBlockSameTimeZoneValidatorTest
	{
		private MockRepository _mocks;
		private ITeamBlockSameTimeZoneValidator _target;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private ITeamInfo _teamInfo1;
		private ITeamInfo _teamInfo2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new TeamBlockSameTimeZoneValidator();
			_teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mocks.StrictMock<ITeamBlockInfo>();
			_teamInfo1 = _mocks.StrictMock<ITeamInfo>();
			_teamInfo2 = _mocks.StrictMock<ITeamInfo>();
		}

		[Test]
		public void ShouldReturnFalseIfNotSameTimeZone()
		{
			var person1 = PersonFactory.CreatePerson();
			person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var person2 = PersonFactory.CreatePerson();
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			var members1 = new List<IPerson> {person1};
			var members2 = new List<IPerson> {person2};

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamInfo1.GroupMembers).Return(members1);
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
				Expect.Call(_teamInfo2.GroupMembers).Return(members2);
			}

			using (_mocks.Playback())
			{
				bool result = _target.Validate(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnTrueIfSameTimeZone()
		{
			var person1 = PersonFactory.CreatePerson();
			person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var person2 = PersonFactory.CreatePerson();
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var members1 = new List<IPerson> { person1 };
			var members2 = new List<IPerson> { person2 };

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamInfo1.GroupMembers).Return(members1);
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
				Expect.Call(_teamInfo2.GroupMembers).Return(members2);
			}

			using (_mocks.Playback())
			{
				bool result = _target.Validate(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsTrue(result);
			}
		}
	}
}