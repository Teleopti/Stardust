using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IReadModelValidationResultPersister
	{
		void SaveScheduleProjectionReadOnly(ReadModelValidationResult input);
		void SavePersonScheduleDay(ReadModelValidationResult input);
		void SaveScheduleDay(ReadModelValidationResult input);
		IEnumerable<ReadModelValidationResult> LoadAllInvalidScheduleProjectionReadOnly();
		IEnumerable<ReadModelValidationResult> LoadAllInvalidPersonScheduleDay();
		IEnumerable<ReadModelValidationResult> LoadAllInvalidScheduleDay();
		void Reset(ValidateReadModelType types);
	}
}