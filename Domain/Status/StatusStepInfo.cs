namespace Teleopti.Ccc.Domain.Status
{
	public class StatusStepInfo
	{
		public StatusStepInfo(string name, string description, string url)
		{
			Name = name;
			Description = description;
			Url = url;
		}
		
		public string Name { get;  }
		public string Description { get; }
		public string Url { get; }
	}
}