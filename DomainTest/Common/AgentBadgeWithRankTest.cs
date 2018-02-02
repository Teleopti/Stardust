using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class AgentBadgeWithRankTest
	{
		[Test]
		public void ShouldGetIsExternalInfo()
		{
			var result = AgentBadgeWithRank.FromAgentBadgeWithRanksTransaction(
				new List<IAgentBadgeWithRankTransaction> { new AgentBadgeWithRankTransaction { IsExternal = true, Person = PersonFactory.CreatePerson(), BadgeType = 8 } });
			result.First().IsExternal.Should().Be.EqualTo(true);
		}
	}
}
