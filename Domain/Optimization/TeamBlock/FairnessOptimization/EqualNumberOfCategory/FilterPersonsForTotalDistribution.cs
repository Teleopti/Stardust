

using System.Collections.Generic;
using System.Linq;
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
			var groupedByWorkflowControlSet = allPersonMatrixList.Select(m => m.Person).Where(p => p.WorkflowControlSet != null)
				.ToLookup(w => w.WorkflowControlSet);

			var personListForTotalDistribution = new List<IPerson>();
			foreach (var group in groupedByWorkflowControlSet)
			{
				if (group.Key.GetFairnessType() == FairnessType.EqualNumberOfShiftCategory)
					personListForTotalDistribution.AddRange(group.Distinct());
			}

			return personListForTotalDistribution;
		}
	}
}