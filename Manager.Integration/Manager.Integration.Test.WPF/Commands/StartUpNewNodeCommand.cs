using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Manager.Integration.Test.Helpers;
using Manager.IntegrationTest.Console.Host.Helpers;

namespace Manager.Integration.Test.WPF.Commands
{
	public class StartUpNewNodeCommand : ICommand
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

				var nodeName = IntegrationControllerApiHelper.StartNewNode(httpSender);
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