using System;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class InterceptorPipelineModule : HubPipelineModule
	{
		private readonly Lazy<ICurrentUnitOfWorkFactory> _unitOfWorkFactorProvider;
		private IUnitOfWork _unitOfWork;

		public InterceptorPipelineModule(Lazy<ICurrentUnitOfWorkFactory> unitOfWorkFactory)
		{
			_unitOfWorkFactorProvider = unitOfWorkFactory;
		}

		protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
		{
			if (context.MethodDescriptor.Attributes.Any(a => a.GetType().Name.Contains("UnitOfWork")))
			{
				_unitOfWork = _unitOfWorkFactorProvider.Value.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
			}
			return base.OnBeforeIncoming(context);
		}

		protected override void OnAfterOutgoing(IHubOutgoingInvokerContext context)
		{
			base.OnAfterOutgoing(context);

			if (_unitOfWork != null)
			{
				_unitOfWork.PersistAll();
				_unitOfWork.Dispose();
				_unitOfWork = null;
			}
		}
	}
}