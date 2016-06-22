namespace Teleopti.Ccc.Domain.Exceptions
{
	public class SkillMissingInAnalyticsException : DataMissingInAnalyticsException
	{
		public SkillMissingInAnalyticsException() : base("Skill")
		{
		}
	}
}