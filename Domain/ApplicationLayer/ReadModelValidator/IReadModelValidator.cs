using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelValidator
	{

		IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModel(IPerson person, DateOnly date);

		void Validate(DateTime start, DateTime end, Action<ScheduleProjectionReadOnlyValidationResult> reportProgress, bool ignoreValid = false);

	}
}