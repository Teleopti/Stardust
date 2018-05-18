using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Stardust.Node.ReturnObjects;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.ServiceBus.Custom;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ExportPayrollHandler : IHandle<RunPayrollExportEvent>
	{
		private readonly IComponentContext _componentContext;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly ISearchPath _searchPath;
		private readonly IConfigReader _configReader;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly IPlugInLoader _pluginInLoader;
		private readonly IEnvironmentVariable _environmentVariable;

		public ExportPayrollHandler(IComponentContext componentContext, 
				IStardustJobFeedback stardustJobFeedback, 
				ISearchPath searchPath, 
				IConfigReader configReader, 
				IDataSourceForTenant dataSourceForTenant, 
				IPlugInLoader pluginInLoader, 
				IEnvironmentVariable environmentVariable)
		{
			_componentContext = componentContext;
			_stardustJobFeedback = stardustJobFeedback;
			_searchPath = searchPath ?? new SearchPath();
			_configReader = configReader ?? new ConfigReader();
			_dataSourceForTenant = dataSourceForTenant;
			_pluginInLoader = pluginInLoader;
			_environmentVariable = environmentVariable;
		}

		[AsSystem, UnitOfWork]
		public void Handle(RunPayrollExportEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress, ref IEnumerable<object> returnObjects)
		{
			if (!string.IsNullOrEmpty(_environmentVariable.GetValue("IS_CONTAINER")) &&
				!string.IsNullOrEmpty(_configReader.ConnectionString("AzureStorage")) &&
				parameters.LogOnDatasource != null)
			{
				new PayrollDllCopy(_searchPath, _configReader).CopyPayrollDllFromAzureStorage(parameters.LogOnDatasource);
				//Refresh dlls in db.
				new InitializePayrollFormatsToDb(_pluginInLoader, _dataSourceForTenant).InitializeOneTenant(null, parameters.LogOnDatasource);
				//Force stardust to exit. (Only for container restart).
				returnObjects = new List<object>() {
					new ExitApplication() {ExitCode = 0}
				};
			}

			_stardustJobFeedback.SendProgress = sendProgress;
			AuthenticationMessageHeader.BusinessUnit = parameters.LogOnBusinessUnitId;
			AuthenticationMessageHeader.DataSource = parameters.LogOnDatasource;
			AuthenticationMessageHeader.UserName = SystemUser.Id.ToString();
			AuthenticationMessageHeader.Password = "custom";
			AuthenticationMessageHeader.UseWindowsIdentity = false;
			var theRealOne = _componentContext.Resolve<PayrollExportHandler>();
			theRealOne.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}