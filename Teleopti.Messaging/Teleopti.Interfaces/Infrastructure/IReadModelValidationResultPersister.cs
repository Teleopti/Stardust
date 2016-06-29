using System.Collections.Generic;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IReadModelValidationResultPersister
	{
		void SaveScheduleProjectionReadOnly(ReadModelValidationResult input);
		void SavePersonScheduleDay(ReadModelValidationResult input);
		IEnumerable<ReadModelValidationResult> LoadAllInvalid();
		void Reset();
	}
}