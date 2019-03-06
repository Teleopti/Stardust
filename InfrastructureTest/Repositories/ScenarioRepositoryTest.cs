using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
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
			var repository = ScenarioRepository.DONT_USE_CTOR(UnitOfWork);
			foreach (var scenario in repository.LoadAll())
			{
				((IDeleteTag)scenario).SetDeleted();
				PersistAndRemoveFromUnitOfWork(scenario);
			}

			var ex = Assert.Throws<NoDefaultScenarioException>(() => repository.LoadDefaultScenario());
			ex.Message.Should().Contain("has no default scenario");
		}

		[Test]
		public void ShouldReturnSortedListWithDefaultScenarioFirst()
		{
			var repository = ScenarioRepository.DONT_USE_CTOR(UnitOfWork);

			var scenarioA = CreateAggregateWithCorrectBusinessUnit();
			scenarioA.ChangeName("A");
			var scenarioX = CreateAggregateWithCorrectBusinessUnit();
			scenarioX.ChangeName("X");
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
			var repository = ScenarioRepository.DONT_USE_CTOR(UnitOfWork);

			var scenarioA = CreateAggregateWithCorrectBusinessUnit();
			scenarioA.ChangeName("A");
			scenarioA.EnableReporting = false;
			var scenarioX = CreateAggregateWithCorrectBusinessUnit();
			scenarioX.ChangeName("X");
			scenarioX.DefaultScenario = true;
			scenarioX.EnableReporting = true;

			PersistAndRemoveFromUnitOfWork(scenarioX);
			PersistAndRemoveFromUnitOfWork(scenarioA);

			var result = repository.FindEnabledForReportingSorted();
			result.Single().Should().Be.EqualTo(scenarioX);
		}

		protected override Repository<IScenario> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return ScenarioRepository.DONT_USE_CTOR(currentUnitOfWork);
        }
    }
}