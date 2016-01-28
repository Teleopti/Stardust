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

		[Test, Ignore("only failed on build server")]
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

			var result = repository.LoadAll();
			result[0].Should().Be.EqualTo(scenarioX);
			result[1].Should().Be.EqualTo(scenarioA);
		}

		protected override Repository<IScenario> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new ScenarioRepository(currentUnitOfWork);
        }
    }
}