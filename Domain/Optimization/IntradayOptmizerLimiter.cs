using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptmizerLimiter : IIntradayOptimizerLimiter
	{
		private Percent _minPercentOfGroupLimit = new Percent(0.5);
		private int _minSizeLimit = 100;

		public Percent MinPercentOfGroupLimit
		{
			get { return _minPercentOfGroupLimit; }
		}

		public int MinSizeLimit
		{
			get { return _minSizeLimit; }
		}

		public void SetFromTest(Percent sizeOfGroupLimit, int minSizeLimit)
		{
			_minPercentOfGroupLimit = sizeOfGroupLimit;
			_minSizeLimit = minSizeLimit;
		}
	}
}