using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public interface IExternalPerformanceDataReadModelRepository
	{
		void Add(ExternalPerformanceData model);
	}
}