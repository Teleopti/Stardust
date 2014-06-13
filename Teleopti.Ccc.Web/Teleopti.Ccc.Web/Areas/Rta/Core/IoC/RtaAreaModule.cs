using System;
using System.Security.Policy;
using System.Security.Principal;
using Autofac;
using Autofac.Integration.Wcf;
using log4net;
using MbCache.Configuration;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Adherence;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.IoC
{
	public class RtaAreaModule : Module
	{
		private readonly MbCacheModule _mbCacheModule;

		public RtaAreaModule(MbCacheModule mbCacheModule)
		{
			_mbCacheModule = mbCacheModule;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule(new RealTimeContainerInstaller(_mbCacheModule));
			//builder.RegisterModule(new DateAndTimeModule());
			builder.RegisterType<TeleoptiRtaService>().As<ITeleoptiRtaService>().SingleInstance();

			//var container = buildIoc();
			//AutofacHostFactory.Container = container;

			//container.Resolve<IRtaDataHandler>();
			//container.Resolve<AdherenceAggregatorInitializor>().Initialize();
			//setDefaultGenericPrincipal();
		}

		//private static IContainer buildIoc()
		//{
		//	//var builder = new ContainerBuilder();


		//	//var mbCacheModule = new MbCacheModule(new InMemoryCache(20), null);
		//	//builder.RegisterModule(mbCacheModule);
		//	//builder.RegisterModule(new RealTimeContainerInstaller(mbCacheModule));
		//	//builder.RegisterModule(new DateAndTimeModule());
		//	//builder.RegisterType<TeleoptiRtaService>().As<ITeleoptiRtaService>().SingleInstance();
		//	//return builder.Build();
		//}

		//private static void setDefaultGenericPrincipal()
		//{
		//	try
		//	{
		//		Logger.Debug("Trying to set default generic principal.");
		//		AppDomain.CurrentDomain.SetThreadPrincipal(new GenericPrincipal(new GenericIdentity("Anonymous"), new string[] { }));
		//	}
		//	catch (PolicyException policyException)
		//	{
		//		Logger.Debug("Failed to set thread principal for app domain, because it was already set.", policyException);
		//	}
		//}

	}
}