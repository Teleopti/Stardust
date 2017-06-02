using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PublishedScheduleSpecification : Specification<IPersonScheduleDayReadModel>
	{
		private readonly ILookup<Guid?, IPerson> _permittedPersons;
		private readonly DateOnly _date;

		public PublishedScheduleSpecification(IEnumerable<IPerson> permittedPersons, DateOnly date)
		{
			_permittedPersons = permittedPersons.ToLookup(p => p.Id);
			_date = date;
		}

		public override bool IsSatisfiedBy(IPersonScheduleDayReadModel obj)
		{
			var person = _permittedPersons[obj.PersonId].FirstOrDefault();
			return person != null && isSchedulePublished(_date, person);
		}

		private static bool isSchedulePublished(DateOnly date, IPerson person)
		{
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return false;
			return workflowControlSet.SchedulePublishedToDate.HasValue &&
			       workflowControlSet.SchedulePublishedToDate.Value >= date.Date;
		}
	}
}