using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Models;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Interfaces;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using Newtonsoft.Json;

namespace Manager.Integration.Test.Helpers
{
	public class JobManagerTaskCreator : IDisposable
	{
		public JobManagerTaskCreator(CheckJobStatusTimer checkJobStatusTimer)
		{
			CancellationTokenSource = new CancellationTokenSource();

			ManagerUriBuilder = new ManagerUriBuilder();

			CheckJobStatusTimer = checkJobStatusTimer;
		}

		public ManagerUriBuilder ManagerUriBuilder { get; set; }

		private CheckJobStatusTimer CheckJobStatusTimer { get; set; }

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

		public void CreateNewJobToManagerTask(JobQueueItem jobQueue)
		{
			NewJobToManagerTask = new Task(() => { CreateNewJobToManager(jobQueue); },
			                               CancellationTokenSource.Token);
		}

		public async void CreateNewJobToManager(JobQueueItem jobQueue)
		{
			CreateNewJobToManagerSucceeded = false;

			IHttpSender httpSender = new HttpSender();

			var uri = ManagerUriBuilder.GetAddToJobQueueUri();

			try
			{
				HttpResponseMessage response = null;

				while (!CreateNewJobToManagerSucceeded)
				{
					this.Log().DebugWithLineNumber(
						"Start calling post async. Uri ( " + uri + " ). Job name : ( " + jobQueue.Name + " )");
					try
					{
						response = await httpSender.PostAsync(uri, jobQueue);

						CreateNewJobToManagerSucceeded = response.IsSuccessStatusCode;
					}

					catch
					{
						CreateNewJobToManagerSucceeded = false;

						this.Log().WarningWithLineNumber(
							"HttpRequestException when calling post async, will soon try again. Uri ( " + uri + " ). Job name : ( " +
							jobQueue.Name + " ).");

						Thread.Sleep(TimeSpan.FromSeconds(1));
					}
				}

				if (CreateNewJobToManagerSucceeded)
				{
					if (response != null)
					{
						var str = await response.Content.ReadAsStringAsync();

						var jobId = JsonConvert.DeserializeObject<Guid>(str);

						CheckJobStatusTimer.AddOrUpdateGuidStatus(jobId,
						                                          null);

						this.Log().DebugWithLineNumber(
							"Finished calling post async. Uri : ( " + uri + " ). ( jobId, jobName ) : ( " + jobId + ", " +
							jobQueue.Name + " )");
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message,
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
					this.Log().DebugWithLineNumber("Start calling delete async. Uri ( " + uri + " ). Job id : ( " + guid + " )");

					var response = await httpSender.DeleteAsync(uri);

					DeleteJobToManagerSucceeded = response.IsSuccessStatusCode;

					if (DeleteJobToManagerSucceeded)
					{
						await response.Content.ReadAsStringAsync();
					}

					this.Log().DebugWithLineNumber("Finished calling delete async. Uri ( " + uri + " ). Job id : ( " + guid + " )");
				}

				catch (HttpRequestException)
				{
					this.Log().WarningWithLineNumber(
						"HttpRequestException when calling delete async, will soon try again. Uri ( " + uri + " ). Job id : ( " + guid +
						" )");
				}

				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message,
					                               exp);
				}
			},
			                                  CancellationTokenSource.Token);
		}
	}
}