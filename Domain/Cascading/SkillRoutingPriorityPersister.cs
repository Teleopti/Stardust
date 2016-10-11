using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class SkillRoutingPriorityPersister
	{
		private readonly ISkillRepository _skillRepository;

		public SkillRoutingPriorityPersister(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		public void Persist(IEnumerable<SkillRoutingPriorityModelRow> skillRoutingPriorityModelRows)
		{
			var skillPrioDic = new SortedDictionary<int, IList<Guid>>();
			var skillWithoutPrioList = new List<Guid>();
			foreach (var skillRoutingPriorityModelRow in skillRoutingPriorityModelRows)
			{
				if (!skillRoutingPriorityModelRow.Priority.HasValue)
				{
					skillWithoutPrioList.Add(skillRoutingPriorityModelRow.SkillGuid);
				}
				else
				{
					IList<Guid> listWithSameIndex;
					if (!skillPrioDic.TryGetValue(skillRoutingPriorityModelRow.Priority.Value, out listWithSameIndex))
					{
						skillPrioDic.Add(skillRoutingPriorityModelRow.Priority.Value, new List<Guid> { skillRoutingPriorityModelRow.SkillGuid });
					}
					else
					{
						listWithSameIndex.Add(skillRoutingPriorityModelRow.SkillGuid);
					}
				}
			}

			var allSkills = _skillRepository.LoadAll().ToDictionary(s => s.Id.Value);
			confirm(allSkills,skillPrioDic,skillWithoutPrioList);
		}

		private void confirm(IDictionary<Guid, ISkill> allSkills, SortedDictionary<int, IList<Guid>> prioSortedDictionary,
			List<Guid> skillsToClear)
		{
			var index = 0;
			foreach (var keyValuePair in prioSortedDictionary)
			{
				var skillList = prioSortedDictionary[keyValuePair.Key];
				var detectedActivityGuid = Guid.Empty;
				foreach (var skillGuid in skillList)
				{
					if (detectedActivityGuid != Guid.Empty && allSkills[skillGuid].Activity.Id != detectedActivityGuid)
					{
						throw new InvalidOperationException("Two skills with different activities on the same cascading level");
					}
					detectedActivityGuid = allSkills[skillGuid].Activity.Id.Value;
					allSkills[skillGuid].SetCascadingIndex(index + 1);
				}

				index++;
			}

			foreach (var nonCascadingSkillGuid in skillsToClear)
			{
				allSkills[nonCascadingSkillGuid].ClearCascadingIndex();
			}
		}
	}
}