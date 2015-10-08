using System;
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
	[Category("LongRunning")]
	public class AgentBadgeWithRankTransactionRepositoryTest : RepositoryTest<IAgentBadgeWithRankTransaction>
	{
		private IPerson person;

		protected override IAgentBadgeWithRankTransaction CreateAggregateWithCorrectBusinessUnit()
		{
			person = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person);
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
			UnitOfWork.PersistAll();
			CleanUpAfterTest();

			var target = new AgentBadgeWithRankTransactionRepository(UnitOfWork);
			target.ResetAgentBadges();

			var result = target.Find(person);
			result.Should().Be.Empty();
		}
	}
}