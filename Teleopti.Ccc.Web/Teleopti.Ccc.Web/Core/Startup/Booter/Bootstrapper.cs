using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Core.Startup.Booter
{
	public class Bootstrapper : IBootstrapper
	{
		public void Run(IEnumerable<IBootstrapperTask> bootstrapperTasks)
		{
			foreach (var bootstrapperTask in bootstrapperTasks
										.OrderBy(t =>
													{
														var taskType = t.GetType();
														var attributePrio = (TaskPriorityAttribute)taskType.GetCustomAttributes(typeof(TaskPriorityAttribute), false).Single();
														return attributePrio.Priority;
													}

										))
			{
				bootstrapperTask.Execute();
			}
		}
	}
}