using System;
using System.Windows;

namespace Manager.Integration.Test.WPF
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
	    public App()
	    {
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
	    }

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exp =
				e.ExceptionObject as Exception;

			if (exp != null)
			{
				MessageBox.Show(exp.StackTrace, 
					"UnhandledException", 
					MessageBoxButton.OK, 
					MessageBoxImage.Error);
			}
		}
    }
}