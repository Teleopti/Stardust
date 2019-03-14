namespace Teleopti.Ccc.Domain.Status
{
	public class StatusStepInfo
	{
		public StatusStepInfo(int id, string name, string description, string statusUrl, string pingUrl)
		{
			Id = id;
			Name = name;
			Description = description;
			StatusUrl = statusUrl;
			PingUrl = pingUrl;
		}
		
		public string Name { get;  }
		public string Description { get; }
		public string StatusUrl { get; }
		public string PingUrl { get; }
		public int Id { get; }
	}
}