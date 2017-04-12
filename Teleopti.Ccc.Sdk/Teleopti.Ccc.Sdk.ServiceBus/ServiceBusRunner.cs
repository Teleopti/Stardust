using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
using log4net.Repository.Hierarchy;
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
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus.Container;
using Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[Serializable]
	public class ServiceBusRunner
	{
		private readonly IConfigReader _configReader;
		private readonly Action<int> _requestAdditionalTime;

		[NonSerialized]
		private ConfigFileDefaultHost _requestBus;

		[NonSerialized]
		private ConfigFileDefaultHost _generalBus;

		[NonSerialized]
		private ConfigFileDefaultHost _denormalizeBus;

		private IContainer _sharedContainer;
		private static readonly ILog logger = LogManager.GetLogger(typeof(ServiceBusRunner));

		public ServiceBusRunner(Action<int> requestAdditionalTime )
		{
			_requestAdditionalTime = requestAdditionalTime;
			_configReader = new ConfigReader();
			Nodes = new List<NodeStarter>();
		}

		public void Start()
		{
			hostServiceStart();
		}

		private void hostServiceStart()
		{
			_requestAdditionalTime(60000);
			XmlConfigurator.Configure(new FileInfo("log4net.config"));
			logger.Info($"Starting service bus {nameof(ServiceBusRunner)}");

			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; //ignoreInvalidCertificate
			ServicePointManager.DefaultConnectionLimit = 50;

			var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()));
			_sharedContainer = new ContainerBuilder().Build();
			new ContainerConfiguration(_sharedContainer, toggleManager).Configure(null);
			_sharedContainer.Resolve<HangfireClientStarter>().Start();

            var useRhino = true;

			bool.TryParse(ConfigurationManager.AppSettings["UseRhino"], out useRhino);

		    if (useRhino)
		    {
		        logger.Debug("Using rhino and it services");

		        _generalBus = new ConfigFileDefaultHost("GeneralQueue.config",
		            new GeneralBusBootStrapper(makeContainer(toggleManager, _sharedContainer)));
		        _generalBus.Start();

		        _requestBus = new ConfigFileDefaultHost("RequestQueue.config",
		            new BusBootStrapper(makeContainer(toggleManager, _sharedContainer)));
		        _requestBus.Start();

		        _denormalizeBus = new ConfigFileDefaultHost("DenormalizeQueue.config",
		            new DenormalizeBusBootStrapper(makeContainer(toggleManager, _sharedContainer)));
		        _denormalizeBus.Start();
		    }
		    else
		    {
		        try
		        {
		            AppDomain.CurrentDomain.SetThreadPrincipal(new GenericPrincipal(new GenericIdentity("Anonymous"),
		                new string[] {}));
		        }
		        catch (PolicyException)
		        {
		            //no way of knowing if the the principal is set or not
		        }
		    }
		    AppDomain.MonitoringIsEnabled = true;

			new PayrollDllCopy(new SearchPath()).CopyPayrollDll();

			Task.Run(() =>
			{
				var container = makeContainer(toggleManager, _sharedContainer);
				var initializePayrollFormats = new InitializePayrollFormatsToDb(container.Resolve<IPlugInLoader>(),
					container.Resolve<DataSourceForTenantWrapper>().DataSource()());
				initializePayrollFormats.Initialize();
			}).ContinueWith(t =>
			{
                nodeStarter();
            }
            );
		}

		private static IContainer makeContainer(IToggleManager toggleManager, IContainer sharedContainer)
		{
			var container = new ContainerBuilder().Build();
			new ContainerConfiguration(container, toggleManager).Configure(sharedContainer);
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
					// ignored
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
					// ignored
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
					// ignored
				}
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
			var nodeConfig = new NodeConfiguration(
				new Uri(_configReader.AppConfig("ManagerLocation")),
				Assembly.Load(_configReader.ReadValue("handlerAssembly", "Teleopti.Ccc.Domain")),
				port,
				nodeName,
				_configReader.ReadValue("pingToManagerSeconds", 30),
				_configReader.ReadValue("sendDetailsToManagerMilliSeconds", 2000)
				);

			var iocArgs = new IocArgs(new ConfigReader())
			{
				OptimizeScheduleChangedEvents_DontUseFromWeb = true
			};
			
			var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new TenantServerModule(configuration));
			builder.RegisterModule(new NodeHandlersModule(configuration));

			// Implementation for IBadgeCalculationRepository is different in ServiceBus and ETL
			// So it's not placed in CommonModule
			builder.RegisterType<BadgeCalculationRepository>().As<IBadgeCalculationRepository>();

			var container = builder.Build();

			var messageBroker = container.Resolve<IMessageBrokerComposite>();
			new InitializeMessageBroker(messageBroker).Start(ConfigurationManager.AppSettings.ToDictionary());

			var nodeStarter = new NodeStarter();
			Nodes.Add(nodeStarter);
			nodeStarter.Start(nodeConfig, container);
			
		}

		public IList<NodeStarter> Nodes { get; set; }
	}
}