using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Timers;
using Autofac;
using NodeTest.Fakes;
using NodeTest.Fakes.Timers;
using Stardust.Node.API;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace NodeTest.Attributes
{
	public class ProgressTestAttribute : BaseTestsAttribute
	{
		private Uri CallBackUriTemplateFake { get; set; }

		protected override void SetUp(ContainerBuilder builder)
		{
			var baseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);

			var managerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]);

			var handlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);

			var nodeName = ConfigurationManager.AppSettings["NodeName"];

			CallBackUriTemplateFake = managerLocation;

			var nodeConfiguration = new NodeConfiguration(baseAddress,
			                                              managerLocation,
			                                              handlerAssembly,
			                                              nodeName,
														  10);

			builder.RegisterAssemblyTypes(nodeConfiguration.HandlerAssembly)
				.Where(IsHandler)
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterType<InvokeHandler>();

			builder.RegisterType<SendJobFaultedTimerFake>().SingleInstance();

			builder.RegisterType<SendJobCanceledTimerFake>().SingleInstance();

			builder.RegisterType<SendJobDoneTimerFake>().SingleInstance();

			builder.RegisterType<NodeController>();

			builder.RegisterType<FakeHttpSender>().As<HttpSender>().SingleInstance();

			builder.RegisterInstance(nodeConfiguration);

			builder.RegisterType<TrySendJobDetailToManagerTimerFake>().WithParameter("interval", 1000d).As<TrySendJobDetailToManagerTimer>();
			builder.RegisterType<SendJobDoneTimerFake>().As<TrySendStatusToManagerTimer>();
			builder.RegisterType<SendJobCanceledTimerFake>().As<TrySendJobCanceledToManagerTimer>();
			builder.RegisterType<SendJobFaultedTimerFake>().As<TrySendJobFaultedToManagerTimer>();
			builder.RegisterType<NodeStartupNotificationToManagerFake>().As<TrySendNodeStartUpNotificationToManagerTimer>();
			builder.RegisterType<PingToManagerFake>().As<Timer>();

			builder.RegisterType<WorkerWrapper>().As<IWorkerWrapper>().SingleInstance();
		}

		private bool IsHandler(Type arg)
		{
			return arg.GetInterfaces()
				.Any(x =>
					     x.IsGenericType &&
					     x.GetGenericTypeDefinition() == typeof (IHandle<>));
		}
	}
}