namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork
{
	public interface ICurrentMessageBrokerUnitOfWork
	{
		ILiteUnitOfWork Current();
	}
}