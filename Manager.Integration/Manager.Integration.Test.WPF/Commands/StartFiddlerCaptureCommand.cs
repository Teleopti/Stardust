using System;
using System.Windows.Input;
using Manager.Integration.Test.WPF.HttpListeners.Fiddler;

namespace Manager.Integration.Test.WPF.Commands
{
	public class StartFiddlerCaptureCommand : ICommand
	{
		public StartFiddlerCaptureCommand(FiddlerCapture fiddlerCapture)
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
			return !FiddlerCapture.IsStarted;
		}

		public void Execute(object parameter)
		{
			FiddlerCapture.Start();
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