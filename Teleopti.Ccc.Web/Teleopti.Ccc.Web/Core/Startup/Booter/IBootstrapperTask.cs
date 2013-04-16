using System.Threading.Tasks;

namespace Teleopti.Ccc.Web.Core.Startup.Booter
{
	public interface IBootstrapperTask
	{
		Task Execute();
	}
}
