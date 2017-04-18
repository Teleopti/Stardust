namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main
{
	public class WebUrlHolder
	{
		private readonly string _webUrl;

		public WebUrlHolder(string webUrl)
		{
			_webUrl = webUrl;
		}

		public string WebUrl
		{
			get { return _webUrl; }
		}
	}
}
