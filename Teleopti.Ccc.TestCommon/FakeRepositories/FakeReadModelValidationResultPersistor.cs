using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{

	public class FakeReadModelValidationResultPersistor:IReadModelValidationResultPersister
	{
		private readonly IList<ReadModelValidationResult> _invalidResults = new List<ReadModelValidationResult>();

		public void SaveScheduleProjectionReadOnly(ReadModelValidationResult input)
		{
			_invalidResults.Add(input);
		}

		public void SavePersonScheduleDay(ReadModelValidationResult input)
		{
			_invalidResults.Add(input);
		}

		public void SaveScheduleDay(ReadModelValidationResult input)
		{
			_invalidResults.Add(input);
		}

		public IEnumerable<ReadModelValidationResult> LoadAllInvalidScheduleProjectionReadOnly()
		{
			return _invalidResults;
		}

		public IEnumerable<ReadModelValidationResult> LoadAllInvalidPersonScheduleDay()
		{
			return _invalidResults;
		}

		public IEnumerable<ReadModelValidationResult> LoadAllInvalidScheduleDay()
		{
			return _invalidResults;
		}

		public void Reset(ValidateReadModelType types)
		{
			_invalidResults.Clear();
		}
	}
}
