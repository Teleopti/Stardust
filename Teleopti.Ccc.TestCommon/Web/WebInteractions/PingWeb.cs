namespace Teleopti.Ccc.TestCommon.Web.WebInteractions
{
	public static class PingWeb
	{
		public static void Execute()
		{
			using (var http = new Http())
			{
				http.Get("ToggleHandler/AllToggles");
			}
		}
	}
}