using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Helpers;

namespace Manager.Integration.Test.Tasks
{
    public class StartJobTaskHelper
    {
        public Task ExecuteTasks(IEnumerable<JobManagerTaskCreator> jobManagerTaskCreators,
                                 CancellationTokenSource cancellationTokenSource,
                                 TimeSpan timeOut)
        {
            return Task.Factory.StartNew(() =>
            {
                if (jobManagerTaskCreators != null && jobManagerTaskCreators.Any())
                {
                    // 1. Start all tasks.
                    foreach (var jobManagerTaskCreator in jobManagerTaskCreators)
                    {
                        jobManagerTaskCreator.StartCreateNewJobToManagerTask(timeOut);
                    }

                    IEnumerable<JobManagerTaskCreator> notSuccededTasks =
                        jobManagerTaskCreators.Where(manager => manager.CreateNewJobToManagerSucceeded == false);

                    while (notSuccededTasks.Any())
                    {
                        foreach (var notSuccededTask in notSuccededTasks)
                        {
                            notSuccededTask.StartCreateNewJobToManagerTask(timeOut);
                        }

                        notSuccededTasks =
                            jobManagerTaskCreators.Where(manager => manager.CreateNewJobToManagerSucceeded == false);
                    }
                }
            });
        }
    }
}