

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IFilterPersonsForTotalDistribution
	{
		IEnumerable<IPerson> Filter(IList<IScheduleMatrixPro> allPersonMatrixList, bool schedulerSeniority11111);
	}

	public class FilterPersonsForTotalDistribution : IFilterPersonsForTotalDistribution
	{

		public IEnumerable<IPerson> Filter(IList<IScheduleMatrixPro> allPersonMatrixList, bool schedulerSeniority11111)
		{
			var personListForTotalDistribution = new HashSet<IPerson>();
			foreach (var scheduleMatrixPro in allPersonMatrixList)
			{
				var person = scheduleMatrixPro.Person;
				var workflowControlSet = person.WorkflowControlSet;
				if (workflowControlSet == null) continue;
				if (workflowControlSet.GetFairnessType(schedulerSeniority11111) == FairnessType.EqualNumberOfShiftCategory)
					personListForTotalDistribution.Add(person);
			}

			return personListForTotalDistribution;
		}
	}
}