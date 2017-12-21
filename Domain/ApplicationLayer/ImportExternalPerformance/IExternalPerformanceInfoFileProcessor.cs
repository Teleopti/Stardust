using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public interface IExternalPerformanceInfoFileProcessor
	{
		ExternalPerformanceInfoProcessResult Process(ImportFileData importFileData, Action<string> sendProgress);
	}
}