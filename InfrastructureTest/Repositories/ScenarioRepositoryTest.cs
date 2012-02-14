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
            _rep = RepositoryFactory.CreateScenarioRepository(UnitOfWork);
        }


        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IScenario CreateAggregateWithCorrectBusinessUnit()
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate("Dummy", false, true);
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
        public void ShouldCreateWithFactory()
        {
            Assert.NotNull(new ScenarioRepository(UnitOfWorkFactory.Current));
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

        /// <summary>
        /// Test that only one defaul workspace is set
        /// </summary>
        [Test]
        public void CanOnlyHaveOneDefault()
        {
            IScenario myScenario1 = ScenarioFactory.CreateScenarioAggregate("Dummy", false, true);
            IScenario myScenario2 = ScenarioFactory.CreateScenarioAggregate("Dummy", false, true);
            _rep.Add(myScenario1);
            _rep.Add(myScenario2);
            Session.Flush();
            _rep.SetDefault(myScenario1);
            _rep.SetDefault(myScenario2);
            Session.Flush();
            UnitOfWork.Remove(myScenario1);
            UnitOfWork.Remove(myScenario2);

            myScenario1 = _rep.Load(myScenario1.Id.Value);
            myScenario2 = _rep.Load(myScenario2.Id.Value);
            Assert.AreNotEqual(myScenario1.DefaultScenario, myScenario2.DefaultScenario);
        }

        [Test]
        public void VerifyFindAllSorted()
        {
            Scenario scen1 = new Scenario("g");
            Scenario scen2 = new Scenario("a");
            Scenario scenDef = new Scenario("zzzDefault");
            _rep.SetDefault(scenDef);
            PersistAndRemoveFromUnitOfWork(scen1);
            PersistAndRemoveFromUnitOfWork(scen2);
            PersistAndRemoveFromUnitOfWork(scenDef);

            IList<IScenario> list = _rep.FindAllSorted();
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(scenDef, list[0]);
            Assert.AreEqual(scen2, list[1]);
            Assert.AreEqual(scen1, list[2]);
        }

        [Test]
        public void VerifyFindEnabledForReportingSorted()
        {
            Scenario scenario1 = new Scenario("c");
            Scenario scenario2 = new Scenario("a");
            Scenario scenario3 = new Scenario("b");
            scenario1.EnableReporting = true;
            scenario3.EnableReporting = true;

            _rep.SetDefault(scenario3);
            PersistAndRemoveFromUnitOfWork(scenario1);
            PersistAndRemoveFromUnitOfWork(scenario2);
            PersistAndRemoveFromUnitOfWork(scenario3);

            IList<IScenario> list = _rep.FindEnabledForReportingSorted();

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(scenario3, list[0]);
            Assert.AreEqual(scenario1, list[1]);
        }

        /// <summary>
        /// Verifies the can load default scenario.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-03-05
        /// </remarks>
        [Test]
        public void VerifyCanLoadDefaultScenario()
        {
            IScenario myScenario1 = ScenarioFactory.CreateScenarioAggregate("myScenario1", false, true);
            IScenario myScenario2 = ScenarioFactory.CreateScenarioAggregate("myScenario2", false, true);
            _rep.Add(myScenario1);
            _rep.Add(myScenario2);
            Session.Flush();
            _rep.SetDefault(myScenario1);
            Session.Flush();

            UnitOfWork.Remove(myScenario1);
            UnitOfWork.Remove(myScenario2);

            IScenario loadedScenario = _rep.LoadDefaultScenario();

            Assert.AreEqual(loadedScenario, myScenario1);
        }

        protected override Repository<IScenario> TestRepository(IUnitOfWork unitOfWork)
        {
            return new ScenarioRepository(unitOfWork);
        }
    }
}