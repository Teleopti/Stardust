namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	using WatiN.Core;

	public class GlobalMenuPage : Page
	{
		[FindBy(Id = "global-menu-list")]
		public List GlobalMenuList;

		[FindBy(Id = "signout-button")]
		public Link SignoutButton { get; set; }
	}
}