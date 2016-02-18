using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptmizerLimiter : IIntradayOptimizerLimiter
	{
		private Percent _sizeOfGroupLimit = new Percent(0.5);
		private int _minSizeLimit = 100;

		public Percent SizeOfGroupLimit
		{
			get { return _sizeOfGroupLimit; }
		}

		public int MinSizeLimit
		{
			get { return _minSizeLimit; }
		}

		public void SetFromTest(Percent sizeOfGroupLimit, int minSizeLimit)
		{
			_sizeOfGroupLimit = sizeOfGroupLimit;
			_minSizeLimit = minSizeLimit;
		}
	}
}