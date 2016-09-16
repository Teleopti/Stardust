using System.Globalization;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsConfigurationRepository
	{
		CultureInfo GetCulture();
	}
}