using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public interface IValidatePeriod
	{
		double RelativeDifference { get; }
		DateTimePeriod DateTimePeriod { get; }
	}
}