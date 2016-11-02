namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public interface IInitMaxSeatForStateHolder
	{
		void Execute(int intervalLength);
	}
}