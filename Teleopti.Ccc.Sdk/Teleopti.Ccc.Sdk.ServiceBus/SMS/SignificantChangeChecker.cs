using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.SMS
{
	public interface ISignificantChangeChecker
	{
		bool IsSignificantChange(DateOnlyPeriod dateOnlyPeriod, IPerson person);
	}

	public class SignificantChangeChecker : ISignificantChangeChecker
	{
		public bool IsSignificantChange(DateOnlyPeriod dateOnlyPeriod, IPerson person)
		{
			return true;
		}
	}
}