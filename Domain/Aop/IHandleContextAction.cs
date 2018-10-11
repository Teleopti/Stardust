
namespace Teleopti.Ccc.Domain.Aop
{
	public interface IHandleContextAction<TContext>
	{
		void Handle(TContext command);
	}

}