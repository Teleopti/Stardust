using System;
using System.Security.Policy;
using System.Security.Principal;
using Autofac;
using MbCache.Configuration;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.Interfaces;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
	public static class RtaFactory
	{
		private static IRtaDataHandler _rtaDataHandler;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(IRtaDataHandler));

		public static IRtaDataHandler DataHandler
		{
			get
			{
				if (_rtaDataHandler == null)
				{
					var builder = configureContainer();
					_rtaDataHandler = builder.Resolve<IRtaDataHandler>();
				}
				setDefaultGenericPrincipal();
				return _rtaDataHandler;
			}
		}

		private static IContainer configureContainer()
		{
			var builder = new ContainerBuilder();

			var mbCacheModule = new MbCacheModule(new InMemoryCache(20), null);
			builder.RegisterModule(mbCacheModule);
			builder.RegisterModule(new RealTimeContainerInstaller(mbCacheModule));
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
				Logger.Info("Failed to set thread principal for app domain, because it was already set.", policyException);
			}
		}
	}
}
