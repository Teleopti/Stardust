using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Interfaces;
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

		public ManagerUriBuilder ManagerUriBuilder { get; set; }

		private CheckJobHistoryStatusTimer CheckJobHistoryStatusTimer { get; set; }

		public bool CreateNewJobToManagerSucceeded { get; set; }

		public bool DeleteJobToManagerSucceeded { get; set; }

		private Task NewJobToManagerTask { get; set; }

		private CancellationTokenSource CancellationTokenSource { get; set; }

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
			NewJobToManagerTask = new Task(() => { CreateNewJobToManager(jobRequestModel); },
			                               CancellationTokenSource.Token);
		}

		public async void CreateNewJobToManager(JobRequestModel jobRequestModel)
		{
			CreateNewJobToManagerSucceeded = false;

			IHttpSender httpSender = new HttpSender();

			var uri = ManagerUriBuilder.GetStartJobUri();

			try
			{
				HttpResponseMessage response = null;
				while (!CreateNewJobToManagerSucceeded)
				{
					LogHelper.LogDebugWithLineNumber(
					"Start calling post async. Uri ( " + uri + " ). Job name : ( " + jobRequestModel.Name + " )",
					Logger);
					try
					{
						response = await httpSender.PostAsync(uri, jobRequestModel);
						CreateNewJobToManagerSucceeded = response.IsSuccessStatusCode;
					}
					catch
					{
						CreateNewJobToManagerSucceeded = false;
						LogHelper.LogWarningWithLineNumber(
					"HttpRequestException when calling post async, will soon try again. Uri ( " + uri + " ). Job name : ( " +
					jobRequestModel.Name + " ).",
					Logger);
						Thread.Sleep(TimeSpan.FromSeconds(1));
					}
				}

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
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(exp.Message,
				                                 Logger,
				                                 exp);
			}
		}

		public void CreateDeleteJobToManagerTask(Guid guid)
		{
			DeleteJobToManagerTask = new Task(async () =>
			{
				DeleteJobToManagerSucceeded = false;

				IHttpSender httpSender = new HttpSender();

				var uri = ManagerUriBuilder.GetCancelJobUri(guid);

				try
				{
					LogHelper.LogDebugWithLineNumber("Start calling delete async. Uri ( " + uri + " ). Job id : ( " + guid + " )",
					                                 Logger);

					var response = await httpSender.DeleteAsync(uri);

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
			},
			                                  CancellationTokenSource.Token);
		}
	}
}