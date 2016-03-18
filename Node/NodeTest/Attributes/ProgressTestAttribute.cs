using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Autofac;
using NodeTest.Fakes;
using NodeTest.Fakes.Timers;
using Stardust.Node.API;
using Stardust.Node.Interfaces;
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

			builder.RegisterType<SendJobFaultedTimerFake>()
				.SingleInstance();

			builder.RegisterType<SendJobCanceledTimerFake>()
				.SingleInstance();

			builder.RegisterType<SendJobDoneTimerFake>()
				.SingleInstance();

			builder.RegisterType<NodeController>();

			builder.RegisterType<FakeHttpSender>()
				.SingleInstance();

			builder.RegisterInstance(nodeConfiguration);

			var trySendJobProgressToManagerTimerFake = new TrySendJobProgressToManagerTimerFake(nodeConfiguration,
																								new FakeHttpSender(),
																								1000);

			builder.RegisterInstance(trySendJobProgressToManagerTimerFake);

			builder.RegisterInstance(new SendJobDoneTimerFake(nodeConfiguration,
			                                                  CallBackUriTemplateFake,
															  trySendJobProgressToManagerTimerFake,
															  new FakeHttpSender()));

			builder.RegisterInstance(new SendJobCanceledTimerFake(nodeConfiguration,
			                                                      CallBackUriTemplateFake,
																  trySendJobProgressToManagerTimerFake,
																  new FakeHttpSender()));

			builder.RegisterInstance(new SendJobFaultedTimerFake(nodeConfiguration,
			                                                     CallBackUriTemplateFake,
																 trySendJobProgressToManagerTimerFake,
																 new FakeHttpSender()));

			builder.Register(context => new NodeStartupNotificationToManagerFake(nodeConfiguration,
																			  CallBackUriTemplateFake,
																			  new FakeHttpSender()));


			// Register IWorkerWrapper.
			builder.Register<IWorkerWrapper>(c => new WorkerWrapper(c.Resolve<InvokeHandler>(),
			                                                        nodeConfiguration,
			                                                        c.Resolve<NodeStartupNotificationToManagerFake>(),
			                                                        new PingToManagerFake(),
			                                                        c.Resolve<SendJobDoneTimerFake>(),
			                                                        c.Resolve<SendJobCanceledTimerFake>(),
			                                                        c.Resolve<SendJobFaultedTimerFake>(),
																	c.Resolve<TrySendJobProgressToManagerTimerFake>(),
																	c.Resolve<FakeHttpSender>()))
				.SingleInstance();
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