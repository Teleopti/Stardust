using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using log4net.Config;
using Manager.Integration.Test.Database;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Tasks;
using Manager.IntegrationTest.Console.Host.Helpers;
using NUnit.Framework;

namespace Manager.Integration.Test.Initializers
{
	public abstract class InitializeAndFinalizeBaseOnTestFixtureSetup
	{
		protected InitializeAndFinalizeBaseOnTestFixtureSetup(int numberOfNodes,
		                                    int numberOfManagers,
											bool useLoadBalancerIfJustOneManager,
											bool waitToStartUp)
		{
			NumberOfNodes = numberOfNodes;
			NumberOfManagers = numberOfManagers;
			UseLoadBalancerIfJustOneManager = useLoadBalancerIfJustOneManager;
			WaitToStartUp = waitToStartUp;
		}

#if (DEBUG)
		protected const string BuildMode = "Debug";

#else
		protected  const string BuildMode = "Release";
#endif

		protected Task Task { get; set; }

		protected string ManagerDbConnectionString { get; set; }

		protected AppDomain AppDomain { get; set; }

		protected int NumberOfNodes { get; set; }

		protected int NumberOfManagers { get; set; }

		public bool UseLoadBalancerIfJustOneManager { get; private set; }

		protected bool WaitToStartUp { get; set; }

		protected AppDomainTask AppDomainTask { get; set; }

		protected CancellationTokenSource CancellationTokenSource { get; set; }

		protected virtual void RegisterContainer()
		{
			var containerBuilder = new ContainerBuilder();

			Container = containerBuilder.Build();

		}

		protected IContainer Container { get; set; }

		public HttpSender HttpSender { get; set; }

		public HttpRequestManager HttpRequestManager { get; set; }

		[SetUp]
		public virtual void SetUp()
		{
			DatabaseHelper.ClearJobData(ManagerDbConnectionString);
		}

		[TearDown]
		public virtual void TearDown()
		{
			
		}

		[OneTimeSetUp]
		public virtual void TestFixtureSetUp()
		{
			HttpSender = new HttpSender();
			HttpRequestManager = new HttpRequestManager();

			AppDomain = AppDomain.CurrentDomain;

			ManagerDbConnectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

			DatabaseHelper.ClearDatabase(ManagerDbConnectionString);

			CancellationTokenSource = new CancellationTokenSource();

			AppDomainTask = new AppDomainTask(BuildMode);

			Task = AppDomainTask.StartTask(numberOfManagers: NumberOfManagers,
			                               numberOfNodes: NumberOfNodes,
										   useLoadBalancerIfJustOneManager: UseLoadBalancerIfJustOneManager,
										   cancellationTokenSource: CancellationTokenSource);

			bool managerUp = HttpRequestManager.IsManagerUp();
			while (!managerUp)
			{
				managerUp = HttpRequestManager.IsManagerUp();
			}

			if (WaitToStartUp)
			{
				var sqlNotiferCancellationTokenSource = new CancellationTokenSource();
				var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

				var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(NumberOfNodes,
																	  sqlNotiferCancellationTokenSource);
				task.Start();

				sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(10));
				sqlNotifier.Dispose();
			}
		}

		[OneTimeTearDown]
		public virtual void TestFixtureTearDown()
        {
            AppDomainTask?.Dispose();
        }
	}
}