namespace Teleopti.Ccc.Domain.Status
{
	public class StatusStepInfo
	{
		public StatusStepInfo(string name, string url)
		{
			Name = name;
			Url = url;
		}
		
		public string Name { get;  }
		public string Url { get; }
	}
}