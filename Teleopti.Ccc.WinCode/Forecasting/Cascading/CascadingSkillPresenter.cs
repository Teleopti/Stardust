using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting.Cascading
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_CascadingSkillsGUI_40018)]
	public class CascadingSkillPresenter
	{
		private readonly ISkillRepository _skillRepository;
		private cascadingSkillModel _internalModel;

		public CascadingSkillPresenter(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		public IList<ISkill> NonCascadingSkills => model().NonCascadingSkills;

		public IList<List<ISkill>> CascadingSkills => model().CascadingSkills;

		public void MakeCascading(ISkill skill)
		{
			if (NonCascadingSkills.Remove(skill))
			{
				model().AddNewCascadingSkill(skill);
			}
		}

		public void MakeNonCascading(ISkill skill)
		{
			if (model().RemoveCascadingSkill(skill))
			{
				NonCascadingSkills.Add(skill);
			}
		}

		public void Confirm()
		{
			for (var index = 0; index < model().CascadingSkills.Count; index++)
			{
				foreach (var skill in model().CascadingSkills[index])
				{
					skill.SetCascadingIndex(index + 1);
				}
			}

			foreach (var nonCascadingSkill in model().NonCascadingSkills)
			{
				nonCascadingSkill.ClearCascadingIndex();
			}
		}

		public void MoveUpCascadingSkills(ISkill skill)
		{
			var index = model().FindItemIndexForSkill(skill);
			if (index > 0)
			{
				var currentSkills = CascadingSkills[index];
				CascadingSkills.RemoveAt(index);
				CascadingSkills.Insert(index - 1, currentSkills);
			}
		}

		public void MoveDownCascadingSkills(ISkill skill)
		{
			var index = model().FindItemIndexForSkill(skill);
			if (index > -1 && index < model().CascadingSkills.Count - 1)
			{
				var currentSkills = model().CascadingSkills[index];
				CascadingSkills.RemoveAt(index);
				CascadingSkills.Insert(index + 1, currentSkills);
			}
		}

		public void MakeParalell(ISkill masterSkill, ISkill skillToInclude)
		{
			var indexForSkillToInclude = model().FindItemIndexForSkill(skillToInclude);
			var indexForMasterSkill = model().FindItemIndexForSkill(masterSkill);
			CascadingSkills[indexForMasterSkill].AddRange(CascadingSkills[indexForSkillToInclude]);
			CascadingSkills[indexForMasterSkill].Sort(new skillNameSorter());
			CascadingSkills.RemoveAt(indexForSkillToInclude);
		}


		public void Unparalell(ISkill skill)
		{
			var index = model().FindItemIndexForSkill(skill);
			var skills = CascadingSkills[index];
			skills.Reverse();
			CascadingSkills.RemoveAt(index);
			foreach (var skillToAdd in skills)
			{
				CascadingSkills.Insert(index, new List<ISkill> { skillToAdd });
			}
		}

		private cascadingSkillModel model()
		{
			if (_internalModel == null)
			{
				_internalModel = new cascadingSkillModel();
				var allSkills = _skillRepository.LoadAll().Where(x => !((IDeleteTag) x).IsDeleted);
				var cascadingSkills = allSkills.Where(x => x.IsCascading()).OrderBy(x => x.CascadingIndex);
				var nonCascadingSkills = allSkills.Except(cascadingSkills).Where(x=> x.CanBeCascading()).OrderBy(x => x.Name);

				foreach (var skillPerIndex in cascadingSkills.GroupBy(x => x.CascadingIndex))
				{
					_internalModel.CascadingSkills.Add(skillPerIndex.ToList());
				}

				foreach (var nonCascadingSkill in nonCascadingSkills)
				{
					_internalModel.NonCascadingSkills.Add(nonCascadingSkill);
				}
			}
			return _internalModel;
		}


		private class skillNameSorter : IComparer<ISkill>
		{
			public int Compare(ISkill x, ISkill y)
			{
				return string.CompareOrdinal(x.Name, y.Name);
			}
		}

		private class cascadingSkillModel
		{
			public cascadingSkillModel()
			{
				NonCascadingSkills = new List<ISkill>();
				CascadingSkills = new List<List<ISkill>>();
			}

			public IList<ISkill> NonCascadingSkills { get; }
			public IList<List<ISkill>> CascadingSkills { get; }

			public void AddNewCascadingSkill(ISkill skill)
			{
				CascadingSkills.Add(new List<ISkill> {skill});
			}

			public bool RemoveCascadingSkill(ISkill skill)
			{
				for (var i = CascadingSkills.Count - 1; i >= 0; i--)
				{
					var cascadingSkill = CascadingSkills[i];
					if (cascadingSkill.Remove(skill))
					{
						if (!cascadingSkill.Any())
						{
							CascadingSkills.Remove(cascadingSkill);
						}
						return true;
					}
				}
				return false;
			}

			public int FindItemIndexForSkill(ISkill skill)
			{
				for (var index = 0; index < CascadingSkills.Count; index++)
				{
					if (CascadingSkills[index].Any(theSkill => theSkill.Equals(skill)))
					{
						return index;
					}
				}
				return -1;
			}
		}
	}
}