using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Manager.Integration.Test.Timers;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Interfaces;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using Newtonsoft.Json;

namespace Manager.Integration.Test.Helpers
{
	public class JobManagerTaskCreator : IDisposable
	{
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
					this.Log().DebugWithLineNumber(
						"Start calling post async. Uri ( " + uri + " ). Job name : ( " + jobRequestModel.Name + " )");
					try
					{
						response = await httpSender.PostAsync(uri, jobRequestModel);
						CreateNewJobToManagerSucceeded = response.IsSuccessStatusCode;
					}
					catch
					{
						CreateNewJobToManagerSucceeded = false;

						this.Log().WarningWithLineNumber(
							"HttpRequestException when calling post async, will soon try again. Uri ( " + uri + " ). Job name : ( " +
							jobRequestModel.Name + " ).");
						Thread.Sleep(TimeSpan.FromSeconds(1));
					}
				}

				if (CreateNewJobToManagerSucceeded)
				{
					var str = await response.Content.ReadAsStringAsync();

					var jobId = JsonConvert.DeserializeObject<Guid>(str);

					CheckJobHistoryStatusTimer.AddOrUpdateGuidStatus(jobId,
					                                                 null);

					this.Log().DebugWithLineNumber(
						"Finished calling post async. Uri : ( " + uri + " ). ( jobId, jobName ) : ( " + jobId + ", " +
						jobRequestModel.Name + " )");
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