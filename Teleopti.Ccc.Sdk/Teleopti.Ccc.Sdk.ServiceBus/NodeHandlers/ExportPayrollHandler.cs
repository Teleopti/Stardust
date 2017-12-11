using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;


namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ExportPayrollHandler : IHandle<RunPayrollExportEvent>
	{
		private readonly IComponentContext _componentContext;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public ExportPayrollHandler(IComponentContext componentContext, IStardustJobFeedback stardustJobFeedback)
		{
			_componentContext = componentContext;
			_stardustJobFeedback = stardustJobFeedback;
		}

		[AsSystem, UnitOfWork]
		public virtual void Handle(RunPayrollExportEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
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