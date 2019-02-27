using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests ContractScheduleRepository
    ///</summary>
    [TestFixture]
    [Category("BucketB")]
    public class ContractScheduleRepositoryTest : RepositoryTest<IContractSchedule>
    {
        /// <summary>
        /// Renove this suppress later when more tests are written
        /// </summary>
        private IContractScheduleRepository rep; 

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            rep = ContractScheduleRepository.DONT_USE_CTOR(UnitOfWork);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IContractSchedule CreateAggregateWithCorrectBusinessUnit()
        {
            IContractSchedule contractSchedule = ContractScheduleFactory.CreateContractSchedule("dummyContractSchedule");

            return contractSchedule;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IContractSchedule loadedAggregateFromDatabase)
        {
            IContractSchedule org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(org.ContractScheduleWeeks.Count(), loadedAggregateFromDatabase.ContractScheduleWeeks.Count());
        }

        protected override Repository<IContractSchedule> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return ContractScheduleRepository.DONT_USE_CTOR(currentUnitOfWork);
        }

        [Test]
        public void VerifyFindAllContractScheduleByDescription()
        {
            IContractSchedule contractSchedule1 = ContractScheduleFactory.CreateContractSchedule("AAA");
            IContractSchedule contractSchedule2 = ContractScheduleFactory.CreateContractSchedule("BBB");
            IContractSchedule contractSchedule3 = ContractScheduleFactory.CreateContractSchedule("CCC");

            PersistAndRemoveFromUnitOfWork(contractSchedule3);
            PersistAndRemoveFromUnitOfWork(contractSchedule2);
            PersistAndRemoveFromUnitOfWork(contractSchedule1);

            IList<IContractSchedule> testList = new List<IContractSchedule>(rep.FindAllContractScheduleByDescription());

            Assert.AreEqual(3, testList.Count);
            Assert.AreEqual(contractSchedule1, testList[0]);
            Assert.AreEqual(contractSchedule2, testList[1]);
            Assert.AreEqual(contractSchedule3, testList[2]);


        }


        [Test]
        public void VerifyLoadAllAggregate()
        {
            IContractSchedule cs = new ContractSchedule("for test");
            cs.AddContractScheduleWeek(createContractScheduleWeek());
            cs.AddContractScheduleWeek(createContractScheduleWeek());
            IContractSchedule cs2 = new ContractSchedule("for test 2");
            cs2.AddContractScheduleWeek(createContractScheduleWeek());
            cs2.AddContractScheduleWeek(createContractScheduleWeek());

            PersistAndRemoveFromUnitOfWork(cs2);
            PersistAndRemoveFromUnitOfWork(cs);
            Session.Clear();

            IList<IContractSchedule> loaded = new List<IContractSchedule>(rep.LoadAllAggregate());
            Assert.AreEqual(2, loaded.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded[0].ContractScheduleWeeks));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded[0].ContractScheduleWeeks.First()[DayOfWeek.Wednesday]));
            Assert.AreEqual(2, loaded[0].ContractScheduleWeeks.Count());
        }

        private static IContractScheduleWeek createContractScheduleWeek()
        {
            IContractScheduleWeek cWeek = new ContractScheduleWeek();
            cWeek.Add(DayOfWeek.Monday, true);
            cWeek.Add(DayOfWeek.Tuesday, true);
            cWeek.Add(DayOfWeek.Wednesday, true);
            cWeek.Add(DayOfWeek.Thursday, true);
            cWeek.Add(DayOfWeek.Friday, true);
            cWeek.Add(DayOfWeek.Saturday, true);
            cWeek.Add(DayOfWeek.Sunday, true);
            return cWeek;
        }
    }
}
