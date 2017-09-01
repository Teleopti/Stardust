namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class LimitForNoResourceCalculation : ILimitForNoResourceCalculation
	{
		public int NumberOfAgents { get; private set; } = 100;

		public void SetFromTest(int limit)
		{
			NumberOfAgents = limit;
		}
	}
}