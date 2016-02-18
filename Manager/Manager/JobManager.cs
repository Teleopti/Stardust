using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
    public class JobManager
    {
        private readonly IJobRepository _jobRepository;
        private readonly IWorkerNodeRepository _nodeRepository;
        private readonly IHttpSender _httpSender;

        private static readonly ILog Logger = LogManager.GetLogger(typeof (JobManager));

        public JobManager(IJobRepository jobRepository,
                          IWorkerNodeRepository nodeRepository,
                          IHttpSender httpSender)
        {
            _jobRepository = jobRepository;
            _nodeRepository = nodeRepository;
            _httpSender = httpSender;
        }

        public void CheckAndAssignNextJob()
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Start CheckAndAssignNextJob.");

            try
            {
                var availableNodes = _nodeRepository.LoadAllFreeNodes();

                var upNodes = new List<WorkerNode>();

                if (availableNodes != null && availableNodes.Any())
                {
                    LogHelper.LogInfoWithLineNumber(Logger,
                                    "Found ( " + availableNodes.Count + " ) available nodes");
                }

                foreach (var availableNode in availableNodes)
                {
                    var nodeUriBuilder = new NodeUriBuilderHelper(availableNode.Url);

                    Uri postUri = nodeUriBuilder.GetIsAliveTemplateUri();

                    LogHelper.LogInfoWithLineNumber(Logger,
                                                    "Test available node is alive : Url ( " + postUri + " )");

                    int numberOfTries = 0;

                    Task<bool> success= _httpSender.TryGetAsync(postUri);

                    while (success == null || !success.Result)
                    {
                        numberOfTries++;

                        Thread.Sleep(TimeSpan.FromMilliseconds(100));

                        LogHelper.LogInfoWithLineNumber(Logger,
                                                        "Try again to test available node is alive : Url ( " + postUri + " )");

                        success = _httpSender.TryGetAsync(postUri);

                        if (numberOfTries == 5)
                        {
                            break;
                        }
                    }

                    if (success.Result)
                    {
                        LogHelper.LogInfoWithLineNumber(Logger,
                                                        "Node Url ( " + postUri + " ) is available and alive.");

                        upNodes.Add(availableNode);
                    }
                }

                _jobRepository.CheckAndAssignNextJob(upNodes,
                                                     _httpSender);

                LogHelper.LogInfoWithLineNumber(Logger,
                                                "Finished CheckAndAssignNextJob.");
            }

            catch (Exception exp)
            {
                LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
                throw;
            }

        }

        public void Add(JobDefinition job)
        {
            _jobRepository.Add(job);

            CheckAndAssignNextJob();
        }

        public void CancelThisJob(Guid id)
        {
            _jobRepository.CancelThisJob(id,
                                         _httpSender);
        }

        public void SetEndResultOnJobAndRemoveIt(Guid jobId,
                                                 string result)
        {
            _jobRepository.SetEndResultOnJob(jobId,
                                             result);

            _jobRepository.DeleteJob(jobId);
        }

        public void ReportProgress(JobProgressModel model)
        {
            _jobRepository.ReportProgress(model.JobId,
                                          model.ProgressDetail);
        }

        public JobHistory GetJobHistory(Guid jobId)
        {
            return _jobRepository.History(jobId);
        }

        public IList<JobHistory> GetJobHistoryList()
        {
            return _jobRepository.HistoryList();
        }

        public IList<JobHistoryDetail> JobHistoryDetails(Guid jobId)
        {
            return _jobRepository.JobHistoryDetails(jobId);
        }
    }
}