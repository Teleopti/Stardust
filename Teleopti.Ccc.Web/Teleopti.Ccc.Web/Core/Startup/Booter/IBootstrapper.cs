using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;

namespace Teleopti.Ccc.Web.Core.Startup.Booter
{
	public interface IBootstrapper
	{
		IEnumerable<Task> Run(IEnumerable<IBootstrapperTask> bootstrapperTasks, IAppBuilder application);
	}
}