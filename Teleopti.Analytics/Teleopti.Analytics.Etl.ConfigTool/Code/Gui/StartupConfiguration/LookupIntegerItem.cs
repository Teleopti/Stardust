namespace Teleopti.Analytics.Etl.ConfigTool.Code.Gui.StartupConfiguration
{
	public class LookupIntegerItem
	{
		public LookupIntegerItem(int id, string text)
		{
			Id = id;
			Text = text;
		}

		public int Id { get; private set; }
		public string Text { get; private set; }
	}
}