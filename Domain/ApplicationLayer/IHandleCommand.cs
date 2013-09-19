
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[IsNotDeadCode("Command handler resolved dynamically by implementations of ICommandDispatcher")]
	public interface IHandleCommand<TCommand>
	{
		void Handle(TCommand command);
	}
}