using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class SkillRoutingPriorityModel
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IActivityRepository _activityRepository;

		public SkillRoutingPriorityModel(ISkillRepository skillRepository, IActivityRepository activityRepository)
		{
			_skillRepository = skillRepository;
			_activityRepository = activityRepository;
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

		public List<SkillRoutingActivityRow> SkillRoutingActivites()
		{
			var allActivites = _activityRepository.LoadAll().Where(x => !x.IsDeleted && x.RequiresSkill);
			var activityList = new List<SkillRoutingActivityRow>();
			foreach (var activity in allActivites)
			{
				activityList.Add(new SkillRoutingActivityRow {ActivityGuid = activity.Id.Value, ActivityName = activity.Name});
			}

			return activityList;
		}
	}

	[Serializable]
	public class SkillRoutingPriorityModelRow
	{
		public Guid ActivityGuid { get; set; }
		public string ActivityName { get; set; }
		public int? Priority { get; set; }
		public string SkillName { get; set; }
		public Guid SkillGuid { get; set; }
	}

	[Serializable]
	public class SkillRoutingActivityRow
	{
		public Guid ActivityGuid { get; set; }
		public string ActivityName { get; set; }
	}
}