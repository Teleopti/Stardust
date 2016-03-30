using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Castle.DynamicProxy;
using log4net.Config;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Interceptor;
using Manager.Integration.Test.Notifications;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.Validators;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using NUnit.Framework;

namespace Manager.Integration.Test.Initializers
{
	public abstract class InitializeAndFinalizeBaseOnSetup
	{
		protected InitializeAndFinalizeBaseOnSetup(int numberOfNodes,
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
		protected const bool ClearDatabase = true;
		protected const string BuildMode = "Debug";

#else
		protected  const bool ClearDatabase = true;
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

			containerBuilder.RegisterType<FunctionalTestInterceptor>()
				.Named<IInterceptor>("FunctionalTestInterceptor");

			Container = containerBuilder.Build();
		}

		protected IContainer Container { get; set; }

		[SetUp]
		public virtual void SetUp()
		{
			AppDomain = AppDomain.CurrentDomain;

			AppDomain.UnhandledException += AppDomainUnHandledException;

			ManagerDbConnectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

			if (ClearDatabase)
			{
				DatabaseHelper.TryClearDatabase(ManagerDbConnectionString);
			}

			CancellationTokenSource = new CancellationTokenSource();

			AppDomainTask = new AppDomainTask(BuildMode);

			Task = AppDomainTask.StartTask(numberOfManagers: NumberOfManagers,
										   numberOfNodes: NumberOfNodes,
										   useLoadBalancerIfJustOneManager: UseLoadBalancerIfJustOneManager,
										   cancellationTokenSource: CancellationTokenSource);



			if (WaitToStartUp)
			{
				var sqlNotiferCancellationTokenSource = new CancellationTokenSource();
				var sqlNotifier = new SqlNotifier(ManagerDbConnectionString);

				var task = sqlNotifier.CreateNotifyWhenNodesAreUpTask(NumberOfNodes,
																	  sqlNotiferCancellationTokenSource,
																	  IntegerValidators.Value1IsEqualToValue2Validator);
				task.Start();

				sqlNotifier.NotifyWhenAllNodesAreUp.Wait(TimeSpan.FromMinutes(10));
				sqlNotifier.Dispose();
			}

		}

		[TearDown]
		public virtual void TearDown()
		{
			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}
		}

		[TestFixtureSetUp]
		public virtual void TestFixtureSetUp()
		{
		}

		[TestFixtureTearDown]
		public virtual void TestFixtureTearDown()
		{
		}

		protected virtual void AppDomainUnHandledException(object sender,
		                                                   UnhandledExceptionEventArgs e)
		{
			var exp = e.ExceptionObject as Exception;

			if (exp != null)
			{
				this.Log().FatalWithLineNumber(exp.Message,
				                               exp);
			}
		}
	}
}