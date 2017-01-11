using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ReverseSkillRoutingPriorities
	{
		public List<SkillRoutingPriorityModelRow> ReverseList(List<SkillRoutingPriorityModelRow> list)
		{
			var nullList = list.Where(r => !r.Priority.HasValue);
			var workingList = list.Where(r => r.Priority.HasValue);
			var skillRoutingPriorityModelRows = workingList as IList<SkillRoutingPriorityModelRow> ?? workingList.ToList();
			if (!skillRoutingPriorityModelRows.Any())
				return nullList.ToList();

			var reversedList = new List<SkillRoutingPriorityModelRow>();
			while (true)
			{
				var min = skillRoutingPriorityModelRows.Min(r => r.Priority);
				var max = skillRoutingPriorityModelRows.Max(r => r.Priority);
				var minGuids = skillRoutingPriorityModelRows.Where(r => r.Priority == min).ToList();
				var maxGuids = skillRoutingPriorityModelRows.Where(r => r.Priority == max).ToList();
				foreach (var skillRoutingPriorityModelRow in minGuids)
				{
					skillRoutingPriorityModelRow.Priority = max;
					reversedList.Add(skillRoutingPriorityModelRow);
				}
				foreach (var skillRoutingPriorityModelRow in maxGuids)
				{
					skillRoutingPriorityModelRow.Priority = min;
					if(reversedList.All(r => r.SkillGuid != skillRoutingPriorityModelRow.SkillGuid))
						reversedList.Add(skillRoutingPriorityModelRow);
				}
				skillRoutingPriorityModelRows = workingList.Where(r => r.Priority > min && r.Priority < max).ToList();
				if (!skillRoutingPriorityModelRows.Any())
					return nullList.Concat(reversedList).ToList();
			}	
		}
	}
}