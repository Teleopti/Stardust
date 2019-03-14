using System;

namespace Teleopti.Ccc.Domain.Status
{
	public class CustomStatusStepInfo
	{
		public CustomStatusStepInfo(int id, string name, string description, int limitInSeconds, string pingUrl)
		{
			Id = id;
			Name = name;
			Description = description;
			Limit = limitInSeconds;
			PingUrl = pingUrl;
		}

		public int Id { get; }
		public string Name { get; }
		public string Description { get; }
		public int Limit { get; }
		public string PingUrl { get; }
	}
}