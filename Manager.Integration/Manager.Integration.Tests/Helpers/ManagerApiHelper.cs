using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Timers;
using Newtonsoft.Json;

namespace Manager.Integration.Test.Helpers
{
    public class ManagerApiHelper
    {
        public ManagerApiHelper()
        {
            ManagerUriBuilder =new ManagerUriBuilder();
        }

        public ManagerUriBuilder ManagerUriBuilder { get; private set; }

        public ManagerApiHelper(CheckJobHistoryStatusTimer checkJobHistoryStatusTimer)
        {
            CheckJobHistoryStatusTimer = checkJobHistoryStatusTimer;
        }

        public CheckJobHistoryStatusTimer CheckJobHistoryStatusTimer { get; set; }

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
                    DefineDefaultRequestHeaders(client);

                    var sez = JsonConvert.SerializeObject(jobRequestModel);

                    var uri = ManagerUriBuilder.GetStartJobUri();

                    HttpResponseMessage response = await client.PostAsync(uri,
                                                                          new StringContent(sez,
                                                                                            Encoding.UTF8,
                                                                                            MediaTypeConstants.ApplicationJson));
                    var str = await response.Content.ReadAsStringAsync();

                    try
                    {
                        Guid jobId = JsonConvert.DeserializeObject<Guid>(str);

                        CheckJobHistoryStatusTimer.AddOrUpdateGuidStatus(jobId,
                                                                         null);
                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }

        public Task<HttpResponseMessage> CreateManagerCancelTask(Guid guid)
        {
            Task<HttpResponseMessage> taskToReturn =
                new Task<HttpResponseMessage>(() =>
                {
                    Task<HttpResponseMessage> response;

                    using (var client = new HttpClient())
                    {
                        DefineDefaultRequestHeaders(client);

                        var uri = ManagerUriBuilder.GetCancelJobUri(guid);

                        response = client.DeleteAsync(uri);
                    }

                    return response.Result;
                });

            return taskToReturn;
        }
    }
}