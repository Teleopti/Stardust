using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AbsenceWaitlisting
{
	public class ArrangeRequestsByProcessOrder
	{
		public IList<IPersonRequest> GetRequestsSortedByDate(IList<IPersonRequest> personReuqestList)
		{
			var result =
				personReuqestList.Where(
					x => x.Person.WorkflowControlSet.AbsenceRequestWaitlistProcessOrder != WaitlistProcessOrder.BySeniority);
			return result.OrderBy(x => x.CreatedOn).ToList();
		}

		public IList<IPersonRequest> GetRequestsSortedBySeniority(IList<IPersonRequest> personReuqestList)
		{
			var requestsHavingSenerioty =
				personReuqestList.Where(
					x => x.Person.WorkflowControlSet.AbsenceRequestWaitlistProcessOrder == WaitlistProcessOrder.BySeniority);
			return requestsHavingSenerioty.OrderByDescending(x => x.Person.Seniority).ThenBy(x => x.CreatedOn).ToList();
		}
	}
}