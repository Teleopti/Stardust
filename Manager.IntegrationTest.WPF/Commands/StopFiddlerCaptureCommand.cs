using System;
using System.Windows;
using System.Windows.Input;
using Manager.IntegrationTest.WPF.HttpListeners.Fiddler;

namespace Manager.IntegrationTest.WPF.Commands
{
	public class StopFiddlerCaptureCommand : ICommand
	{
		public StopFiddlerCaptureCommand(FiddlerCapture fiddlerCapture)
		{
			if (fiddlerCapture == null)
			{
				throw new ArgumentNullException("fiddlerCapture");
			}

			FiddlerCapture = fiddlerCapture;

			FiddlerCapture.PropertyChanged += FiddlerCapture_PropertyChanged;
		}

		private void FiddlerCapture_PropertyChanged(object sender, 
													System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("IsStarted", StringComparison.InvariantCultureIgnoreCase))
			{
				OnCanExecuteChanged();
			}
		}

		public FiddlerCapture FiddlerCapture { get; private set; }

		public bool CanExecute(object parameter)
		{
			return FiddlerCapture.IsStarted;
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
                Application.Current.Dispatcher.BeginInvoke(new Action(() => handler.Invoke(this, EventArgs.Empty)));
        }
	}
}