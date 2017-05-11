

using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IFilterPersonsForTotalDistribution
	{
		IEnumerable<IPerson> Filter(IEnumerable<IScheduleMatrixPro> allPersonMatrixList);
	}

	public class FilterPersonsForTotalDistribution : IFilterPersonsForTotalDistribution
	{

		public IEnumerable<IPerson> Filter(IEnumerable<IScheduleMatrixPro> allPersonMatrixList)
		{
			var personListForTotalDistribution = new HashSet<IPerson>();
			foreach (var scheduleMatrixPro in allPersonMatrixList)
			{
				var person = scheduleMatrixPro.Person;
				var workflowControlSet = person.WorkflowControlSet;
				if (workflowControlSet == null) continue;
				if (workflowControlSet.GetFairnessType() == FairnessType.EqualNumberOfShiftCategory)
					personListForTotalDistribution.Add(person);
			}

			return personListForTotalDistribution;
		}
	}
}