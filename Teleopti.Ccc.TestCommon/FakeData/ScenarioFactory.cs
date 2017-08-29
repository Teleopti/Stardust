using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class ScenarioFactory
	{
		public static IScenario CreateScenarioAggregate()
		{
			IScenario ret = new Scenario("Default");

			return ret;
		}

		public static Scenario CreateScenarioAggregate(string name,
													   bool defaultWorkspace)
		{
			Scenario ret = new Scenario(name);
			ret.DefaultScenario = defaultWorkspace;
			return ret;
		}

		public static Scenario CreateScenario(string name, bool defaultWorkspace, bool enableReporting)
		{
			return new Scenario(name) {DefaultScenario = defaultWorkspace, EnableReporting = enableReporting};
		}

		public static Scenario CreateScenarioWithId(string name, bool defaultWorkspace)
		{
			Scenario ret = new Scenario(name);
			ret.SetId(Guid.NewGuid());
			ret.DefaultScenario = defaultWorkspace;
			return ret;
		}
	}
}