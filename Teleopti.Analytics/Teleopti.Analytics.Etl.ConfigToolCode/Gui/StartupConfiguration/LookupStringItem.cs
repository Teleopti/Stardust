namespace Teleopti.Analytics.Etl.ConfigToolCode.Gui.StartupConfiguration
{
	public class LookupStringItem
	{
		public LookupStringItem(string id, string text)
		{
			Id = id;
			Text = text;
		}

		public string Id { get; private set; }
		public string Text { get; private set; }
	}
}