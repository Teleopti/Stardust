namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public interface ILineExtractorValidator
	{
		PerformanceInfoExtractionResult ExtractAndValidate(string line);
	}
}