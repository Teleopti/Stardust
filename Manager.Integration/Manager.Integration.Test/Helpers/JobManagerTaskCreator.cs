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

		public JobManagerTaskCreator(CheckJobHistoryStatusTimer checkJobHistoryStatusTimer)
		{
			CancellationTokenSource = new CancellationTokenSource();

			ManagerUriBuilder = new ManagerUriBuilder();

			CheckJobHistoryStatusTimer = checkJobHistoryStatusTimer;
		}

		public ManagerUriBuilder ManagerUriBuilder { get; }

		private CheckJobHistoryStatusTimer CheckJobHistoryStatusTimer { get; }

		public bool CreateNewJobToManagerSucceeded { get; set; }

		public bool DeleteJobToManagerSucceeded { get; set; }

		private Task NewJobToManagerTask { get; set; }

		private CancellationTokenSource CancellationTokenSource { get; }

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

		private void DefineDefaultRequestHeaders(HttpClient client)
		{
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeConstants.ApplicationJson));
		}

		public void StartDeleteJobToManagerTask()
		{
			DeleteJobToManagerTask.Start();
		}

		public void StartAndWaitDeleteJobToManagerTask(TimeSpan timeout)
		{
			DeleteJobToManagerTask.Start();

			DeleteJobToManagerTask.Wait(timeout);
		}

		public void StartCreateNewJobToManagerTask()
		{
			NewJobToManagerTask.Start();
		}

		public void StartAndWaitCreateNewJobToManagerTask(TimeSpan timeout)
		{
			NewJobToManagerTask.Start();

			NewJobToManagerTask.Wait(timeout);
		}

		public void CreateNewJobToManagerTask(JobRequestModel jobRequestModel)
		{
			NewJobToManagerTask = new Task(async () =>
			{
				CreateNewJobToManagerSucceeded = false;

				using (var client = new HttpClient())
				{
					DefineDefaultRequestHeaders(client);

					var sez = JsonConvert.SerializeObject(jobRequestModel);

					var uri = ManagerUriBuilder.GetStartJobUri();

					try
					{
						LogHelper.LogDebugWithLineNumber(
							"Start calling post async. Uri ( " + uri + " ). Job name : ( " + jobRequestModel.Name + " )",
							Logger);

						var response = await client.PostAsync(uri,
						                                      new StringContent(sez,
						                                                        Encoding.UTF8,
						                                                        MediaTypeConstants.ApplicationJson));

						CreateNewJobToManagerSucceeded = response.IsSuccessStatusCode;

						if (CreateNewJobToManagerSucceeded)
						{
							var str = await response.Content.ReadAsStringAsync();

							var jobId = JsonConvert.DeserializeObject<Guid>(str);

							CheckJobHistoryStatusTimer.AddOrUpdateGuidStatus(jobId,
							                                                 null);

							LogHelper.LogDebugWithLineNumber(
								"Finished calling post async. Uri : ( " + uri + " ). ( jobId, jobName ) : ( " + jobId + ", " +
								jobRequestModel.Name + " )",
								Logger);
						}
					}

					catch (HttpRequestException)
					{
						LogHelper.LogWarningWithLineNumber(
							"HttpRequestException when calling post async, will soon try again. Uri ( " + uri + " ). Job name : ( " +
							jobRequestModel.Name + " ).",
							Logger);
					}

					catch (Exception exp)
					{
						LogHelper.LogErrorWithLineNumber(exp.Message,
						                                 Logger,
						                                 exp);
					}
				}
			},
			CancellationTokenSource.Token);
		}

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
						LogHelper.LogDebugWithLineNumber("Start calling delete async. Uri ( " + uri + " ). Job id : ( " + guid + " )",
						                                 Logger);

						var response = await client.DeleteAsync(uri);

						DeleteJobToManagerSucceeded = response.IsSuccessStatusCode;

						if (DeleteJobToManagerSucceeded)
						{
							await response.Content.ReadAsStringAsync();
						}

						LogHelper.LogDebugWithLineNumber("Finished calling delete async. Uri ( " + uri + " ). Job id : ( " + guid + " )",
						                                 Logger);
					}

					catch (HttpRequestException)
					{
						LogHelper.LogWarningWithLineNumber(
							"HttpRequestException when calling delete async, will soon try again. Uri ( " + uri + " ). Job id : ( " + guid +
							" )",
							Logger);
					}

					catch (Exception exp)
					{
						LogHelper.LogErrorWithLineNumber(exp.Message,
						                                 Logger,
						                                 exp);
					}
				}
			},
			CancellationTokenSource.Token);
		}
	}
}