using System.Threading.Tasks;
using Owin;

namespace Teleopti.Ccc.Web.Core.Startup.Booter
{
	public interface IBootstrapperTask
	{
		Task Execute(IAppBuilder application);
	}
}
