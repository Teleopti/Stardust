using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Manager.Integration.Test.Helpers;
using Manager.IntegrationTest.Console.Host.Helpers;

namespace Manager.Integration.Test.WPF.Commands
{
	public class StartUpNewManagerCommand : ICommand
	{
		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			Task.Factory.StartNew(() =>
			{
				var httpSender = new HttpSender();

				var managerName = IntegrationControllerApiHelper.StartNewManager(httpSender);

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