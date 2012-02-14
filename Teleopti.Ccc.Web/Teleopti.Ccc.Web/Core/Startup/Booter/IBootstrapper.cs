using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Core.Startup.Booter
{
	public interface IBootstrapper
	{
		void Run(IEnumerable<IBootstrapperTask> bootstrapperTasks);
	}
}