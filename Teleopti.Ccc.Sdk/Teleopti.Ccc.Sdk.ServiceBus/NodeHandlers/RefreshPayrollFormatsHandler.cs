using Stardust.Node.Interfaces;
using Stardust.Node.ReturnObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class RefreshPayrollFormatsHandler : IHandle<RefreshPayrollFormatsEvent>
	{
		private readonly IConfigReader _configReader;
		private readonly ISearchPath _searchPath;
		private readonly IPlugInLoader _pluginInLoader;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		public RefreshPayrollFormatsHandler(ISearchPath searchPath, 
			IConfigReader configReader, 
			IPlugInLoader plugInLoader, 
			IDataSourceForTenant dataSourceForTenant)
		{
			_searchPath = searchPath ?? new SearchPath();
			_configReader = configReader ?? new ConfigReader();
			_pluginInLoader = plugInLoader;
			_dataSourceForTenant = dataSourceForTenant;
		}
		public void Handle(RefreshPayrollFormatsEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress, ref IEnumerable<object> returnObjects)
		{
			if (!string.IsNullOrEmpty(_configReader.AppConfig("IsContainer")) && 
				!string.IsNullOrEmpty(_configReader.ConnectionString("AzureStorage")))
			{
				new PayrollDllCopy(_searchPath, _configReader).CopyPayrollDllFromAzureStorage(parameters.TenantName);
				new InitializePayrollFormatsToDb(_pluginInLoader, _dataSourceForTenant).InitializeOneTenant(null, parameters.TenantName);
				returnObjects = new List<object>() {
						new ExitApplication() {ExitCode = 0}
					};
			}
		}
	}
}
