using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	public interface IValidatePeriod
	{
		double RelativeDifference { get; }
		double RelativeDifferenceWithShrinkage { get; }
		DateTimePeriod DateTimePeriod { get; }
	}
}