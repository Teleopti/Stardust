using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;
using Manager.Integration.Test.Helpers;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;

namespace Manager.Integration.Test.Tasks
{
	public class StartJobTaskHelper
	{
		public Task ExecuteDeleteJobTasks(IEnumerable<JobManagerTaskCreator> jobManagerTaskCreators,
		                                  CancellationTokenSource cancellationTokenSource,
		                                  TimeSpan timeOut)
		{
			return Task.Factory.StartNew(() =>
			{
				this.Log().DebugWithLineNumber("Start.");

				if (jobManagerTaskCreators != null && jobManagerTaskCreators.Any())
				{
					foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
					{
						jobManagerTaskCreator.StartAndWaitDeleteJobToManagerTask(timeOut);
					}

					var notSuccededTasks =
						jobManagerTaskCreators.Where(manager => manager.DeleteJobToManagerSucceeded == false);

					while (notSuccededTasks.Any())
					{
						foreach (var notSuccededTask in notSuccededTasks)
						{
							notSuccededTask.StartAndWaitDeleteJobToManagerTask(timeOut);
						}

						notSuccededTasks =
							jobManagerTaskCreators.Where(manager => manager.DeleteJobToManagerSucceeded == false);
					}
				}

				this.Log().DebugWithLineNumber("Finished.");
			});
		}

		public Task ExecuteCreateNewJobTasks(IEnumerable<JobManagerTaskCreator> jobManagerTaskCreators,
		                                     CancellationTokenSource cancellationTokenSource,
		                                     TimeSpan timeOut)
		{
			return Task.Factory.StartNew(() =>
			{
				this.Log().DebugWithLineNumber("Start.");

				if (jobManagerTaskCreators != null && jobManagerTaskCreators.Any())
				{
					foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
					{
						jobManagerTaskCreator.StartAndWaitCreateNewJobToManagerTask(timeOut);
						Thread.Sleep(TimeSpan.FromMilliseconds(200)); //five jobs each second
					}

					var notSuccededTasks =
						jobManagerTaskCreators.Where(manager => manager.CreateNewJobToManagerSucceeded == false);

					while (notSuccededTasks.Any())
					{
						foreach (var notSuccededTask in notSuccededTasks)
						{
							notSuccededTask.StartAndWaitCreateNewJobToManagerTask(timeOut);
						}

						notSuccededTasks =
							jobManagerTaskCreators.Where(manager => manager.CreateNewJobToManagerSucceeded == false);
					}
				}

				this.Log().DebugWithLineNumber("Finished.");
			});
		}
	}
}