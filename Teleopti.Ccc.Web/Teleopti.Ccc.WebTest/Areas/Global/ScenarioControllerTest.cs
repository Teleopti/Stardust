using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class ScenarioControllerTest
	{
		[Test]
		public void ShouldGetDefaultScenarioFirst()
		{
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has(new Scenario("Test").WithId());
			scenarioRepository.Has(new Scenario("Default") {DefaultScenario = true}.WithId());
			var target = new ScenarioController(scenarioRepository);
			var result = target.Scenarios();
			result.Length.Should().Be.EqualTo(2);
			result[0].DefaultScenario.Should().Be.True();
		}

		[Test]
		public void ShouldGetScenarios()
		{
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has(new Scenario("Test").WithId());
			var target = new ScenarioController(scenarioRepository);
			var result = target.Scenarios();
			result.Length.Should().Be.EqualTo(1);
			result[0].Name.Should().Be.EqualTo("Test");
		}
	}
}