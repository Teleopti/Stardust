using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using log4net;
using log4net.Config;
using Stardust.Node;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.ServiceBus.Container;
using Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[Serializable]
	public class ServiceBusRunner
	{
		private readonly IConfigReader _configReader;
		private readonly Action<int> _requestAdditionalTime;

		private IContainer _sharedContainer;
		private IInstallationEnvironment _installationEnvironment;
		private static readonly ILog logger = LogManager.GetLogger(typeof(ServiceBusRunner));

		public ServiceBusRunner(Action<int> requestAdditionalTime, IInstallationEnvironment installationEnvironment, IConfigReader configReader = null)
		{
			_requestAdditionalTime = requestAdditionalTime;
			_installationEnvironment = installationEnvironment;
			_configReader = configReader ?? new ConfigReader();
			Nodes = new List<NodeStarter>();
		}

		public void Start()
		{
			hostServiceStart();
		}

		private void hostServiceStart()
		{
			_requestAdditionalTime(60000);
			XmlConfigurator.Configure();
			logger.Info($"Starting service bus {nameof(ServiceBusRunner)}");

			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; //ignoreInvalidCertificate
			ServicePointManager.DefaultConnectionLimit = 50;

			var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(_configReader)
			{
				TeleoptiPrincipalForLegacy = true
			});
			_sharedContainer = new ContainerBuilder().Build();
			new ContainerConfiguration(_sharedContainer, toggleManager).Configure(null, _configReader);
			_sharedContainer.Resolve<IHangfireClientStarter>().Start();


			try
			{
				AppDomain.CurrentDomain.SetThreadPrincipal(new GenericPrincipal(new GenericIdentity("Anonymous"),
					new string[] { }));
			}
			catch (PolicyException)
			{
				//no way of knowing if the the principal is set or not
			}

			AppDomain.MonitoringIsEnabled = true;

			var numberOfNodes = _configReader.AppConfig("NumberOfNodes");
			if (numberOfNodes != null && numberOfNodes == "1" && !string.IsNullOrEmpty(_configReader.AppConfig("IsContainer")))
			{
				nodeStarter();
			}
			else
			{
				if (!toggleManager.IsEnabled(Toggles.Wfm_Payroll_SupportMultiDllPayrolls_75959))
					new PayrollDllCopy(new SearchPath()).CopyPayrollDll();

				Task.Run(() =>
				{
					var container = makeContainer(toggleManager, _sharedContainer, _configReader);
					if (!toggleManager.IsEnabled(Toggles.Wfm_Payroll_SupportMultiDllPayrolls_75959))
					{
						var initializePayrollFormats = new InitializePayrollFormatsToDb(container.Resolve<IPlugInLoader>(),
							container.Resolve<DataSourceForTenantWrapper>().DataSource()());
						initializePayrollFormats.Initialize();
					}
					
				}).ContinueWith(t =>
					{
						nodeStarter();
					}
				);
			}
		}

		private static IContainer makeContainer(IToggleManager toggleManager, IContainer sharedContainer, IConfigReader configReader)
		{
			var container = new ContainerBuilder().Build();
			new ContainerConfiguration(container, toggleManager).Configure(sharedContainer, configReader);
			return container;
		}

		public void Stop()
		{
			logger.Info($"Stopping service bus {nameof(ServiceBusRunner)}");
			hostServiceStop();
		}

		private void hostServiceStop()
		{
			_requestAdditionalTime(60000);

			if (Nodes.Any())
			{
				Nodes.ForEach(node =>
				{
					node.Stop();
				});
				Nodes = new List<NodeStarter>();
			}
		}

		private void nodeStarter()
		{
			var port = _configReader.ReadValue("port", 14100);
			var totalNodes = _configReader.ReadValue("NumberOfNodes", Environment.ProcessorCount);
			for (var portIndex = 1; portIndex <= totalNodes; portIndex++)
			{
				var nodeName = "Node" + portIndex;
				var localPort = port;
				var nodeThread = new Thread(() => startNode(localPort, nodeName));
				nodeThread.Start();
				// a little delay
				Thread.Sleep(3000);
				port++;
			}
		}

		private void startNode(int port, string nodeName)
		{
			var managerLocation = new Uri(_configReader.AppConfig("ManagerLocation"));
			var handlerAssembly = Assembly.Load(_configReader.ReadValue("handlerAssembly", "Teleopti.Ccc.Domain"));
			var pingToManagerSeconds = _configReader.ReadValue("pingToManagerSeconds", 120);
			// if changing this, also change in StardustServerStarter AllowedNodeDownTimeSeconds = 360
			var sendDetailsToManagerMilliSeconds = _configReader.ReadValue("sendDetailsToManagerMilliSeconds", 2000);
			var enableGC = _configReader.ReadValue("EnableGc", true);

			var fixedNodeIp = _configReader.ReadValue("FixedNodeIp", "");

			var iocArgs = new IocArgs(_configReader)
			{
				OptimizeScheduleChangedEvents_DontUseFromWeb = true,
				TeleoptiPrincipalForLegacy = true
			};

			var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);

			var fetchNodeConfiguration = new FetchNodeConfiguration(new WfmInstallationEnvironment());
			var nodeConfig = fetchNodeConfiguration.GetNodeConfiguration(port, nodeName, fixedNodeIp, managerLocation, handlerAssembly, pingToManagerSeconds, sendDetailsToManagerMilliSeconds, enableGC);
			
			var configuration = new IocConfiguration(iocArgs, toggleManager);
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new NodeHandlersModule(configuration));

			builder.RegisterType<BadgeCalculationRepository>().As<IBadgeCalculationRepository>();

			var container = builder.Build();

			var messageBroker = container.Resolve<IMessageBrokerComposite>();
			string messageBrokerConnection;
			var configurationAppSettings = ConfigurationManager.AppSettings.ToDictionary();
			if (!configurationAppSettings.TryGetValue("MessageBroker", out messageBrokerConnection))
			{
				var configvalues = new Dictionary<string, string>();
				configvalues.Add("MessageBroker", _configReader.AppConfig("MessageBroker"));
				new InitializeMessageBroker(messageBroker).Start(configvalues);
			}
			else
				new InitializeMessageBroker(messageBroker).Start(configurationAppSettings);


			var nodeStarter = new NodeStarter();
			Nodes.Add(nodeStarter);
			nodeStarter.Start(nodeConfig, container);

		}

		public IList<NodeStarter> Nodes { get; set; }
	}
}