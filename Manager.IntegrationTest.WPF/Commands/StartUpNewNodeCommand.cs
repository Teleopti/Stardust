using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Manager.IntegrationTest.Helpers;
using Manager.IntegrationTest.ConsoleHost.Helpers;

namespace Manager.IntegrationTest.WPF.Commands
{
	public class StartUpNewNodeCommand : ICommand
	{
		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			Task.Run(() =>
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
                Application.Current.Dispatcher.BeginInvoke(new Action(() => handler.Invoke(this, EventArgs.Empty)));
        }
    }
}