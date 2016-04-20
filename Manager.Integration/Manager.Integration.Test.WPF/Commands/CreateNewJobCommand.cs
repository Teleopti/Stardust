using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Manager.Integration.Test.Helpers;
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
				var uri = ManagerUriBuilder.GetStartJobUri();
				var username = SecurityHelper.GetLoggedInUser();

				for (var i = 0; i < NumberOfJobs; i++)
				{
					var fastJobParams = new FastJobParams("Fast job data " + i);

					var fastJobParamsToJson = JsonConvert.SerializeObject(fastJobParams);

					var job = new JobQueueItem
					{
						Name = "Job Name " + i,
						Serialized = fastJobParamsToJson,
						Type = "NodeTest.JobHandlers.FastJobParams",
						CreatedBy = username
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