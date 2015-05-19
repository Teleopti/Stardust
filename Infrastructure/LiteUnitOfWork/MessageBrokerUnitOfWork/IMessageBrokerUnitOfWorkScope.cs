using System;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork
{
	public interface IMessageBrokerUnitOfWorkScope
	{
		void Start();
		void End(Exception exception);
	}
}