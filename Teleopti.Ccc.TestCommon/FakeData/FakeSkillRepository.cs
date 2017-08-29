using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSkillRepository : ISkillRepository
	{
		private readonly IList<ISkill> _skills;
 
		public FakeSkillRepository()
		{
			_skills = new List<ISkill>();
		}

		public ISkill Has(string skillName, IActivity activity, int? cascadingIndex)
		{
			var skill = SkillFactory.CreateSkill(skillName).WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			skill.Activity = activity;
			_skills.Add(skill);

			if (cascadingIndex.HasValue)
			{
				skill.SetCascadingIndex(cascadingIndex.Value);
			}
			return skill;
		}

		public ISkill Has(string skillName, IActivity activity, TimePeriod openHours)
		{
			var skill = SkillFactory.CreateSkill(skillName).WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			skill.Activity = activity;
			_skills.Add(skill);
			return skill;
		}

		public ISkill Has(string skillName, IActivity activity)
		{
			return Has(skillName, activity, null);
		}

		public void Has(params ISkill[] skills)
		{
			foreach (var skill in skills)
			{
				_skills.Add(skill);
			}
		}

		public void Add(ISkill skill)
		{
			_skills.Add(skill);
		}

		public void Remove(ISkill skill)
		{
			_skills.Remove(_skills.First(x => x.Id == skill.Id));
		}

		public ISkill Get(Guid id)
		{
			return _skills.FirstOrDefault(x => x.Id == id);
		}

		public IList<ISkill> LoadAll()
		{
			return _skills;
		}

		public ISkill Load(Guid id)
		{
			return _skills.First(x => x.Id == id);
		}

		public ICollection<ISkill> FindAllWithWorkloadAndQueues()
		{
			throw new NotImplementedException();
		}

		public ICollection<ISkill> FindAllWithoutMultisiteSkills()
		{
			return _skills;
		}

		public ICollection<ISkill> FindAllWithSkillDays(DateOnlyPeriod periodWithSkillDays)
		{
			//This is not correct. Depending on another aggregate... Leave that problem for now
			return _skills.ToArray();
		}

		public ISkill LoadSkill(ISkill skill)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ISkill> LoadAllSkills()
		{
			return _skills;
		}

		public IMultisiteSkill LoadMultisiteSkill(ISkill skill)
		{
			throw new NotImplementedException();
		}

		public int MinimumResolution()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ISkill> FindSkillsWithAtLeastOneQueueSource()
		{
			return _skills;
		}

		public ICollection<ISkill> LoadSkills(IEnumerable<Guid> skillIdList)
		{
			return _skills.Where(s => skillIdList.Contains(s.Id.Value)).ToList();
		}

		public IEnumerable<ISkill> FindSkillsContain(string searchString, int maxHits)
		{
			return _skills.Where(x => x.Name.Contains(searchString)).Take(maxHits);
		}

		public IMultisiteSkill HasMultisiteSkill(string skillName, IActivity activity)
		{
			var skill = SkillFactory.CreateMultisiteSkill(skillName).WithId();
			var childSkill1 = SkillFactory.CreateChildSkill(skillName + " child1", skill).WithId();
			var childSkill2 = SkillFactory.CreateChildSkill(skillName + " child2", skill).WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			skill.Activity = activity;
			childSkill1.Activity = activity;
			childSkill2.Activity = activity;
			_skills.Add(skill);
			_skills.Add(childSkill1);
			_skills.Add(childSkill2);
			return skill;
		}
	}
}