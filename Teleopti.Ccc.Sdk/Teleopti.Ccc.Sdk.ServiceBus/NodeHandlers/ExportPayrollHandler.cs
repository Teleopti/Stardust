using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;


namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ExportPayrollHandler : IHandle<RunPayrollExportEvent>
	{
		private readonly IComponentContext _componentContext;

		public ExportPayrollHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		[AsSystem, UnitOfWork]
		public virtual void Handle(RunPayrollExportEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			AuthenticationMessageHeader.BusinessUnit = parameters.LogOnBusinessUnitId;
			AuthenticationMessageHeader.DataSource = parameters.LogOnDatasource;
			AuthenticationMessageHeader.UserName = SystemUser.Id.ToString(); 
			AuthenticationMessageHeader.Password = "custom";
			AuthenticationMessageHeader.UseWindowsIdentity = false;
			var theRealOne = _componentContext.Resolve<PayrollExportHandler>();
			theRealOne.Handle(parameters);
		}
	}
}