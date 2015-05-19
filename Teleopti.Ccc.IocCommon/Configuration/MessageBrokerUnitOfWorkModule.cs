using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MessageBrokerUnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<MessageBrokerUnitOfWorkState>()
				.As<ICurrentMessageBrokerUnitOfWork>()
				.As<IMessageBrokerUnitOfWorkScope>()
				.SingleInstance();
			builder.RegisterType<MessageBrokerUnitOfWorkAspect>()
				.As<IMessageBrokerUnitOfWorkAspect>()
				.SingleInstance();
		}
	}
}