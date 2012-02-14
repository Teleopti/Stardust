using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class ScenarioProviderTest
    {
        private MockRepository mocks;
        private IScenarioRepository scenarioRepository;
        private IScenarioProvider target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            scenarioRepository = mocks.StrictMock<IScenarioRepository>();
            target = new ScenarioProvider(scenarioRepository);
        }

        [Test]
        public void ShouldReturnDefaultScenario()
        {
            IScenario scenario = mocks.StrictMock<IScenario>();
            using(mocks.Record())
            {
                Expect.Call(scenarioRepository.LoadDefaultScenario()).Return(scenario);
            }
            using (mocks.Playback())
            {
                var defaultScenario = target.DefaultScenario();
                Assert.IsNotNull(defaultScenario);
            }
        }
    }
}
