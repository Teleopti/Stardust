using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.Cascading
{
	public class CascadingSkillPresenter
	{
		private readonly ISkillRepository _skillRepository;
		private cascadingSkillModel _internalModel;

		public CascadingSkillPresenter(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		public IEnumerable<ISkill> NonCascadingSkills => model().NonCascadingSkills;

		public IEnumerable<ISkill> CascadingSkills => model().CascadingSkills;

		public void MakeCascading(ISkill skill)
		{
			if (model().NonCascadingSkills.Remove(skill))
			{
				model().CascadingSkills.Add(skill);
			}
		}

		public void MakeNonCascading(ISkill skill)
		{
			if (model().CascadingSkills.Remove(skill))
			{
				model().NonCascadingSkills.Add(skill);
			}
		}

		public void Confirm()
		{
			foreach (var skill in model().CascadingSkills)
			{
				skill.SetCascadingIndex(model().CascadingSkills);
			}

			foreach (var nonCascadingSkill in model().NonCascadingSkills)
			{
				nonCascadingSkill.ClearCascadingIndex();
			}
		}

		public void MoveUpCascadingSkill(ISkill skill)
		{
			var currentIndex = model().CascadingSkills.IndexOf(skill);
			if (currentIndex > 0)
			{
				model().CascadingSkills.RemoveAt(currentIndex);
				model().CascadingSkills.Insert(currentIndex - 1, skill);
			}
		}

		public void MoveDownCascadingSkill(ISkill skill)
		{
			var currentIndex = model().CascadingSkills.IndexOf(skill);
			if (currentIndex > -1 && currentIndex < model().CascadingSkills.Count - 1)
			{
				model().CascadingSkills.RemoveAt(currentIndex);
				model().CascadingSkills.Insert(currentIndex + 1, skill);
			}
		}

		private cascadingSkillModel model()
		{
			if (_internalModel == null)
			{
				_internalModel = new cascadingSkillModel();
				var skills = _skillRepository.LoadAll().OrderBy(x => x.CascadingIndex);
				foreach (var skill in skills)
				{
					if (skill.IsCascading())
					{
						_internalModel.CascadingSkills.Add(skill);
					}
					else
					{
						_internalModel.NonCascadingSkills.Add(skill);
					}
				}
			}
			return _internalModel;
		}

		private class cascadingSkillModel
		{
			public cascadingSkillModel()
			{
				NonCascadingSkills = new List<ISkill>();
				CascadingSkills = new List<ISkill>();
			}

			public IList<ISkill> NonCascadingSkills { get; }
			public IList<ISkill> CascadingSkills { get; }
		}
	}
}