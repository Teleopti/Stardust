using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.Cascading
{
	public class CascadingSkillModel
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IList<ISkill> _nonCascadingSkills;
		private readonly IList<ISkill> _cascadingSkills;

		public CascadingSkillModel(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
			_nonCascadingSkills = new List<ISkill>();
			_cascadingSkills = new List<ISkill>();
		}

		public void Init()
		{
			var skills = _skillRepository.LoadAll().OrderBy(x => x.CascadingIndex);
			foreach (var skill in skills)
			{
				if (skill.IsCascading())
				{
					_cascadingSkills.Add(skill);
				}
				else
				{
					_nonCascadingSkills.Add(skill);
				}
			}
		}

		public IEnumerable<ISkill> NonCascadingSkills => _nonCascadingSkills;

		public IEnumerable<ISkill> CascadingSkills => _cascadingSkills;

		public void MakeCascading(ISkill skill)
		{
			if (_nonCascadingSkills.Remove(skill))
			{
				_cascadingSkills.Add(skill);
			}
		}

		public void MakeNonCascading(ISkill skill)
		{
			if (_cascadingSkills.Remove(skill))
			{
				_nonCascadingSkills.Add(skill);
			}
		}

		public void Confirm()
		{
			foreach (var skill in _cascadingSkills)
			{
				skill.SetCascadingIndex(_cascadingSkills);
			}

			foreach (var nonCascadingSkill in _nonCascadingSkills)
			{
				nonCascadingSkill.ClearCascadingIndex();
			}
		}

		public void MoveUpCascadingSkill(ISkill skill)
		{
			var currentIndex = _cascadingSkills.IndexOf(skill);
			if (currentIndex > 0)
			{
				_cascadingSkills.RemoveAt(currentIndex);
				_cascadingSkills.Insert(currentIndex - 1, skill);
			}
		}

		public void MoveDownCascadingSkill(ISkill skill)
		{
			var currentIndex = _cascadingSkills.IndexOf(skill);
			if (currentIndex > -1 && currentIndex < _cascadingSkills.Count - 1)
			{
				_cascadingSkills.RemoveAt(currentIndex);
				_cascadingSkills.Insert(currentIndex + 1, skill);
			}
		}
	}
}