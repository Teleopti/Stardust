using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.SkillPages
{
	public interface ISkillGeneralView
	{
		void SetActivityList(IList<IActivity> activities);
		void SetResolutionEnableState(bool enabled);
		IActivity SelectedActivity();
		void SetSelectedResolution(int resolution);
	}

	public class SkillGeneralPresenter
	{
		private readonly ISkillGeneralView _skillGeneralView;
		private readonly ISkill _theSkill;
		private IList<IActivity> _activities;
		private readonly IList<ISkill> _allSkills;


		public SkillGeneralPresenter(ISkillGeneralView skillGeneralView, ISkill theSkill, IEnumerable<IActivity> allActivities, IList<ISkill> allSkills)
		{
			_skillGeneralView = skillGeneralView;
			_theSkill = theSkill;
			_allSkills = allSkills;
			CheckActivitiesList(allActivities);
			SetResolutionStatus();
		}

		public void OnActivityChanged()
		{
			SetResolutionStatus();
		}

		public void SetActivitiesList()
		{
			//saved
			if (_theSkill.Id != null)
			{
				SetActivitiesToSavedSkill();
				return;
			}
			_skillGeneralView.SetActivityList(_activities);
		}

		private void SetResolutionStatus()
		{
			// saved
			if (_theSkill.Id != null)
			{
				_skillGeneralView.SetSelectedResolution(_theSkill.DefaultResolution);
				_skillGeneralView.SetResolutionEnableState(false);
				return;
			}
			IList<ISkill> skills = GetListOfSkills(_skillGeneralView.SelectedActivity());
			if (skills.Count == 0)
			{
				_skillGeneralView.SetSelectedResolution(_theSkill.DefaultResolution);
				_skillGeneralView.SetResolutionEnableState(true);
				return;
			}

			_skillGeneralView.SetSelectedResolution(skills[0].DefaultResolution);
			_skillGeneralView.SetResolutionEnableState(false);

		}
		private IList<ISkill> GetListOfSkills(IActivity activity)
		{	
			IList<ISkill> retList = new List<ISkill>();
			foreach (ISkill skill in _allSkills)
			{
				if (_theSkill.Equals(skill))
					continue;

				if (skill.Activity.Equals(activity))
					retList.Add(skill);
			}
			return retList;
			
		}

		private void CheckActivitiesList(IEnumerable<IActivity> allActivities)
		{
			_activities = new List<IActivity>();
			foreach (IActivity activity in allActivities)
			{
				if (activity.RequiresSkill && !activity.IsDeleted)
				{
					_activities.Add(activity);
				}
			}
			if (_theSkill.Activity != null)
			{
				if (!_activities.Contains(_theSkill.Activity))
					_activities.Add(_theSkill.Activity);
			}
		}

		private void SetActivitiesToSavedSkill()
		{
			var resolution = _theSkill.DefaultResolution;
			foreach (var skill in _allSkills)
			{
				if (skill.DefaultResolution != resolution)
					_activities.Remove(skill.Activity);
			}
			_skillGeneralView.SetActivityList(_activities);
		}
	}

	
}