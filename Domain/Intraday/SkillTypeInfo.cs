namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillTypeInfo
	{
		public SkillTypeInfo(bool supportsAbandonRate, bool supportsReforecastedAgents)
		{
			SupportsAbandonRate = supportsAbandonRate;
			SupportsReforecastedAgents = supportsReforecastedAgents;
		}

		public bool SupportsAbandonRate { get; }
		public bool SupportsReforecastedAgents { get; }
	}
}