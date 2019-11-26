using System;
using System.Windows;
using System.Windows.Input;
using Fiddler;

namespace Manager.Integration.Test.WPF.Commands
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
			{
				handler(this, System.EventArgs.Empty);
			}
		}
	}
}