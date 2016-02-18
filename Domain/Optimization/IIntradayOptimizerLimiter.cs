using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizerLimiter
	{
		Percent SizeOfGroupLimit { get; }
		int MinSizeLimit { get; }
	}
}