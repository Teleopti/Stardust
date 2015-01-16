namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance
{
	public interface IPerformanceCounter
	{
		bool IsEnabled { get; }
		int Limit { get; set; }
		void Count();
	}
}