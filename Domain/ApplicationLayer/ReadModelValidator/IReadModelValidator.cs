using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelValidator
	{
		void SetTargetTypes(IList<ValidateReadModelType> types);

		IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModel(IPerson person, DateOnly date);
		IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModel(Guid personId, DateOnly date);

		void Validate(DateTime start, DateTime end, Action<ReadModelValidationResult> reportProgress,
			bool ignoreValid = false);

	}
}