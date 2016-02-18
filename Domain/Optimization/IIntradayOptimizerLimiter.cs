using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizerLimiter
	{
		Percent MinPercentOfGroupLimit { get; }
		int MinSizeLimit { get; }
	}
}