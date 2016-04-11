using System;
using System.Windows.Input;

namespace Manager.Integration.Test.WPF.Commands
{
	public class StartUpNewNodeCommand : ICommand
	{
		public bool CanExecute(object parameter)
		{
			throw new NotImplementedException();
		}

		public void Execute(object parameter)
		{
			throw new NotImplementedException();
		}

		public event EventHandler CanExecuteChanged;
	}
}