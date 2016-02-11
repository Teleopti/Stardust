namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class LimitForNoResourceCalculation : ILimitForNoResourceCalculation
	{
		private int _limit = 100;

		public int NumberOfAgents
		{
			get { return _limit; }
		}

		public void SetFromTest(int limit)
		{
			_limit = limit;
		}
	}
}