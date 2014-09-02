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
			var person = PersonFactory.CreatePerson("_");
			var target = new AgentBadge
			{
				Person = person,
				BadgeType = BadgeType.Adherence,
				BronzeBadge = 4
			};

			target.AddBadge(new AgentBadge
			{
				Person = person,
				BadgeType = BadgeType.Adherence,
				BronzeBadge = 1,
				BronzeBadgeAdded = true,
				LastCalculatedDate = new DateOnly(2014, 01, 01)
			}, silverToBronzeRate, goldToSilverRate);

			Assert.AreEqual(target.BronzeBadge, 0);
			Assert.AreEqual(target.BronzeBadgeAdded, false);

			Assert.AreEqual(target.SilverBadge, 1);
			Assert.AreEqual(target.SilverBadgeAdded, true);

			Assert.AreEqual(target.GoldBadge, 0);
			Assert.AreEqual(target.GoldBadgeAdded, false);

		    Assert.AreEqual(target.LastCalculatedDate, new DateOnly(2014, 01, 01));
	    }

	    [Test]
	    public void ShouldAwardGoldBadgeWhenSilverBadgeMatchesSettingRate()
		{
			var person = PersonFactory.CreatePerson("_");
			var target = new AgentBadge
			{
				Person = person,
				BadgeType = BadgeType.Adherence,
				BronzeBadge = 4,
				SilverBadge = 1
			};

			target.AddBadge(new AgentBadge
			{
				Person = person,
				BadgeType = BadgeType.Adherence,
				BronzeBadge = 1,
				BronzeBadgeAdded = true,
				LastCalculatedDate = new DateOnly(2014, 01, 01)
			}, silverToBronzeRate, goldToSilverRate);

			Assert.AreEqual(target.BronzeBadge, 0);
			Assert.AreEqual(target.BronzeBadgeAdded, false);

			Assert.AreEqual(target.SilverBadge, 0);
		    Assert.AreEqual(target.SilverBadgeAdded, false);

			Assert.AreEqual(target.GoldBadge, 1);
			Assert.AreEqual(target.GoldBadgeAdded, true);

			Assert.AreEqual(target.LastCalculatedDate, new DateOnly(2014, 01, 01));
	    }

		[Test]
		public void ShouldThrownExceptionWhenAddDifferentTypeBadge()
		{
			var person = PersonFactory.CreatePerson("_");
			var target = new AgentBadge
			{
				Person = person,
				BadgeType = BadgeType.Adherence,
				BronzeBadge = 1
			};

			var newBadge = new AgentBadge
			{
				Person = person,
				BadgeType = BadgeType.AnsweredCalls,
				BronzeBadge = 1,
				BronzeBadgeAdded = true,
				LastCalculatedDate = new DateOnly(2014, 01, 01)
			};

			Assert.Throws<ArgumentException>(()=> target.AddBadge(newBadge, silverToBronzeRate, goldToSilverRate));
		}

		[Test]
		public void ShouldThrownExceptionWhenRateNotLargerThanZero()
		{
			var person = PersonFactory.CreatePerson("_");
			var target = new AgentBadge
			{
				Person = person,
				BadgeType = BadgeType.AnsweredCalls,
				BronzeBadge = 1
			};

			var newBadge = new AgentBadge
			{
				Person = person,
				BadgeType = BadgeType.AnsweredCalls,
				BronzeBadge = 1,
				BronzeBadgeAdded = true,
				LastCalculatedDate = new DateOnly(2014, 01, 01)
			};

			Assert.Throws<ArgumentOutOfRangeException>(() => target.AddBadge(newBadge, 0, goldToSilverRate));
			Assert.Throws<ArgumentOutOfRangeException>(() => target.AddBadge(newBadge, -1, goldToSilverRate));

			Assert.Throws<ArgumentOutOfRangeException>(() => target.AddBadge(newBadge, silverToBronzeRate, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => target.AddBadge(newBadge, silverToBronzeRate, -1));
		}
	}
}