using System;
using System.Security.Policy;
using System.Security.Principal;
using Autofac;
using Autofac.Integration.Wcf;
using log4net;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Adherence;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.IoC
{
	public class RtaAreaModule : Module
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(RtaAreaModule));

		protected override void Load(ContainerBuilder builder)
		{
			var container = buildIoc();
			AutofacHostFactory.Container = container;

			container.Resolve<IRtaDataHandler>();
			container.Resolve<AdherenceAggregatorInitializor>().Initialize();
			setDefaultGenericPrincipal();
		}

		private static IContainer buildIoc()
		{
			var builder = RtaContainerBuilder.CreateBuilder();
			builder.RegisterType<TeleoptiRtaService>().As<ITeleoptiRtaService>().SingleInstance();
			builder.RegisterModule(new DateAndTimeModule());
			return builder.Build();
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

	}
}