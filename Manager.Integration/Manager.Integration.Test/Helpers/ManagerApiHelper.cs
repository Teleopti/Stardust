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
    public class ManagerApiHelper : IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (ManagerApiHelper));


        public ManagerUriBuilder ManagerUriBuilder { get; private set; }

        public ManagerApiHelper(CheckJobHistoryStatusTimer checkJobHistoryStatusTimer)
        {
            CancellationTokenSource = new CancellationTokenSource();

            ManagerUriBuilder = new ManagerUriBuilder();

            CheckJobHistoryStatusTimer = checkJobHistoryStatusTimer;
        }

        public CheckJobHistoryStatusTimer CheckJobHistoryStatusTimer { get; private set; }

        public void DefineDefaultRequestHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeConstants.ApplicationJson));
        }

        public Task CreateManagerDoThisTask(JobRequestModel jobRequestModel)
        {
            return new Task(async () =>
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        DefineDefaultRequestHeaders(client);

                        var sez = JsonConvert.SerializeObject(jobRequestModel);

                        var uri = ManagerUriBuilder.GetStartJobUri();

                        HttpResponseMessage response = await client.PostAsync(uri,
                                                                              new StringContent(sez,
                                                                                                Encoding.UTF8,
                                                                                                MediaTypeConstants.ApplicationJson),
                                                                              CancellationTokenSource.Token);
                        response.EnsureSuccessStatusCode();

                        var str = await response.Content.ReadAsStringAsync();

                        Guid jobId = JsonConvert.DeserializeObject<Guid>(str);

                        CheckJobHistoryStatusTimer.AddOrUpdateGuidStatus(jobId,
                                                                         null);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogErrorWithLineNumber("ManagerApiHelper CreateManagerDoThisTask problem with Post Async" + ex,
                                                         Logger);
                    }
                }
            });
        }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        public Task CreateManagerCancelTask(Guid guid)
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                return null;
            }

            return new Task(async () =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        DefineDefaultRequestHeaders(client);

                        var uri = ManagerUriBuilder.GetCancelJobUri(guid);

                        var response = await client.DeleteAsync(uri,
                                                                CancellationTokenSource.Token);

                        response.EnsureSuccessStatusCode();

                        await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception exp)
                {
                    LogHelper.LogErrorWithLineNumber("Delete async error : ",
                                                     Logger,
                                                     exp);
                }
            });
        }

        public void Dispose()
        {
            if (CancellationTokenSource != null &&
                !CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }

            if (CheckJobHistoryStatusTimer != null)
            {
                CheckJobHistoryStatusTimer.Dispose();
            }
        }
    }
}