namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public interface IExternalPerformancePersister
	{
		void Persist(ExternalPerformanceInfoProcessResult result);
	}
}
