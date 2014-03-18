using System;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using Autofac;
using Autofac.Integration.Wcf;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Adherence;
using log4net;
using log4net.Config;
using ContainerBuilder = Teleopti.Ccc.Rta.Server.ContainerBuilder;

namespace Teleopti.Ccc.Rta.WebService
{
	public class Global : System.Web.HttpApplication
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (Global));
		private static Timer _timer;

		protected void Application_Start(object sender, EventArgs e)
		{
			XmlConfigurator.Configure();
			var container = buildIoc();
			AutofacHostFactory.Container = container;
			_timer = new Timer(flushBufferToDatabase, container, 0, 5000);

			container.Resolve<IRtaDataHandler>();
			container.Resolve<AdherenceAggregatorInitializor>().Initialize();

			setDefaultGenericPrincipal();
		}

		private static IContainer buildIoc()
		{
			var builder = ContainerBuilder.CreateBuilder();
			builder.RegisterType<TeleoptiRtaService>().SingleInstance();
			return builder.Build();
		}


		private void flushBufferToDatabase(object state)
		{
			var container = state as IContainer;
			var cache = container.Resolve<IActualAgentStateCache>();

			cache.FlushCacheToDatabase();
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		private static void setDefaultGenericPrincipal()
		{
			try
			{
				Logger.Debug("Trying to set default generic principal.");
				AppDomain.CurrentDomain.SetThreadPrincipal(new GenericPrincipal(new GenericIdentity("Anonymous"), new string[] { }));
			}
			catch (PolicyException policyException)
			{
				Logger.Debug("Failed to set thread principal for app domain, because it was already set.", policyException);
			}
		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{

		}
	}
}