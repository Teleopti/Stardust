using Autofac;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
	public class WorkerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TestReportProgressJobCode>();
			builder.RegisterType<TestReportProgressJobParams>();
			builder.RegisterType<TestReportProgressJobWorker>().As<IHandle<TestReportProgressJobParams>>();

			builder.RegisterType<TestJobCode>();
			builder.RegisterType<TestJobParams>();
			builder.RegisterType<TestJobWorker>().As<IHandle<TestJobParams>>();
			builder.RegisterType<FailingJobWorker>().As<IHandle<FailingJobParams>>();
		}
	}
}