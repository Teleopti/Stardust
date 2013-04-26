using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Web.Core.Startup.Booter
{
	public interface IBootstrapper
	{
		IEnumerable<Task> Run(IEnumerable<IBootstrapperTask> bootstrapperTasks);
	}
}