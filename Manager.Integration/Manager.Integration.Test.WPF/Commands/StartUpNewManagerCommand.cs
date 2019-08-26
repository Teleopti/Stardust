﻿using System;
using System.Threading.Tasks;
using System.Windows;
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
			Task.Run(() =>
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
                Application.Current.Dispatcher.BeginInvoke(new Action(() => handler.Invoke(this, EventArgs.Empty)));
        }
	}
}