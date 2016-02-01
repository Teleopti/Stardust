using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class ScenarioRepositoryTest : RepositoryTest<IScenario>
    {
        protected override IScenario CreateAggregateWithCorrectBusinessUnit()
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate("Dummy", false);
            return scenario;
        }

		protected override void VerifyAggregateGraphProperties(IScenario loadedAggregateFromDatabase)
        {
            Assert.AreEqual(loadedAggregateFromDatabase.Description.Name, "Dummy");
        }
        
		[Test]
		public void ShouldThrowIfDefaultScenarioDoesNotExist()
		{
			var repository = new ScenarioRepository(UnitOfWork);
			foreach (var scenario in repository.LoadAll())
			{
				((IDeleteTag)scenario).SetDeleted();
				PersistAndRemoveFromUnitOfWork(scenario);
			}

			Assert.Throws<DataSourceException>(() => repository.LoadDefaultScenario());
		}

		[Test]
		public void ShouldReturnSortedListWithDefaultScenarioFirst()
		{
			var repository = new ScenarioRepository(UnitOfWork);

			var scenarioA = CreateAggregateWithCorrectBusinessUnit();
			scenarioA.Description = new Description("A");
			var scenarioX = CreateAggregateWithCorrectBusinessUnit();
			scenarioX.Description = new Description("X");
			scenarioX.DefaultScenario = true;

			PersistAndRemoveFromUnitOfWork(scenarioX);
			PersistAndRemoveFromUnitOfWork(scenarioA);

			var result = repository.FindAllSorted();
			result[0].Should().Be.EqualTo(scenarioX);
			result[1].Should().Be.EqualTo(scenarioA);
		}

		[Test]
		public void ShouldIgnoreScenariosNotEnabledForReporting()
		{
			var repository = new ScenarioRepository(UnitOfWork);

			var scenarioA = CreateAggregateWithCorrectBusinessUnit();
			scenarioA.Description = new Description("A");
			scenarioA.EnableReporting = false;
			var scenarioX = CreateAggregateWithCorrectBusinessUnit();
			scenarioX.Description = new Description("X");
			scenarioX.DefaultScenario = true;
			scenarioX.EnableReporting = true;

			PersistAndRemoveFromUnitOfWork(scenarioX);
			PersistAndRemoveFromUnitOfWork(scenarioA);

			var result = repository.FindEnabledForReportingSorted();
			result.Single().Should().Be.EqualTo(scenarioX);
		}

		protected override Repository<IScenario> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new ScenarioRepository(currentUnitOfWork);
        }
    }
}