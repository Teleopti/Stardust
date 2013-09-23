using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PublishedScheduleSpecification : Specification<IPersonScheduleDayReadModel>
	{
		private readonly IEnumerable<IPerson> _permittedPersons;
		private readonly DateTime _date;

		public PublishedScheduleSpecification(IEnumerable<IPerson> permittedPersons, DateTime date)
		{
			_permittedPersons = permittedPersons;
			_date = date;
		}

		public override bool IsSatisfiedBy(IPersonScheduleDayReadModel obj)
		{
			var person = (from p in _permittedPersons where (p.Id == obj.PersonId) select p).FirstOrDefault();

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