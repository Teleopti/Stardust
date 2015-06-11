using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.MessageBroker
{
	public class MessageBrokerServerTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			
			builder.UseTestDouble<FakeMessageBrokerUnitOfWorkAspect>().For<IMessageBrokerUnitOfWorkAspect>();

			builder.UseTestDouble(new FakeSignalR()).For<ISignalR>();
			builder.UseTestDouble(new ActionImmediate()).For<IActionScheduler>();

			builder.UseTestDouble(new FakeCurrentDatasource()).For<ICurrentDataSource>();
			builder.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();

			builder.UseTestDouble(new FakeMailboxRepository()).For<IMailboxRepository>();
		}
	}
}