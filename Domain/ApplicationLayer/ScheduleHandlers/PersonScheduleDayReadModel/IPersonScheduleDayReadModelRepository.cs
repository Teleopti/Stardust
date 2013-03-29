using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IPersonScheduleDayReadModelRepository
	{
		IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId);
		IEnumerable<PersonScheduleDayReadModel> ForTeam(DateTimePeriod period, Guid teamId);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId);

		void SaveReadModel(PersonScheduleDayReadModel model);
		bool IsInitialized();
	}
}