namespace Teleopti.Interfaces.Domain
{
	public interface IPaging
	{
		int Take  { get; set; }
		int Skip { get; set; }
		int TotalCount { get; set; }
	}
}