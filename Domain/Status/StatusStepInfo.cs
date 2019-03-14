namespace Teleopti.Ccc.Domain.Status
{
	public class StatusStepInfo
	{
		public StatusStepInfo(string name, string description, string statusUrl)
		{
			Name = name;
			Description = description;
			StatusUrl = statusUrl;
		}
		
		public string Name { get;  }
		public string Description { get; }
		public string StatusUrl { get; }
	}
}