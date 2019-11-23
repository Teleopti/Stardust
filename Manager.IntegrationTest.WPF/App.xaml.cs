using System;
using System.Net;
using System.Windows;

namespace Manager.IntegrationTest.WPF
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
	    public App()
	    {
			if (ServicePointManager.DefaultConnectionLimit < 10)
			{
				ServicePointManager.DefaultConnectionLimit = 200;
			}

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			//DbInterception.Add(new NoLockInterceptor());
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