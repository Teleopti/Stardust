using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PublishedScheduleSpecification : Specification<IPersonScheduleDayReadModel>
	{
		private readonly ISchedulePersonProvider _schedulePersonProvider;
		private readonly DateTime _date;
		private readonly IEnumerable<IPerson> _personsInTeam;

		public PublishedScheduleSpecification(ISchedulePersonProvider schedulePersonProvider, Guid teamId, DateTime date)
		{
			_schedulePersonProvider = schedulePersonProvider;
			_date = date;
			_personsInTeam = _schedulePersonProvider.GetPermittedPersonsForTeam(new DateOnly(date), 
																		  teamId,
			                                                              DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere);
		}

		public override bool IsSatisfiedBy(IPersonScheduleDayReadModel obj)
		{
			var person = (from p in _personsInTeam where (p.Id == obj.PersonId) select p).FirstOrDefault();

			if (person != null && isSchedulePublished(_date, person))
				return true;
			return false;
		}

		private static bool isSchedulePublished(DateTime date, IPerson person)
		{
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return false;
			return workflowControlSet.SchedulePublishedToDate.HasValue &&
			       workflowControlSet.SchedulePublishedToDate.Value.AddDays(1) > date;
		}
	}
}