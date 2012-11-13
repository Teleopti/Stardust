namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface ISignInNewPage
	{
		void SelectTestDataApplicationLogon();
		void SignInApplication(string userName, string password);
		void TrySignInApplication(string userName, string password);
	}
}