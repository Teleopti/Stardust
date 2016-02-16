using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Timers;
using Newtonsoft.Json;

namespace Manager.Integration.Test.Helpers
{
    public class JobManagerTaskCreator : IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (JobManagerTaskCreator));

        public ManagerUriBuilder ManagerUriBuilder { get; private set; }

        public JobManagerTaskCreator(CheckJobHistoryStatusTimer checkJobHistoryStatusTimer)
        {
            CancellationTokenSource = new CancellationTokenSource();

            ManagerUriBuilder = new ManagerUriBuilder();

            CheckJobHistoryStatusTimer = checkJobHistoryStatusTimer;
        }

        private CheckJobHistoryStatusTimer CheckJobHistoryStatusTimer { get; set; }

        private void DefineDefaultRequestHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeConstants.ApplicationJson));
        }

        public void StartDeleteJobToManagerTask(TimeSpan timeout)
        {
            DeleteJobToManagerTask.Start();

            DeleteJobToManagerTask.Wait(timeout);
        }

        public void StartCreateNewJobToManagerTask(TimeSpan timeout)
        {
            NewJobToManagerTask.Start();

            NewJobToManagerTask.Wait(timeout);
        }

        public bool CreateNewJobToManagerSucceeded { get; set; }

        public bool DeleteJobToManagerSucceeded { get; set; }

        public void CreateNewJobToManagerTask(JobRequestModel jobRequestModel)
        {
            NewJobToManagerTask = new Task(async () =>
            {
                CreateNewJobToManagerSucceeded = false;

                using (var client = new HttpClient())
                {
                    DefineDefaultRequestHeaders(client);

                    var sez = JsonConvert.SerializeObject(jobRequestModel);

                    Uri uri = ManagerUriBuilder.GetStartJobUri();

                    try
                    {
                        LogHelper.LogInfoWithLineNumber("Start calling post async. Uri ( " + uri + " )",
                                                        Logger);

                        HttpResponseMessage response = await client.PostAsync(uri,
                                                                              new StringContent(sez,
                                                                                                Encoding.UTF8,
                                                                                                MediaTypeConstants.ApplicationJson),
                                                                              CancellationTokenSource.Token);

                        CreateNewJobToManagerSucceeded = response.IsSuccessStatusCode;

                        if (CreateNewJobToManagerSucceeded)
                        {
                            var str = await response.Content.ReadAsStringAsync();

                            Guid jobId = JsonConvert.DeserializeObject<Guid>(str);

                            CheckJobHistoryStatusTimer.AddOrUpdateGuidStatus(jobId,
                                                                             null);
                        }

                        LogHelper.LogInfoWithLineNumber("Finished calling post async. Uri ( " + uri + " )",
                                                        Logger);
                    }

                    catch (Exception)
                    {
                        // Supress all errors is OK.                            
                    }
                }
            },
                                           CancellationTokenSource.Token);
        }

        private Task NewJobToManagerTask { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        public void CreateDeleteJobToManagerTask(Guid guid)
        {
            DeleteJobToManagerTask = new Task(async () =>
            {
                DeleteJobToManagerSucceeded = false;

                using (var client = new HttpClient())
                {
                    DefineDefaultRequestHeaders(client);

                    var uri = ManagerUriBuilder.GetCancelJobUri(guid);

                    try
                    {
                        LogHelper.LogInfoWithLineNumber("Start calling delete async. Uri ( " + uri + " )",
                                                        Logger);

                        var response = await client.DeleteAsync(uri,
                                                                CancellationTokenSource.Token);

                        DeleteJobToManagerSucceeded = response.IsSuccessStatusCode;

                        if (DeleteJobToManagerSucceeded)
                        {
                            await response.Content.ReadAsStringAsync();
                        }

                        LogHelper.LogInfoWithLineNumber("Finished calling delete async. Uri ( " + uri + " )",
                                                        Logger);
                    }

                    catch (Exception)
                    {
                        // Supress all errors is OK.    
                    }
                }
            },
                                              CancellationTokenSource.Token);
        }

        private Task DeleteJobToManagerTask { get; set; }

        public void Dispose()
        {
            if (CancellationTokenSource != null &&
                !CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }

            if (NewJobToManagerTask != null)
            {
                NewJobToManagerTask.Dispose();
            }

            if (DeleteJobToManagerTask != null)
            {
                DeleteJobToManagerTask.Dispose();
            }
        }
    }
}