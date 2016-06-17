using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelValidator
	{

		IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModel(IPerson person, DateOnly date);
		IEnumerable<ScheduleProjectionReadOnlyValidationResult> Validate(DateTime start, DateTime end);
	}
}