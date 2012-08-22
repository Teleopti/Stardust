using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests ContractRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
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
            IContract contract = ContractFactory.CreateContract("dummyContract");

            return contract;
        }

        [Test]
        public void VerifyCanPersistProperties()
        {
            _contract = ContractFactory.CreateContract("MyContract");
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
            _contract.IsWorkTimeFromContract = true;
            _contract.IsWorkTimeFromSchedulePeriod = false;

            PersistAndRemoveFromUnitOfWork(_contract);
            IList<IContract> loadedContracts = new ContractRepository(UnitOfWork).LoadAll();
            Assert.AreEqual(1, loadedContracts.Count);
            Assert.AreEqual(1, loadedContracts[0].MultiplicatorDefinitionSetCollection.Count);
            Assert.AreEqual(definitionSet.MultiplicatorType, loadedContracts[0].MultiplicatorDefinitionSetCollection[0].MultiplicatorType);
            Assert.AreEqual(TimeSpan.FromMinutes(1), loadedContracts[0].PositivePeriodWorkTimeTolerance);
            Assert.AreEqual(TimeSpan.FromMinutes(2), loadedContracts[0].NegativePeriodWorkTimeTolerance);
            Assert.That(loadedContracts[0].PlanningTimeBankMax,Is.EqualTo(TimeSpan.FromHours(2)));
            Assert.That(loadedContracts[0].PlanningTimeBankMin,Is.EqualTo(TimeSpan.FromHours(-2)));
            Assert.That(loadedContracts[0].NegativeDayOffTolerance,Is.EqualTo(-5));
            Assert.That(loadedContracts[0].PositiveDayOffTolerance,Is.EqualTo(5));
            Assert.IsTrue(loadedContracts[0].IsWorkTimeFromContract );
            Assert.IsFalse( loadedContracts[0].IsWorkTimeFromSchedulePeriod  );

        }

        [Test]
        public void VerifyCanLoadContractsAndInitializeMultiplicatorDefinitionSets()
        {
            _contract = ContractFactory.CreateContract("MyContract");
            IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime",
                                                                                       MultiplicatorType.Overtime);
            PersistAndRemoveFromUnitOfWork(definitionSet);
            _contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
            PersistAndRemoveFromUnitOfWork(_contract);
            ICollection<IContract> loadedContracts = new ContractRepository(UnitOfWork).FindAllContractByDescription();
            Assert.AreEqual(1, loadedContracts.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedContracts.First().MultiplicatorDefinitionSetCollection));
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

        protected override Repository<IContract> TestRepository(IUnitOfWork unitOfWork)
        {
            return new ContractRepository(unitOfWork);
        }

		/// <summary>
		/// Determines whether this instance can be created.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void CanCreate()
		{
			new ContractRepository(UnitOfWorkFactory.Current).Should().Not.Be.Null();
		}

        [Test, Ignore("do not know if this should be filtered in db?")]
        public void VerifyDeletedMultiplicatorIsNotPartOfContract()
        {
            IMultiplicatorDefinitionSet originalMultiplicatorDefinitionSet = new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime);
            PersistAndRemoveFromUnitOfWork(originalMultiplicatorDefinitionSet);

            IContract contract = ContractFactory.CreateContract("MyContract");
            PersistAndRemoveFromUnitOfWork(contract);

            contract.AddMultiplicatorDefinitionSetCollection(originalMultiplicatorDefinitionSet);
            PersistAndRemoveFromUnitOfWork(contract);

            new MultiplicatorDefinitionSetRepository(UnitOfWork).Remove(originalMultiplicatorDefinitionSet);
            PersistAndRemoveFromUnitOfWork(originalMultiplicatorDefinitionSet);

            contract = new ContractRepository(UnitOfWork).Get(contract.Id.Value);
            Assert.AreEqual(0, contract.MultiplicatorDefinitionSetCollection.Count);
        }
    }
}