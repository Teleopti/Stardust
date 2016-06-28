using System.Collections.Generic;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IScheduleProjectionReadOnlyCheckResultPersister
	{
		void Save(ReadModelValidationResult input);
		IEnumerable<ReadModelValidationResult> LoadAllInvalid();
		void Reset();
	}
}