using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests ContractRepository
    ///</summary>
    [TestFixture]
    [Category("BucketB")]
    public class ContractRepositoryTest : RepositoryTest<IContract>
    {
        private IContract _contract;
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
        protected override IContract CreateAggregateWithCorrectBusinessUnit()
        {
            IContract contract = new Contract("dummyContract");

            return contract;
        }

        [Test]
        public void VerifyCanPersistProperties()
        {
            _contract = new Contract("MyContract");
            IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime",
                                                                                       MultiplicatorType.Overtime);
            PersistAndRemoveFromUnitOfWork(definitionSet);
            _contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
            _contract.PositivePeriodWorkTimeTolerance = TimeSpan.FromMinutes(1);
            _contract.NegativePeriodWorkTimeTolerance = TimeSpan.FromMinutes(2);
            _contract.PlanningTimeBankMax = TimeSpan.FromHours(2);
            _contract.PlanningTimeBankMin = TimeSpan.FromHours(-2);
            _contract.NegativeDayOffTolerance = -5;
            _contract.PositiveDayOffTolerance = 5;
            _contract.WorkTimeSource = WorkTimeSource.FromContract;

            PersistAndRemoveFromUnitOfWork(_contract);
            var loadedContracts = new ContractRepository(UnitOfWork).LoadAll().ToList();
            Assert.AreEqual(1, loadedContracts.Count);
            Assert.AreEqual(1, loadedContracts[0].MultiplicatorDefinitionSetCollection.Count);
            Assert.AreEqual(definitionSet.MultiplicatorType, loadedContracts[0].MultiplicatorDefinitionSetCollection[0].MultiplicatorType);
            Assert.AreEqual(TimeSpan.FromMinutes(1), loadedContracts[0].PositivePeriodWorkTimeTolerance);
            Assert.AreEqual(TimeSpan.FromMinutes(2), loadedContracts[0].NegativePeriodWorkTimeTolerance);
            Assert.That(loadedContracts[0].PlanningTimeBankMax,Is.EqualTo(TimeSpan.FromHours(2)));
            Assert.That(loadedContracts[0].PlanningTimeBankMin,Is.EqualTo(TimeSpan.FromHours(-2)));
            Assert.That(loadedContracts[0].NegativeDayOffTolerance,Is.EqualTo(-5));
            Assert.That(loadedContracts[0].PositiveDayOffTolerance,Is.EqualTo(5));
            Assert.That(loadedContracts[0].WorkTimeSource, Is.EqualTo(WorkTimeSource.FromContract));

        }

        [Test]
        public void VerifyCanLoadContractsAndInitializeMultiplicatorDefinitionSets()
        {
            _contract = new Contract("MyContract");
            IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime",
                                                                                       MultiplicatorType.Overtime);
            PersistAndRemoveFromUnitOfWork(definitionSet);
            _contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
            PersistAndRemoveFromUnitOfWork(_contract);
            ICollection<IContract> loadedContracts = new ContractRepository(UnitOfWork).FindAllContractByDescription();
            Assert.AreEqual(1, loadedContracts.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedContracts.First().MultiplicatorDefinitionSetCollection));
        }

		[Test]
	  public void ShouldFindContractsContain()
		{
			var name = RandomName.Make();
			var contract = new Contract(RandomName.Make() + name + RandomName.Make());
			PersistAndRemoveFromUnitOfWork(contract);

			var loadedContracts = new ContractRepository(UnitOfWork).FindContractsContain(name, 10);

			loadedContracts.Should().Have.SameValuesAs(contract);
		}

		[Test]
		public void ShouldMissContractsContain()
		{
			var contract = new Contract(RandomName.Make());
			PersistAndRemoveFromUnitOfWork(contract);

			var loadedContracts = new ContractRepository(UnitOfWork).FindContractsContain(RandomName.Make(), 10);

			loadedContracts.Should().Be.Empty();
		}

		[Test]
		public void ShouldOnlyFetchMaxNumberOfItems()
		{
				const int maxHits = 2;
				var name = RandomName.Make();
			PersistAndRemoveFromUnitOfWork(new Contract(name));
			PersistAndRemoveFromUnitOfWork(new Contract(name));
			PersistAndRemoveFromUnitOfWork(new Contract(name));

			var loadedContracts = new ContractRepository(UnitOfWork).FindContractsContain(name, maxHits);

			loadedContracts.Count().Should().Be.EqualTo(maxHits);
		}

		[Test]
		public void ShouldNotMakeSearchCallIfMaxItemsIsZero()
		{
			using (Session.SessionFactory.WithStats())
			{
				var statementsBefore = Session.SessionFactory.Statistics.PrepareStatementCount;

				new ContractRepository(UnitOfWork).FindContractsContain(RandomName.Make(), 0);

				var statementsAfter = Session.SessionFactory.Statistics.PrepareStatementCount;
				(statementsAfter - statementsBefore).Should().Be.EqualTo(0);
			}
		}

		/// <summary>
		/// Verifies the aggregate graph properties.
		/// </summary>
		/// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
		protected override void VerifyAggregateGraphProperties(IContract loadedAggregateFromDatabase)
        {
            IContract org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
        }

        protected override Repository<IContract> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new ContractRepository(currentUnitOfWork, null, null);
        }

    }
}