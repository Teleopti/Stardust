using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
	public class ScenarioCreator
	{
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public IScenario Create(string name, bool defaultScenario, bool enableReporting, bool restricted)
		{
			IScenario scenario = new Scenario(name)
			{
				DefaultScenario = defaultScenario,
				EnableReporting = enableReporting,
				Restricted = restricted
			};
			return scenario;
		}

		public static IScenario CreateDefault(ISessionFactory sessionFactory, IBusinessUnit businessUnit, IPerson person)
		{
			IScenario scenario = new Scenario("Default")
			{
				DefaultScenario = true,
				EnableReporting = true,
				Restricted = false
			};

			var setChangeInfoCommand = new SetChangeInfoCommand();
			setChangeInfoCommand.Execute((AggregateRoot)scenario, person);

			typeof(VersionedAggregateRootWithBusinessUnit)
				 .GetField("_businessUnit", BindingFlags.NonPublic | BindingFlags.Instance)
				 .SetValue(scenario, businessUnit);

			using (ISession session = sessionFactory.OpenSession())
			{
				session.Save(scenario);
				session.Flush();
				session.Close();
			}

			return scenario;
		}
	}
}
