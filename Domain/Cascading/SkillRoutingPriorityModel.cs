using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Cascading
{
	public interface ISkillRoutingPriorityModel
	{
		List<SkillRoutingPriorityModelRow> SkillRoutingPriorityModelRows();
	}

	public class SkillRoutingPriorityModel : ISkillRoutingPriorityModel
	{
		private readonly ISkillRepository _skillRepository;

		public SkillRoutingPriorityModel(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		public List<SkillRoutingPriorityModelRow> SkillRoutingPriorityModelRows()
		{
			var allSkills = _skillRepository.LoadAll().Where(x => !((IDeleteTag)x).IsDeleted);
			var skillList = new List<SkillRoutingPriorityModelRow>();
			foreach (var skill in allSkills)
			{
				var modelRow = new SkillRoutingPriorityModelRow();
				modelRow.ActivityGuid = skill.Activity.Id.Value;
				modelRow.ActivityName = skill.Activity.Name;
				modelRow.Priority = skill.CascadingIndex;
				modelRow.SkillGuid = skill.Id.Value;
				modelRow.SkillName = skill.Name;

				skillList.Add(modelRow);
			}
			return skillList;
		}
	}

	public class SkillRoutingPriorityModelRow
	{
		public Guid ActivityGuid { get; set; }
		public string ActivityName { get; set; }
		public int? Priority { get; set; }
		public string SkillName { get; set; }
		public Guid SkillGuid { get; set; }
	}
}