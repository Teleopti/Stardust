using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public interface IValidatePeriod
	{
		double RelativeDifference { get; }
		DateTimePeriod DateTimePeriod { get; }
	}
}