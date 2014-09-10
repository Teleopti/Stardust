using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.IocCommon.Conventions;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon
{
	public abstract class TeleoptiModule : Module
	{
		private readonly IConfigReader _configReader;

		protected TeleoptiModule(IConfigReader configReader)
		{
			_configReader = configReader;
			var setters = propertySetters();
			GetType().GetProperties().ForEach(prop => setters.ForEach(setter => setter.SetPropertyValue(this, prop)));
		}

		private IEnumerable<ISetModuleProperties> propertySetters()
		{
			return new []
			{
				new SetPropertiesFromAppSettings(_configReader)
			};
		}
	}
}