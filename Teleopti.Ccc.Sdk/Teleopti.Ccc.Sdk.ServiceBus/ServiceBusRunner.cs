using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Autofac;
using log4net.Config;
using Stardust.Node;
using Stardust.Node.API;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus.Container;
using Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[Serializable]
	public class ServiceBusRunner
	{
		private readonly Action<int> _requestAdditionalTime;

		[NonSerialized]
		private ConfigFileDefaultHost _requestBus;
		[NonSerialized]
		private ConfigFileDefaultHost _generalBus;
		[NonSerialized]
		private ConfigFileDefaultHost _denormalizeBus;
		[NonSerialized]
		private ConfigFileDefaultHost _payrollBus;
		[NonSerialized]
		private ConfigFileDefaultHost _rtaBus;

		private IContainer _sharedContainer;

		public ServiceBusRunner(Action<int> requestAdditionalTime)
		{
			_requestAdditionalTime = requestAdditionalTime;
		}

		public void Start()
		{
			hostServiceStart();
		}

		private void hostServiceStart()
		{
			_requestAdditionalTime(60000);

			XmlConfigurator.Configure(new FileInfo("log4net.config"));

			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; //ignoreInvalidCertificate
			ServicePointManager.DefaultConnectionLimit = 50;

			var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()));
			_sharedContainer = new ContainerBuilder().Build();
			new ContainerConfiguration(_sharedContainer, toggleManager).Configure(null);
			if (toggleManager.IsEnabled(Toggles.Wfm_Use_Stardust))
			{
				var nodeThread = new Thread(StartNode);
				nodeThread.Start();
			}

			var useRhino = true;

			bool.TryParse(ConfigurationManager.AppSettings["UseRhino"], out useRhino);

			if (useRhino)
			{
				_requestBus = new ConfigFileDefaultHost("RequestQueue.config", new BusBootStrapper(makeContainer(toggleManager, _sharedContainer)));
				_requestBus.Start();

				_generalBus = new ConfigFileDefaultHost("GeneralQueue.config", new GeneralBusBootStrapper(makeContainer(toggleManager, _sharedContainer)));
				_generalBus.Start();

				_denormalizeBus = new ConfigFileDefaultHost("DenormalizeQueue.config", new DenormalizeBusBootStrapper(makeContainer(toggleManager, _sharedContainer)));
				_denormalizeBus.Start();

				_rtaBus = new ConfigFileDefaultHost("RtaQueue.config", new RtaBusBootStrapper(makeContainer(toggleManager, _sharedContainer)));
				_rtaBus.Start();

				new PayrollDllCopy(new SearchPath()).CopyPayrollDll();

				_payrollBus = new ConfigFileDefaultHost("PayrollQueue.config", new PayrollBusBootStrapper(makeContainer(toggleManager, _sharedContainer)));
				_payrollBus.Start();
			}
			AppDomain.MonitoringIsEnabled = true;
		}

		private static IContainer makeContainer(IToggleManager toggleManager, IContainer sharedContainer)
		{
			var container = new ContainerBuilder().Build();
				new ContainerConfiguration(container, toggleManager).Configure(sharedContainer);
			return container;
		}

		public void Stop()
		{
			hostServiceStop();
		}

		private void hostServiceStop()
		{
			_requestAdditionalTime(60000);

			if (NodeStarter != null)
			{
				NodeStarter.Stop();
				NodeStarter = null;
			}
			DisposeBusHosts();
		}

		public void DisposeBusHosts()
		{
			if (_requestBus != null)
			{
				try
				{
					_requestBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
			if (_generalBus != null)
			{
				try
				{
					_generalBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
			if (_denormalizeBus != null)
			{
				try
				{
					_denormalizeBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
			if (_payrollBus != null)
			{
				try
				{
					_payrollBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
			if (_rtaBus != null)
			{
				try
				{
					_rtaBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
		}

		private void StartNode()
		{
			var assemblyName =
				Assembly.GetExecutingAssembly()
					.GetReferencedAssemblies()
					.FirstOrDefault(x => x.Name.Equals(ConfigurationManager.AppSettings["HandlerAssembly"]));
			if(assemblyName == null)
				throw new Exception("Can not find the Assembly specified in AppSettings['HandlerAssembly']");

			var assembly = Assembly.Load(assemblyName);
			var nodeConfig = new NodeConfiguration(new Uri(ConfigurationManager.AppSettings["NodeBaseAddress"]),
					 new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
					 assembly, 
					 ConfigurationManager.AppSettings["NodeName"],
					 int.Parse(ConfigurationManager.AppSettings["PingToManagerSeconds"]));

			var iocArgs = new IocArgs(new ConfigReader());
			var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new TenantServerModule(configuration));
			builder.RegisterModule(new NodeHandlersModule(configuration));
			var container = builder.Build();

			var messageBroker = container.Resolve<IMessageBrokerComposite>();
			new InitializeMessageBroker(messageBroker).Start(ConfigurationManager.AppSettings.ToDictionary());

			NodeStarter = new NodeStarter();
			NodeStarter.Start(nodeConfig, container);
		}

		public NodeStarter NodeStarter { get; set; }
	}
}