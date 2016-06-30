using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelValidator
	{
		void Validate(DateTime start,DateTime end,Action<ReadModelValidationResult> reportProgress,
			bool ignoreValid = false);

		void SetTargetTypes(IList<ValidateReadModelType> types);

		bool ValidateReadModelPersonScheduleDay(IPerson person,DateOnly date,IScheduleDay scheduleDay);
		PersonScheduleDayReadModel FetchReadModelPersonScheduleDay(IPerson person,DateOnly date);
		PersonScheduleDayReadModel BuildReadModelPersonScheduleDay(IPerson person,IScheduleDay scheduleDay);
		PersonScheduleDayReadModel BuildReadModelPersonScheduleDay(Guid personId, DateOnly date);


		bool ValidateReadModelScheduleDay(IPerson person,DateOnly date,IScheduleDay scheduleDay);
		ScheduleDayReadModel FetchReadModelScheduleDay(IPerson person,DateOnly date);
		ScheduleDayReadModel BuildReadModelScheduleDay(IPerson person,IScheduleDay scheduleDay);
		ScheduleDayReadModel BuildReadModelScheduleDay(Guid personId, DateOnly date);

		bool ValidateReadModelScheduleProjectionReadOnly(IPerson person,DateOnly date,IScheduleDay scheduleDay);

		IEnumerable<ScheduleProjectionReadOnlyModel> FetchReadModelScheduleProjectionReadOnly(IPerson person,
			DateOnly date);

		IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModelScheduleProjectionReadOnly(Guid personId,DateOnly date);
		IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModelScheduleProjectionReadOnly(IPerson person,DateOnly date);
		IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModelScheduleProjectionReadOnly(IPerson person,IScheduleDay scheduleDay);

	}
}