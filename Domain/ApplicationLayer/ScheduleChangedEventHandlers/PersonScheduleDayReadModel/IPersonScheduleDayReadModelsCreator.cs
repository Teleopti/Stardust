using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IPersonScheduleDayReadModelsCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		IEnumerable<PersonScheduleDayReadModel> GetReadModels(ProjectionChangedEventBase schedule);
	}
}