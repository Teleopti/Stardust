using Stardust.Node.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class RefreshPayrollFormatsHandler : IHandle<RefreshPayrollFormatsEvent>
	{
		private readonly IConfigReader _configReader;
		private readonly ISearchPath _searchPath;
		private readonly IInitializePayrollFormats _initializePayrollFormats;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly IServerConfigurationRepository _serverConfigurationRepository;

		public RefreshPayrollFormatsHandler(ISearchPath searchPath, 
			IConfigReader configReader,
			IInitializePayrollFormats initializePayrollFormats,
			ITenantUnitOfWork tenantUnitOfWork,
			IServerConfigurationRepository serverConfigurationRepository)
		{
			_searchPath = searchPath ?? new SearchPath();
			_configReader = configReader ?? new ConfigReader();
			_initializePayrollFormats = initializePayrollFormats;
			_tenantUnitOfWork = tenantUnitOfWork;
			_serverConfigurationRepository = serverConfigurationRepository;
		}
		public void Handle(RefreshPayrollFormatsEvent parameters, CancellationTokenSource cancellationTokenSource, 
			Action<string> sendProgress, ref IEnumerable<object> returnObjects)
		{
			var addedFiles = new List<string>();

			if (!string.IsNullOrEmpty(_configReader.AppConfig("IsContainer")) && 
				!string.IsNullOrEmpty(_configReader.ConnectionString("AzureStorage")))
			{
				addedFiles = new PayrollDllCopy(_searchPath, _configReader).CopyPayrollDllFromAzureStorage(parameters.TenantName);
			}
			else
			{
				string sourcePayrollDirectory;
				using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
				{
					sourcePayrollDirectory = _serverConfigurationRepository.Get(ServerConfigurationKey.PayrollSourcePath);
				}
				//use default if not set in ServerConfiguration
				if (string.IsNullOrEmpty(sourcePayrollDirectory))
					sourcePayrollDirectory = _searchPath.PayrollDeployNewPath;
				
				if(!InstallationEnvironment.IsAzure)
					PayrollDllCopy.CopyFiles(sourcePayrollDirectory, _searchPath.Path, parameters.TenantName);
			}

			_initializePayrollFormats.RefreshOneTenant(parameters.TenantName);

			if (addedFiles.Count > 0)
			{
				addedFiles.ForEach(File.Delete);
				addedFiles.Clear();
			}
		}

	
	}
}
