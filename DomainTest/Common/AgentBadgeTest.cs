using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class AgentBadgeTest
	{
		private const int silverToBronzeRate = 5;
		private const int goldToSilverRate = 2;
		
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