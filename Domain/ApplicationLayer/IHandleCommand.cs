
namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IHandleCommand<TCommand>
	{
		void Handle(TCommand command);
	}
}