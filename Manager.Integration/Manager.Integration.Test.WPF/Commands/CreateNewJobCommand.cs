using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Models;
using Manager.Integration.Test.Params;
using Manager.IntegrationTest.Console.Host.Helpers;
using Newtonsoft.Json;

namespace Manager.Integration.Test.WPF.Commands
{
	public class CreateNewJobCommand : ICommand
	{
		public CreateNewJobCommand(int numberOfJobs = 1)
		{
			NumberOfJobs = numberOfJobs;

			HttpSender = new HttpSender();
			ManagerUriBuilder = new ManagerUriBuilder();
		}

		public int NumberOfJobs { get; private set; }

		private HttpSender HttpSender { get; set; }

		public ManagerUriBuilder ManagerUriBuilder { get; set; }

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			Task.Factory.StartNew(() =>
			{
				var uri = ManagerUriBuilder.GetAddToJobQueueUri();

				for (var i = 0; i < NumberOfJobs; i++)
				{
					var testJobParams = new TestJobParams("Test job Data " + i, 10);
					var testJobParamsToJson = JsonConvert.SerializeObject(testJobParams);

					var job = new JobQueueItem
					{
						Name = "Job Name " + i,
						Serialized = testJobParamsToJson,
						Type = "NodeTest.JobHandlers.TestJobParams",
						CreatedBy = "test"
					};

					var response = HttpSender.PostAsync(uri, job);
				}
			});
		}

		public event EventHandler CanExecuteChanged;

		protected virtual void OnCanExecuteChanged()
		{
			var handler = CanExecuteChanged;

			if (handler != null)
			{
				handler(this, System.EventArgs.Empty);
			}
		}
	}
}