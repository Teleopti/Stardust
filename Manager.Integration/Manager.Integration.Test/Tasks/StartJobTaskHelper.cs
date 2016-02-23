using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.Integration.Test.Helpers;

namespace Manager.Integration.Test.Tasks
{
	public class StartJobTaskHelper
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (StartJobTaskHelper));

		public Task ExecuteDeleteJobTasks(IEnumerable<JobManagerTaskCreator> jobManagerTaskCreators,
		                                  CancellationTokenSource cancellationTokenSource,
		                                  TimeSpan timeOut)
		{
			return Task.Factory.StartNew(() =>
			{
				LogHelper.LogDebugWithLineNumber("Start.",
				                                 Logger);

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

				LogHelper.LogDebugWithLineNumber("Finished.",
				                                 Logger);
			});
		}

		public Task ExecuteCreateNewJobTasks(IEnumerable<JobManagerTaskCreator> jobManagerTaskCreators,
		                                     CancellationTokenSource cancellationTokenSource,
		                                     TimeSpan timeOut)
		{
			return Task.Factory.StartNew(() =>
			{
				LogHelper.LogDebugWithLineNumber("Start.",
				                                 Logger);

				if (jobManagerTaskCreators != null && jobManagerTaskCreators.Any())
				{
					foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
					{
						jobManagerTaskCreator.StartAndWaitCreateNewJobToManagerTask(timeOut);
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

				LogHelper.LogDebugWithLineNumber("Finished.",
				                                 Logger);
			});
		}
	}
}