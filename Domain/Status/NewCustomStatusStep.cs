namespace Teleopti.Ccc.Domain.Status
{
	public class NewCustomStatusStep
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public int LimitInSeconds { get; set; }
	}
}