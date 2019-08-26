using System;
using System.Windows;
using System.Windows.Input;
using Fiddler;

namespace Manager.Integration.Test.WPF.Commands
{
	public class UnInstallCertificateCommand : ICommand
	{
		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			UninstallCertificate();
		}

		public event EventHandler CanExecuteChanged;

		private static bool UninstallCertificate()
		{
			if (CertMaker.rootCertExists())
			{
				if (!CertMaker.removeFiddlerGeneratedCerts(true))
				{
					return false;
				}
			}
			else
			{
				MessageBox.Show("Certificate does not exists.",
				                "Uninstall certificate",
				                MessageBoxButton.OK,
				                MessageBoxImage.Information);
			}

			return true;
		}

		protected virtual void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
                Application.Current.Dispatcher.BeginInvoke(new Action(() => handler.Invoke(this, EventArgs.Empty)));
        }
	}
}