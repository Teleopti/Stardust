using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Web.Areas.HealthCheck.Core.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.HealthCheck.Core
{
	public interface IReadModelValidator
	{

		IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModel(IPerson person, DateOnly date);
		IEnumerable<ScheduleProjectionReadOnlyValidationResult> Validate(DateTime start, DateTime end);
	}
}