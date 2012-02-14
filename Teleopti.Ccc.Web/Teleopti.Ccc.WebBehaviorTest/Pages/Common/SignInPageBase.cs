using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public abstract class SignInPageBase : Page
	{
		public bool SingleBusinessUnit { get; set; }
		public bool  HasPermission { get; set; }
		public abstract string ErrorMessage { get; }
		public abstract void SelectFirstApplicationDataSource();
		public abstract void ClickApplicationOkButton();
		public abstract void SignInApplication(string username, string password);
		protected abstract void WaitUntilSignInOrBusinessUnitListOrErrorAppears();
		public abstract void SelectFirstBusinessUnit();
		public abstract void ClickBusinessUnitOkButton();

	}
}