using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.Integration.Test.EventArgs;
using Manager.Integration.Test.Helpers;
using Newtonsoft.Json;

namespace Manager.Integration.Test.Timers
{
	public class CheckJobHistoryStatusTimer : IDisposable
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (CheckJobHistoryStatusTimer));

		public EventHandler<GuidAddedEventArgs> GuidAddedEventHandler;

		public EventHandler<GuidStatusChangedEventArgs> GuidStatusChangedEvent;

		public CheckJobHistoryStatusTimer(int numberOfGuidsToWatch,
		                                  params string[] statusToLookFor)
		{
			if (numberOfGuidsToWatch <= 0)
			{
				throw new ArgumentException();
			}

			ManagerUriBuilder = new ManagerUriBuilder();

			NumberOfGuidsToWatch = numberOfGuidsToWatch;

			StatusToLookFor = statusToLookFor;

			ManualResetEventSlim = new ManualResetEventSlim();

			Guids = new ConcurrentDictionary<Guid, string>();

			CancellationTokenSource = new CancellationTokenSource();

			MyTask = StartTask(CancellationTokenSource);
		}

		private CancellationTokenSource CancellationTokenSource { get; set; }

		private int NumberOfGuidsToWatch { get; set; }
		public string[] StatusToLookFor { get; set; }
		public ManualResetEventSlim ManualResetEventSlim { get; set; }

		public ConcurrentDictionary<Guid, string> Guids { get; set; }

		public ManagerUriBuilder ManagerUriBuilder { get; set; }

		public Task MyTask { get; set; }

		public void Dispose()
		{
			try
			{
				if (CancellationTokenSource != null &&
				    !CancellationTokenSource.IsCancellationRequested)
				{
					CancellationTokenSource.Cancel();
				}
			}

			catch (OperationCanceledException)
			{
			}

			if (MyTask != null)
			{
				MyTask.Dispose();
			}
		}

		public void AddOrUpdateGuidStatus(Guid key,
		                                  string value)
		{
			var guidAlreadyExists = Guids.ContainsKey(key);

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

		public void CancelAllRequest()
		{
			if (CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}
		}

		private Task StartTask(CancellationTokenSource cancellationTokenSource)
		{
			return Task.Factory.StartNew(async () =>
			{
				while (!cancellationTokenSource.IsCancellationRequested)
				{
					if (CancellationTokenSource.IsCancellationRequested)
					{
						CancellationTokenSource.Token.ThrowIfCancellationRequested();
					}

					if (Guids.Count == NumberOfGuidsToWatch)
					{
						var correctStatusConcurrentDictionary = new ConcurrentDictionary<Guid, string>();

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
							ManualResetEventSlim.Set();

							return;
						}


						var inCorrectStatusConcurrentDictionary = Guids.Keys.Except(correctStatusConcurrentDictionary.Keys);


						foreach (var guid in inCorrectStatusConcurrentDictionary)
						{
							var response = await GetJobHistory(guid);

							if (response.IsSuccessStatusCode)
							{
								var jobSerialized = await response.Content.ReadAsStringAsync();

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

					Thread.Sleep(TimeSpan.FromSeconds(10));
				}
			}, cancellationTokenSource.Token);
		}
	}
}