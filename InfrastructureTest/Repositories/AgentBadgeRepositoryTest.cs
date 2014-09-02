using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	public class AgentBadgeRepositoryTest : RepositoryTest<IAgentBadge>
	{
		private IPerson person;

		protected override IAgentBadge CreateAggregateWithCorrectBusinessUnit()
		{
			person = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person);

			var badge = new AgentBadge
			{
				Person = person,
				BadgeType = BadgeType.AnsweredCalls,
				BronzeBadge = 1,
				SilverBadge = 0,
				GoldBadge = 0,
				LastCalculatedDate = new DateOnly(2014, 01, 01)
			};

			return badge;
		}

		protected override void VerifyAggregateGraphProperties(IAgentBadge loadedAggregateFromDatabase)
		{
			var newBadge = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(newBadge.Person.Name, loadedAggregateFromDatabase.Person.Name);
			Assert.AreEqual(newBadge.BadgeType, loadedAggregateFromDatabase.BadgeType);
			Assert.AreEqual(newBadge.BronzeBadge, loadedAggregateFromDatabase.BronzeBadge);
			Assert.AreEqual(newBadge.SilverBadge, loadedAggregateFromDatabase.SilverBadge);
			Assert.AreEqual(newBadge.LastCalculatedDate, loadedAggregateFromDatabase.LastCalculatedDate);
		}

		protected override Repository<IAgentBadge> TestRepository(IUnitOfWork unitOfWork)
		{
			return new AgentBadgeRepository(unitOfWork);
		}

		[Test]
		public void VerifyCanFindByPerson()
		{
			var badge = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(badge);

			var badges = new AgentBadgeRepository(UnitOfWork).Find(person);
			Assert.AreEqual(badges.Count, 1);
		}
		
		[Test]
		public void VerifyCanFindByPersonAndBadgeType()
		{
			var newBadge = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(newBadge);

			var badge = new AgentBadgeRepository(UnitOfWork).Find(person, BadgeType.AnsweredCalls);
			Assert.IsNotNull(badge);
		}
	}
}