namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public interface ILineExtractor
	{
		PerformanceInfoExtractionResult ExtractAndValidate(string line);
	}
}