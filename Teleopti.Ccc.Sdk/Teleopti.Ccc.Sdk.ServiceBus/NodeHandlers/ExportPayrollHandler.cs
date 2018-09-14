using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
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

		public ExportPayrollHandler(IComponentContext componentContext, 
				IStardustJobFeedback stardustJobFeedback, 
				ISearchPath searchPath, 
				IConfigReader configReader)
		{
			_componentContext = componentContext;
			_stardustJobFeedback = stardustJobFeedback;
			_searchPath = searchPath ?? new SearchPath();
			_configReader = configReader ?? new ConfigReader();
		}

		[AsSystem, UnitOfWork]
		public void Handle(RunPayrollExportEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress, ref IEnumerable<object> returnObjects)
		{
			var addedFiles = new List<string>();

			if (!string.IsNullOrEmpty(_configReader.AppConfig("IsContainer")) && 
				!string.IsNullOrEmpty(_configReader.ConnectionString("AzureStorage")))
			{
				addedFiles = new PayrollDllCopy(_searchPath, _configReader).CopyPayrollDllFromAzureStorage(parameters.LogOnDatasource);
			}

			_stardustJobFeedback.SendProgress = sendProgress;
			AuthenticationMessageHeader.BusinessUnit = parameters.LogOnBusinessUnitId;
			AuthenticationMessageHeader.DataSource = parameters.LogOnDatasource;
			AuthenticationMessageHeader.UserName = SystemUser.Id.ToString();
			AuthenticationMessageHeader.Password = "custom";
			AuthenticationMessageHeader.UseWindowsIdentity = false;
			var theRealOne = _componentContext.Resolve<IHandleEvent<RunPayrollExportEvent>>();
			theRealOne.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;

			if (addedFiles.Count > 0)
			{
				addedFiles.ForEach(File.Delete);
				addedFiles.Clear();
			}
		}
	}
}