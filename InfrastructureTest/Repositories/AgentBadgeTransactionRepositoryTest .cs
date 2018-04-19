using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class AgentBadgeTransactionRepositoryTest : RepositoryTest<IAgentBadgeTransaction>
	{
		private IPerson person;

		protected override void ConcreteSetup()
		{
			person = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person);
		}

		protected override IAgentBadgeTransaction CreateAggregateWithCorrectBusinessUnit()
		{
			return new AgentBadgeTransaction
			{
				Amount = 1,
				BadgeType = BadgeType.AverageHandlingTime,
				CalculatedDate = new DateOnly(2014, 9, 9),
				Description = "test",
				InsertedOn = DateTime.SpecifyKind(new DateTime(2014, 9, 10), DateTimeKind.Utc),
				Person = person
			};
		}

		

		protected override void VerifyAggregateGraphProperties(IAgentBadgeTransaction loadedAggregateFromDatabase)
		{
			var newBadgeTran = CreateAggregateWithCorrectBusinessUnit();

			Assert.AreEqual(newBadgeTran.Person.Name, loadedAggregateFromDatabase.Person.Name);
			Assert.AreEqual(newBadgeTran.BadgeType, loadedAggregateFromDatabase.BadgeType);
			Assert.AreEqual(newBadgeTran.Amount, loadedAggregateFromDatabase.Amount);
			Assert.AreEqual(newBadgeTran.CalculatedDate, loadedAggregateFromDatabase.CalculatedDate);
			Assert.AreEqual(newBadgeTran.Description, loadedAggregateFromDatabase.Description);
			Assert.AreEqual(newBadgeTran.InsertedOn, loadedAggregateFromDatabase.InsertedOn);
			Assert.AreEqual(newBadgeTran.IsExternal, loadedAggregateFromDatabase.IsExternal);
		}

		protected override Repository<IAgentBadgeTransaction> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new AgentBadgeTransactionRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldResetBadges()
		{
			var badge = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(badge);

			var target = new AgentBadgeTransactionRepository(UnitOfWork);
			target.ResetAgentBadges();

			var result = target.Find(badge.Person, badge.BadgeType, badge.CalculatedDate, false);
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldRemoveBadgesWithinPeriod()
		{
			var date = new DateOnly(2018, 2, 28);
			var badge1 = CreateAggregateWithCorrectBusinessUnit();
			var badge2 = CreateAggregateWithCorrectBusinessUnit();
			badge2.CalculatedDate = date.AddDays(-1);
			var badge3 = CreateAggregateWithCorrectBusinessUnit();
			badge3.CalculatedDate = date;
			var badge4 = CreateAggregateWithCorrectBusinessUnit();
			badge4.CalculatedDate = date.AddDays(1);
			PersistAndRemoveFromUnitOfWork(badge1);
			PersistAndRemoveFromUnitOfWork(badge2);
			PersistAndRemoveFromUnitOfWork(badge3);
			PersistAndRemoveFromUnitOfWork(badge4);

			var target = new AgentBadgeTransactionRepository(UnitOfWork);
			target.Remove(new DateOnlyPeriod(date.AddDays(-1), date.AddDays(1)));
			Session.Flush();

			var result = target.LoadAll();
			result.Count().Should().Be.EqualTo(1);
			result.ToList()[0].CalculatedDate.Should().Be.EqualTo(badge1.CalculatedDate);
		}

		[Test]
		public void ShouldAbleToPersistExternalBadgeTransaction()
		{
			IAgentBadgeTransaction agentBadgeTransaction = new AgentBadgeTransaction
			{
				Amount = 1,
				BadgeType = 8,
				CalculatedDate = new DateOnly(2018, 2, 1),
				Description = "test",
				InsertedOn = DateTime.SpecifyKind(new DateTime(2018, 2, 1), DateTimeKind.Utc),
				Person = person,
				IsExternal = true
			};

			Session.SaveOrUpdate(agentBadgeTransaction);
			Session.Flush();
			Session.Evict(agentBadgeTransaction);

			var target = new AgentBadgeTransactionRepository(UnitOfWork);
			var result = target.Find(new List<IPerson> { agentBadgeTransaction.Person }, new DateOnlyPeriod(2018, 1, 31, 2018, 2, 2)).FirstOrDefault();
			result.Should().Not.Be.Null();
			result.IsExternal.Should().Be.EqualTo(true);
			result.BadgeType.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldGetBadgeWithinGivenDatePeriod()
		{
			var badge1 = CreateAggregateWithCorrectBusinessUnit();
			badge1.CalculatedDate = new DateOnly(2014, 9, 9);
			PersistAndRemoveFromUnitOfWork(badge1);
			var badge2 = CreateAggregateWithCorrectBusinessUnit();
			badge2.CalculatedDate = new DateOnly(2014, 9, 15);
			PersistAndRemoveFromUnitOfWork(badge2);

			var target = new AgentBadgeTransactionRepository(UnitOfWork);
			var badges = target.Find(new List<IPerson>
				{
					badge1.Person,
					badge2.Person
				},
				new DateOnlyPeriod(2014, 9, 8, 2014, 9, 10));

			badges.Count.Should().Be.EqualTo(1);

			badges = target.Find(new List<IPerson>
			{
				badge1.Person,
				badge2.Person
			}, new DateOnlyPeriod(2014, 9, 8, 2014, 9, 16));

			badges.Count.Should().Be.EqualTo(2);
		}
	}
}
