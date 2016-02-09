using Autofac;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
	public class WorkerModule :Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TestJobCode>();
			builder.RegisterType<TestJobParams>();
			builder.RegisterType<TestJobWorker>().As<IHandle<TestJobParams>>();
			builder.RegisterType<LongRunningJobWorker>().As<IHandle<LongRunningJobParams>>();
			builder.RegisterType<FailingJobWorker>().As<IHandle<FailingJobParams>>();

				 // Register handlers.
			//builder.RegisterAssemblyTypes(nodeConfiguration.HandlerAssembly)
			//										 .Where(IsHandler)
			//										 .AsImplementedInterfaces()
			//										 .SingleInstance();
		}

		//private bool IsHandler(Type arg)
		//{
		//	return arg.GetInterfaces()
		//		 .Any(x =>
		//					 x.IsGenericType &&
		//					 x.GetGenericTypeDefinition() == typeof(IHandle<>));
		//}
	}
}