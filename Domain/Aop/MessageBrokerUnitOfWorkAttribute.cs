using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class MessageBrokerUnitOfWorkAttribute : AspectAttribute
	{
		public MessageBrokerUnitOfWorkAttribute()
			: base(typeof(IMessageBrokerUnitOfWorkAspect))
		{
		}
	}
}