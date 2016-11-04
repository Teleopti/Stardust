namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public static class DoubleExtensions
	{
		public static bool IsPositive(this double value)
		{
			return value > 0.01;
		}
	}
}