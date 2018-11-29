using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelScheduleProjectionReadOnlyValidator
	{
		bool Validate(IPerson person,IScheduleDay scheduleDay,ReadModelValidationMode mode);

		IEnumerable<ScheduleProjectionReadOnlyModel> FetchFromRepository(IPerson person,
			DateOnly date);

		IEnumerable<ScheduleProjectionReadOnlyModel> Build(Guid personId,DateOnly date);
		IEnumerable<ScheduleProjectionReadOnlyModel> Build(IPerson person,DateOnly date);
		IEnumerable<ScheduleProjectionReadOnlyModel> Build(IPerson person,IScheduleDay scheduleDay);
		bool IsInitialized();
	}
}