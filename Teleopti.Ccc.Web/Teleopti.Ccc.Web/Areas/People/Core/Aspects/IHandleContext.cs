
namespace Teleopti.Ccc.Web.Areas.People.Core.Aspects
{
	public interface IHandleContext<TContext>
	{
		void Handle(TContext command);
	}

}