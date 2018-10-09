
namespace Teleopti.Ccc.Web.Areas.Global.Aspect
{
	public interface IHandleContextAction<TContext>
	{
		void Handle(TContext command);
	}

}