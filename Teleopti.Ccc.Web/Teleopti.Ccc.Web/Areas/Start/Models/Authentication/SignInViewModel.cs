using System;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class SignInViewModel
	{
		public SignInViewModel()
		{
			ApplicationSignIn = new SignInApplicationViewModel();
			WindowsSignIn = new SignInWindowsViewModel();
		}

		public SignInApplicationViewModel ApplicationSignIn { get; set; }
		public SignInWindowsViewModel WindowsSignIn { get; set; }

		public Boolean HasApplicationSignIn
		{
			get
			{
				return ApplicationSignIn.HasDataSource;
			}
		}

		public Boolean HasWindowsSignIn
		{
			get
			{
				return WindowsSignIn.HasDataSource;
			}
		}


	}
}