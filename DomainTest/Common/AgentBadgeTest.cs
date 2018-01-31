using System;
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
	public class AgentBadgeTest
	{
		private const int silverToBronzeRate = 5;
		private const int goldToSilverRate = 2;

		[Test]
		public void ShouldGetIsExternalInfo()
		{
			var result = AgentBadge.FromAgentBadgeTransaction(
				new List<IAgentBadgeTransaction> {new AgentBadgeTransaction {IsExternal = true, Person = PersonFactory.CreatePerson(), BadgeType = 8}});
			result.First().IsExternal.Should().Be.EqualTo(true);
		}

	    [Test]
	    public void ShouldAwardSilverBadgeWhenBronzeBadgeMatchesSettingRate()
	    {
			var target = new AgentBadge
			{
				Person = Guid.NewGuid(),
				BadgeType = BadgeType.Adherence,
				TotalAmount = 4
			};

		    target.TotalAmount += 1;

		    Assert.AreEqual(target.GetBronzeBadge(silverToBronzeRate, goldToSilverRate), 0);
			Assert.AreEqual(target.IsBronzeBadgeAdded(silverToBronzeRate, goldToSilverRate), false);

			Assert.AreEqual(target.GetSilverBadge(silverToBronzeRate, goldToSilverRate), 1);
			Assert.AreEqual(target.IsSilverBadgeAdded(silverToBronzeRate, goldToSilverRate), true);

			Assert.AreEqual(target.GetGoldBadge(silverToBronzeRate, goldToSilverRate), 0);
			Assert.AreEqual(target.IsGoldBadgeAdded(silverToBronzeRate, goldToSilverRate), false);
	    }

	    [Test]
	    public void ShouldAwardGoldBadgeWhenSilverBadgeMatchesSettingRate()
		{
			var target = new AgentBadge
			{
				Person = Guid.NewGuid(),
				BadgeType = BadgeType.AverageHandlingTime,
				TotalAmount = 9
			};

		    target.TotalAmount = 10;


			Assert.AreEqual(target.GetBronzeBadge(silverToBronzeRate, goldToSilverRate), 0);
			Assert.AreEqual(target.IsBronzeBadgeAdded(silverToBronzeRate, goldToSilverRate), false);

			Assert.AreEqual(target.GetSilverBadge(silverToBronzeRate, goldToSilverRate), 0);
			Assert.AreEqual(target.IsSilverBadgeAdded(silverToBronzeRate, goldToSilverRate), false);

			Assert.AreEqual(target.GetGoldBadge(silverToBronzeRate, goldToSilverRate), 1);
			Assert.AreEqual(target.IsGoldBadgeAdded(silverToBronzeRate, goldToSilverRate), true);
	    }
	}
}