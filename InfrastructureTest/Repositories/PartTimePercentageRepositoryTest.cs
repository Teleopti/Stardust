using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests PartTimePercentageRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class PartTimePercentageRepositoryTest : RepositoryTest<IPartTimePercentage>
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
        protected override IPartTimePercentage CreateAggregateWithCorrectBusinessUnit()
        {
            IPartTimePercentage partTimePercentage = new PartTimePercentage("dummyContract");

            return partTimePercentage;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IPartTimePercentage loadedAggregateFromDatabase)
        {
            IPartTimePercentage org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
        }

        protected override Repository<IPartTimePercentage> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new PartTimePercentageRepository(currentUnitOfWork);
        }

        [Test]
        public void VerifyLoadSortedByDescription()
        {
            IPartTimePercentage partTimePercentage = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(partTimePercentage);

            ICollection<IPartTimePercentage> partTimePercentages = new PartTimePercentageRepository(UnitOfWork).FindAllPartTimePercentageByDescription();
            Assert.AreEqual(1,partTimePercentages.Count);
        }
    }
}