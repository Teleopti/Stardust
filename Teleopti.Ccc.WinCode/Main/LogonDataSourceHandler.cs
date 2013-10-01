using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.WinCode.Main
{
	public class LogonDataSourceHandler : IDataSourceHandler
	{
		private readonly IComponentContext _componentContext;

		public LogonDataSourceHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public IAvailableDataSourcesProvider AvailableDataSourcesProvider()
		{
			return _componentContext.Resolve<IAvailableDataSourcesProvider>();
		}

		public IEnumerable<IDataSourceProvider> DataSourceProviders()
		{
			return _componentContext.Resolve<IEnumerable<IDataSourceProvider>>();
		}

	}
}
