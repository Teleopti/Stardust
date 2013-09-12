using System.Reflection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class ScenarioHacks
	{
		public static void HackToSetBusinessUnit(this IScenario scenario, IBusinessUnit businessUnit)
		{
			//not nice, but don't want public setter. changing BU must not happen in application runtime.
			typeof(AggregateRootWithBusinessUnit)
				.GetField("_businessUnit", BindingFlags.Instance | BindingFlags.NonPublic)
				.SetValue(scenario, businessUnit);
		}
	}
}