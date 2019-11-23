using System;
using System.Windows;
using System.Windows.Input;
using Fiddler;

namespace Manager.IntegrationTest.WPF.Commands
{
	public class InstallCertificateCommand : ICommand
	{
		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			InstallCertificate();
		}

		public event EventHandler CanExecuteChanged;

		private static bool InstallCertificate()
		{
			if (!CertMaker.rootCertExists())
			{
				if (!CertMaker.createRootCert())
				{
					return false;
				}

				if (!CertMaker.trustRootCert())
				{
					return false;
				}
			}
			else
			{
				MessageBox.Show("Certificate already installed.",
				                "Install certificate",
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