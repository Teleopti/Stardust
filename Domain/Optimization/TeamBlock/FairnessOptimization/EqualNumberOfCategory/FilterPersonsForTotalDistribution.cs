

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IFilterPersonsForTotalDistribution
	{
		IEnumerable<IPerson> Filter(IList<IScheduleMatrixPro> allPersonMatrixList);
	}

	public class FilterPersonsForTotalDistribution : IFilterPersonsForTotalDistribution
	{
		private readonly bool _schedulerHidePointsFairnessSystem28317;
		private readonly bool _schedulerSeniority11111;

		public FilterPersonsForTotalDistribution(bool Scheduler_HidePointsFairnessSystem_28317, bool Scheduler_Seniority_11111)
		{
			_schedulerHidePointsFairnessSystem28317 = Scheduler_HidePointsFairnessSystem_28317;
			_schedulerSeniority11111 = Scheduler_Seniority_11111;
		}

		public IEnumerable<IPerson> Filter(IList<IScheduleMatrixPro> allPersonMatrixList)
		{
			var personListForTotalDistribution = new HashSet<IPerson>();
			foreach (var scheduleMatrixPro in allPersonMatrixList)
			{
				var person = scheduleMatrixPro.Person;
				var workflowControlSet = person.WorkflowControlSet;
				if (workflowControlSet == null) continue;
				if (workflowControlSet.GetFairnessType(_schedulerHidePointsFairnessSystem28317, _schedulerSeniority11111) == FairnessType.EqualNumberOfShiftCategory)
					personListForTotalDistribution.Add(person);
			}

			return personListForTotalDistribution;
		}
	}
}