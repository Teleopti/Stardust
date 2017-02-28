﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PublishedScheduleSpecification : Specification<IPersonScheduleDayReadModel>
	{
		private readonly IEnumerable<IPerson> _permittedPersons;
		private readonly DateOnly _date;

		public PublishedScheduleSpecification(IEnumerable<IPerson> permittedPersons, DateOnly date)
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