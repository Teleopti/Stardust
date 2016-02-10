using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Manager.Integration.Test.EventArgs;
using Manager.Integration.Test.Helpers;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace Manager.Integration.Test.Timers
{
    public class CheckJobHistoryStatusTimer : Timer
    {
        private CancellationTokenSource CancellationTokenSource { get; set; }

        private static readonly ILog Logger = LogManager.GetLogger(typeof (CheckJobHistoryStatusTimer));

        public EventHandler<GuidStatusChangedEventArgs> GuidStatusChangedEvent;

        public EventHandler<GuidAddedEventArgs> GuidAddedEventHandler;

        private int NumberOfGuidsToWatch { get; set; }
        public string[] StatusToLookFor { get; private set; }
        public ManualResetEventSlim ManualResetEventSlim { get; private set; }

        public ConcurrentDictionary<Guid, string> Guids { get; private set; }

        public ManagerUriBuilder ManagerUriBuilder { get; private set; }

        public void AddOrUpdateGuidStatus(Guid key,
                                          string value)
        {
            bool guidAlreadyExists = Guids.ContainsKey(key);

            string oldStatus;
            Guids.TryGetValue(key,
                              out oldStatus);

            var newstatus = Guids.AddOrUpdate(key,
                                              value,
                                              (guid,
                                               s) => value);


            if (!guidAlreadyExists)
            {
                InvokeGuidAddedEventHandler(key);
            }

            InvokeGuidStatusChangedEvent(key,
                                         oldStatus,
                                         newstatus);
        }

        private void InvokeGuidAddedEventHandler(Guid guid)
        {
            if (GuidAddedEventHandler != null)
            {
                GuidAddedEventHandler(this,
                                      new GuidAddedEventArgs(guid));
            }
        }

        private void InvokeGuidStatusChangedEvent(Guid guid,
                                                  string oldStatus,
                                                  string newStatus)
        {
            if (GuidStatusChangedEvent != null)
            {
                if (oldStatus != newStatus)
                {
                    GuidStatusChangedEvent(this,
                                           new GuidStatusChangedEventArgs(guid,
                                                                          oldStatus,
                                                                          newStatus));
                }
            }
        }

        private static void DefineDefaultRequestHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<HttpResponseMessage> GetJobHistory(Guid guid)
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                return null;
            }

            using (var client = new HttpClient())
            {
                DefineDefaultRequestHeaders(client);

                var response = await client.GetAsync(ManagerUriBuilder.GetJobHistoryUri(guid),
                                                     CancellationTokenSource.Token);

                return response;
            }
        }

        protected override void Dispose(bool disposing)
        {
            CancelAllRequest();

            base.Dispose(disposing);            
        }

        public void CancelAllRequest()
        {
            if (CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }
        }

        public CheckJobHistoryStatusTimer(int numberOfGuidsToWatch,
                                          double delay,
                                          CancellationTokenSource cancellationTokenSource,
                                          params string[] statusToLookFor) : base(delay)
        {
            if (numberOfGuidsToWatch <= 0)
            {
                throw new ArgumentException();
            }

            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException("cancellationTokenSource");
            }

            ManagerUriBuilder = new ManagerUriBuilder();

            NumberOfGuidsToWatch = numberOfGuidsToWatch;
            CancellationTokenSource = cancellationTokenSource;
            StatusToLookFor = statusToLookFor;

            ManualResetEventSlim = new ManualResetEventSlim();

            Guids = new ConcurrentDictionary<Guid, string>();

            Elapsed += CheckRequestStatusTimer_Elapsed;

            AutoReset = true;
        }

        private async void CheckRequestStatusTimer_Elapsed(object sender,
                                                           ElapsedEventArgs e)
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            Enabled = false;

            try
            {
                if (Guids.Count == NumberOfGuidsToWatch)
                {
                    ConcurrentDictionary<Guid, string> correctStatusConcurrentDictionary = new ConcurrentDictionary<Guid, string>();

                    foreach (var status in StatusToLookFor)
                    {
                        var dictionaryStatus =
                            Guids.Where(pair => !string.IsNullOrEmpty(pair.Value) &&
                                                pair.Value.Equals(status,
                                                                  StringComparison.InvariantCultureIgnoreCase))
                                .ToDictionary(pair => pair.Key,
                                              pair => pair.Value);

                        foreach (var st in dictionaryStatus)
                        {
                            var addOrUpdate = correctStatusConcurrentDictionary.AddOrUpdate(st.Key,
                                                                                            st.Value,
                                                                                            (guid,
                                                                                             s) => st.Value);
                        }
                    }


                    if (correctStatusConcurrentDictionary.Count == NumberOfGuidsToWatch)
                    {
                        Stop();

                        ManualResetEventSlim.Set();

                        return;
                    }

                    IEnumerable<Guid> inCorrectStatusConcurrentDictionary = Guids.Keys.Except(correctStatusConcurrentDictionary.Keys);

                    foreach (var guid in inCorrectStatusConcurrentDictionary)
                    {
                        var response = await GetJobHistory(guid);

                        if (response.IsSuccessStatusCode)
                        {
                            string jobSerialized = await response.Content.ReadAsStringAsync();

                            try
                            {
                                var jobHistory =
                                    JsonConvert.DeserializeObject<JobHistory>(jobSerialized);

                                AddOrUpdateGuidStatus(guid,
                                                      jobHistory.Result);
                            }
                            catch (Exception exp)
                            {
                                LogHelper.LogErrorWithLineNumber(exp.Message,
                                                                 Logger,
                                                                 exp);
                            }
                        }
                    }
                }
            }

            finally
            {
                Enabled = true;
            }
        }
    }
}