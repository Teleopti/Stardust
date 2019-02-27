using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class SeniorityWorkDayRanksRepositoryTest : RepositoryTest<ISeniorityWorkDayRanks>
	{
		protected override ISeniorityWorkDayRanks CreateAggregateWithCorrectBusinessUnit()
		{
			ISeniorityWorkDayRanks seniorityWorkDayRanks = new SeniorityWorkDayRanks
			{
				Monday = 7,
				Tuesday = 6,
				Wednesday = 5,
				Thursday = 4,
				Friday = 3,
				Saturday = 2,
				Sunday = 1
			};
			return seniorityWorkDayRanks;
		}

		[Test]
		public void VerifyCanPersistProperties()
		{
			var seniorityWorkDayRanks = CreateAggregateWithCorrectBusinessUnit();

			PersistAndRemoveFromUnitOfWork(seniorityWorkDayRanks);
			var loadedRanks = SeniorityWorkDayRanksRepository.DONT_USE_CTOR(UnitOfWork).LoadAll().ToList();

			Assert.AreEqual(1, loadedRanks.Count);
			Assert.AreEqual(7, loadedRanks[0].Monday);
			Assert.AreEqual(6, loadedRanks[0].Tuesday);
			Assert.AreEqual(5, loadedRanks[0].Wednesday);
			Assert.AreEqual(4, loadedRanks[0].Thursday);
			Assert.AreEqual(3, loadedRanks[0].Friday);
			Assert.AreEqual(2, loadedRanks[0].Saturday);
			Assert.AreEqual(1, loadedRanks[0].Sunday);
		}

		protected override void VerifyAggregateGraphProperties(ISeniorityWorkDayRanks loadedAggregateFromDatabase)
		{
			var org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.Monday, loadedAggregateFromDatabase.Monday);
			Assert.AreEqual(org.Tuesday, loadedAggregateFromDatabase.Tuesday);
			Assert.AreEqual(org.Wednesday, loadedAggregateFromDatabase.Wednesday);
			Assert.AreEqual(org.Thursday, loadedAggregateFromDatabase.Thursday);
			Assert.AreEqual(org.Friday, loadedAggregateFromDatabase.Friday);
			Assert.AreEqual(org.Saturday, loadedAggregateFromDatabase.Saturday);
			Assert.AreEqual(org.Sunday, loadedAggregateFromDatabase.Sunday);
		}

		protected override Repository<ISeniorityWorkDayRanks> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return SeniorityWorkDayRanksRepository.DONT_USE_CTOR(currentUnitOfWork.Current());
		}
	}
}
