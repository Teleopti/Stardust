using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	public interface IValidatePeriod
	{
		double RelativeDifference { get; }
		DateTimePeriod DateTimePeriod { get; }
	}
}