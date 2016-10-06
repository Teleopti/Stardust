using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class AgentBadgeWithRankTransactionRepositoryTest : RepositoryTest<IAgentBadgeWithRankTransaction>
	{
		private IPerson person;

		protected override void ConcreteSetup()
		{
			person = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person);
		}

		protected override IAgentBadgeWithRankTransaction CreateAggregateWithCorrectBusinessUnit()
		{
			return new AgentBadgeWithRankTransaction
			{
				BronzeBadgeAmount = 3,
				SilverBadgeAmount = 2,
				GoldBadgeAmount = 1,
				BadgeType = BadgeType.AverageHandlingTime,
				CalculatedDate = new DateOnly(2014, 9, 9),
				Description = "test",
				InsertedOn = new DateTime(2014, 9, 10),
				Person = person
			};
		}

		protected override void VerifyAggregateGraphProperties(IAgentBadgeWithRankTransaction loadedAggregateFromDatabase)
		{
			var newBadgeTran = CreateAggregateWithCorrectBusinessUnit();

			Assert.AreEqual(newBadgeTran.Person.Name, loadedAggregateFromDatabase.Person.Name);
			Assert.AreEqual(newBadgeTran.BadgeType, loadedAggregateFromDatabase.BadgeType);
			Assert.AreEqual(newBadgeTran.BronzeBadgeAmount, loadedAggregateFromDatabase.BronzeBadgeAmount);
			Assert.AreEqual(newBadgeTran.SilverBadgeAmount, loadedAggregateFromDatabase.SilverBadgeAmount);
			Assert.AreEqual(newBadgeTran.GoldBadgeAmount, loadedAggregateFromDatabase.GoldBadgeAmount);
			Assert.AreEqual(newBadgeTran.CalculatedDate, loadedAggregateFromDatabase.CalculatedDate);
			Assert.AreEqual(newBadgeTran.Description, loadedAggregateFromDatabase.Description);
			Assert.AreEqual(newBadgeTran.InsertedOn, loadedAggregateFromDatabase.InsertedOn);
		}

		protected override Repository<IAgentBadgeWithRankTransaction> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new AgentBadgeWithRankTransactionRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldResetBadges()
		{
			var badgeTransaction = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(badgeTransaction);

			var target = new AgentBadgeWithRankTransactionRepository(UnitOfWork);
			target.ResetAgentBadges();

			var result = target.Find(person,badgeTransaction.BadgeType,badgeTransaction.CalculatedDate);
			result.Should().Be.Null();
		}


		[Test]
		public void ShouldGetBadgeWithinGivenDatePeriod()
		{
			var badge1 = CreateAggregateWithCorrectBusinessUnit();
			badge1.CalculatedDate = new DateOnly(2014,9,9);
			PersistAndRemoveFromUnitOfWork(badge1);
			var badge2 = CreateAggregateWithCorrectBusinessUnit();
			badge2.CalculatedDate = new DateOnly(2014,9,15);
			PersistAndRemoveFromUnitOfWork(badge2);

			var target = new AgentBadgeWithRankTransactionRepository(UnitOfWork);


			var badges = target.Find(new List<IPerson> { badge1.Person,badge2.Person },new DateOnlyPeriod(2014,9,8,2014,9,10));

			badges.Count.Should().Be.EqualTo(1);

			badges = target.Find(new List<IPerson> { badge1.Person,badge2.Person },new DateOnlyPeriod(2014,9,8,2014,9,16));

			badges.Count.Should().Be.EqualTo(2);

		}
	}
}