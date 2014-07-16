using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	///<summary>
	/// Tests AbsenceRepository
	///</summary>
	[TestFixture]
	[Category("LongRunning")]
	public class AgentBadgeRepositoryTest : RepositoryTest<AgentBadge>
	{
		/// <summary>
		/// Runs every test. Implemented by repository's concrete implementation.
		/// </summary>
		protected override void ConcreteSetup()
		{
		}

		/// <summary>
		/// Creates an aggregate using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		protected override AgentBadge CreateAggregateWithCorrectBusinessUnit()
		{
			var agentBadge = new AgentBadge()
			{
				PersonId = Guid.NewGuid(),
				BronzeBadge = 12,
				SilverBadge = 3,
				GoldenBadge = 1
			};

			return agentBadge;
		}

		/// <summary>
		/// Verifies the aggregate graph properties.
		/// </summary>
		/// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
		protected override void VerifyAggregateGraphProperties(AgentBadge loadedAggregateFromDatabase)
		{
			var agentBadge = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(agentBadge.BronzeBadge, loadedAggregateFromDatabase.BronzeBadge);
			Assert.AreEqual(agentBadge.SilverBadge, loadedAggregateFromDatabase.SilverBadge);
			Assert.AreEqual(agentBadge.GoldenBadge, loadedAggregateFromDatabase.GoldenBadge);
		}

		protected override Repository<AgentBadge> TestRepository(IUnitOfWork unitOfWork)
		{
			return new AgentBadgeRepository(unitOfWork);
		}

		/// <summary>
		/// Verifies that the loading AgentBadge is correct
		/// </summary>
		/// <remarks>
		/// Created by: Xinfeng
		/// Created date: 2014-07-15
		/// </remarks>
		[Test]
		public void VerifyLoadByPersonId()
		{
			var personId1 = Guid.NewGuid();
			var agentBadge1 = new AgentBadge()
			{
				PersonId = personId1,
				BronzeBadge = 1,
				SilverBadge = 2,
				GoldenBadge = 3,
			};

			PersistAndRemoveFromUnitOfWork(agentBadge1);

			var rep = new AgentBadgeRepository(UnitOfWork);
			var result = rep.Load(personId1);

			Assert.AreEqual(personId1, result.PersonId);
			Assert.AreEqual(1, result.BronzeBadge);
			Assert.AreEqual(2, result.SilverBadge);
			Assert.AreEqual(3, result.GoldenBadge);

		}
		/// <summary>
		/// Verifies that the loading AgentBadge is correct
		/// </summary>
		/// <remarks>
		/// Created by: Xinfeng
		/// Created date: 2014-07-15
		/// </remarks>
		[Test]
		public void VerifyLoadAll()
		{
			var personId1 = Guid.NewGuid();
			var agentBadge1 = new AgentBadge()
			{
				PersonId = personId1,
				BronzeBadge = 1,
				SilverBadge = 2,
				GoldenBadge = 3,
			};

			var personId2 = Guid.NewGuid();
			var agentBadge2 = new AgentBadge()
			{
				PersonId = personId1,
				BronzeBadge = 21,
				SilverBadge = 22,
				GoldenBadge = 23,
			};

			var personId3 = Guid.NewGuid();
			var agentBadge3 = new AgentBadge()
			{
				PersonId = personId1,
				BronzeBadge = 31,
				SilverBadge = 32,
				GoldenBadge = 33,
			};

			PersistAndRemoveFromUnitOfWork(agentBadge1);
			PersistAndRemoveFromUnitOfWork(agentBadge2);
			PersistAndRemoveFromUnitOfWork(agentBadge3);

			var rep = new AgentBadgeRepository(UnitOfWork);
			IList<AgentBadge> result = rep.LoadAll().ToList();


			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result.Any(badge => badge.PersonId == personId1));
			Assert.IsTrue(result.Any(badge => badge.PersonId == personId2));
			Assert.IsTrue(result.Any(badge => badge.PersonId == personId3));
		}
	}
}