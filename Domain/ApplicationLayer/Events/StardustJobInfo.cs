namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class StardustJobInfo : EventWithInfrastructureContext, IStardustJobInfo
	{
		public string JobName { get; set; }
		public string UserName { get; set; }
		public string Policy { get; set; }
	}
}