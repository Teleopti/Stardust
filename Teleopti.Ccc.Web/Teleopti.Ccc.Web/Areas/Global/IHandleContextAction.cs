
namespace Teleopti.Ccc.Web.Areas.Global
{
	public interface IHandleContextAction<TContext>
	{
		void Handle(TContext command);
	}

}