using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Owin;

namespace Teleopti.Ccc.Web.Core.Startup.Booter
{
	public class Bootstrapper : IBootstrapper
	{
		public IEnumerable<Task> Run(IEnumerable<IBootstrapperTask> bootstrapperTasks, IAppBuilder application)
		{
			bootstrapperTasks = bootstrapperTasks
				.OrderBy(t =>
					{
						var taskType = t.GetType();
						var attributePrio = (TaskPriorityAttribute)taskType.GetCustomAttributes(typeof(TaskPriorityAttribute), false).FirstOrDefault();
						return attributePrio != null ? attributePrio.Priority : int.MaxValue;
					})
				.ToArray();
			return bootstrapperTasks.Select(t => t.Execute(application)).Where(x => x != null).ToArray();
		}
	}
}