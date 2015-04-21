using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using log4net;
using IErrorHandler = System.ServiceModel.Dispatcher.IErrorHandler;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	public class Log4NetWcfLogger : IErrorHandler
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
 
		public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
		{
		}

		public bool HandleError(Exception error)
		{
			log.Error("An unexpected has occurred.", error);

			return false;
		}
	}

	public class Log4NetServiceBehavior : IServiceBehavior
	{
		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints,
			BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			IErrorHandler errorHandler = new Log4NetWcfLogger();

			foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
			{
				channelDispatcher.ErrorHandlers.Add(errorHandler);
			}
		}
	}

	public class Log4NetBehaviorExtensionElement : BehaviorExtensionElement
	{
		public override Type BehaviorType
		{
			get { return typeof(Log4NetServiceBehavior); }
		}

		protected override object CreateBehavior()
		{
			return new Log4NetServiceBehavior();
		}
	}
}