using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests ScenarioRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class ScenarioRepositoryTest : RepositoryTest<IScenario>
    {
        private IScenarioRepository _rep;

        /// <summary>
        /// Runs before every test.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _rep = new ScenarioRepository(UnitOfWork);
        }


        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IScenario CreateAggregateWithCorrectBusinessUnit()
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate("Dummy", false);
            return scenario;
        }


        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        protected override void VerifyAggregateGraphProperties(IScenario loadedAggregateFromDatabase)
        {
            Assert.AreEqual(loadedAggregateFromDatabase.Description.Name, "Dummy");
        }
        
		[Test]
		public void ShouldThrowIfDefaultScenarioDoesNotExist()
		{
			foreach (var scenario in _rep.LoadAll())
			{
				((IDeleteTag)scenario).SetDeleted();
				PersistAndRemoveFromUnitOfWork(scenario);
			}

			Assert.Throws<DataSourceException>(() => _rep.LoadDefaultScenario());
		}


        protected override Repository<IScenario> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new ScenarioRepository(currentUnitOfWork);
        }
    }
}