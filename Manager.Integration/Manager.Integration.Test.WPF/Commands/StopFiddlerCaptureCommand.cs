using System;
using System.Windows.Input;
using Manager.Integration.Test.WPF.HttpListeners.Fiddler;

namespace Manager.Integration.Test.WPF.Commands
{
	public class StopFiddlerCaptureCommand : ICommand
	{
		public StopFiddlerCaptureCommand(FiddlerCapture fiddlerCapture)
		{
			FiddlerCapture = fiddlerCapture;

			if (fiddlerCapture == null)
			{
				throw new ArgumentNullException("fiddlerCapture");
			}
		}

		public FiddlerCapture FiddlerCapture { get; private set; }

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			FiddlerCapture.Stop();
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