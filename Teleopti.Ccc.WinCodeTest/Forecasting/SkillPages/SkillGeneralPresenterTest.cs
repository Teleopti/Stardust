using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.SkillPages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.SkillPages
{
	[TestFixture]
	public class SkillGeneralPresenterTest
	{
		private MockRepository _mocks;
		private ISkillGeneralView _view;
		private ISkill _skill;
		private SkillGeneralPresenter _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_view = _mocks.DynamicMock<ISkillGeneralView>();
			_skill = _mocks.DynamicMock<ISkill>();

		}

		[Test]
		public void ShouldSetActivitiesListAccordingToResolutionIfSavedSkill()
		{
			//should only set not connected activities or activities connected to skill
			//with same resolution as the one edited. _skill and _skill2 = 15 resolution, skill3 = 60
			// activity1 not connected, activity2 conn. to _skill and skill2, activity3 connected to skill3
			// we should get activity1 and activity2
			var skill2 = _mocks.StrictMock<ISkill>();
			var skill3 = _mocks.StrictMock<ISkill>();

			var activity1 = _mocks.StrictMock<IActivity>();
			var activity2 = _mocks.StrictMock<IActivity>();
			var activity3 = _mocks.StrictMock<IActivity>();

			Expect.Call(activity1.RequiresSkill).Return(true);
			Expect.Call(activity2.RequiresSkill).Return(true);
			Expect.Call(activity3.RequiresSkill).Return(true);
			Expect.Call(activity1.IsDeleted).Return(false);
			Expect.Call(activity2.IsDeleted).Return(false);
			Expect.Call(activity3.IsDeleted).Return(false);

			Expect.Call(_skill.Id).Return(Guid.NewGuid());
			
			Expect.Call(_skill.DefaultResolution).Return(15).Repeat.Any();
			Expect.Call(skill2.DefaultResolution).Return(15);
			Expect.Call(skill3.DefaultResolution).Return(60);
			Expect.Call(skill3.Activity).Return(activity3);
			
			_view.SetActivityList(new List<IActivity> { activity1, activity2 });

			_mocks.ReplayAll();
			_target = new SkillGeneralPresenter(_view, _skill, new List<IActivity> {activity1, activity2, activity3 }, new List<ISkill> { _skill, skill2, skill3 });
			_target.SetActivitiesList();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldAddDeletedActivityToListIfSkillHasThatActivity()
		{
			var skill2 = _mocks.DynamicMock<ISkill>();
			var skill3 = _mocks.DynamicMock<ISkill>();

			var activity1 = _mocks.DynamicMock<IActivity>();
			var activity2 = _mocks.DynamicMock<IActivity>();
			var activity3 = _mocks.DynamicMock<IActivity>();

			Expect.Call(activity1.RequiresSkill).Return(true);
			Expect.Call(activity2.RequiresSkill).Return(true);
			Expect.Call(activity3.RequiresSkill).Return(true);
			Expect.Call(activity1.IsDeleted).Return(true);
			Expect.Call(activity2.IsDeleted).Return(false);
			Expect.Call(activity3.IsDeleted).Return(false);

			Expect.Call(skill2.Activity).Return(activity2);
			Expect.Call(skill3.Activity).Return(activity3);

			Expect.Call(_skill.Activity).Return(activity1);
			Expect.Call(_skill.Id).Return(null);

			_view.SetActivityList(new List<IActivity> { activity2, activity3, activity1 });
			_mocks.ReplayAll();
		 	_target = new SkillGeneralPresenter(_view, _skill, new List<IActivity> { activity1, activity2, activity3 }, new List<ISkill> { _skill, skill2, skill3 });
			_target.SetActivitiesList();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldDisableResolutionIfSavedSkill()
		{
			Expect.Call(_skill.Id).Return(Guid.NewGuid());
			Expect.Call(_skill.DefaultResolution).Return(30);
			_view.SetSelectedResolution(30);
			_view.SetResolutionEnableState(false);
			_mocks.ReplayAll();
			_target = new SkillGeneralPresenter(_view, _skill, new List<IActivity>(), new List<ISkill>());
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSetResolutionToEarlierSkillAndDisableIfActivityConnected()
		{
			var skill2 = _mocks.DynamicMock<ISkill>();
			var skill3 = _mocks.DynamicMock<ISkill>();
			var activity1 = _mocks.StrictMock<IActivity>();

			Expect.Call(activity1.RequiresSkill).Return(true);
			Expect.Call(activity1.IsDeleted).Return(false);
			
			Expect.Call(_skill.Id).Return(null);
			Expect.Call(_view.SelectedActivity()).Return(activity1);

			Expect.Call(_skill.Equals(_skill)).Return(true);
			Expect.Call(skill2.Activity).Return(activity1);
			Expect.Call(skill3.Activity).Return(activity1);
			Expect.Call(activity1.Equals(activity1)).Return(true).Repeat.Twice();
			Expect.Call(skill2.DefaultResolution).Return(30);
			_view.SetSelectedResolution(30);
			_view.SetResolutionEnableState(false);
			_mocks.ReplayAll();
			_target = new SkillGeneralPresenter(_view, _skill, new List<IActivity>{activity1}, new List<ISkill> {_skill, skill2, skill3 });
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSetToDefaultResolutionAndEnableOnUnconnectedActivity()
		{
			var skill2 = _mocks.DynamicMock<ISkill>();
			var skill3 = _mocks.DynamicMock<ISkill>();
			var activity1 = _mocks.StrictMock<IActivity>();
			var activity2 = _mocks.StrictMock<IActivity>();

			Expect.Call(activity1.RequiresSkill).Return(true);
			Expect.Call(activity2.RequiresSkill).Return(true);
			Expect.Call(activity1.IsDeleted).Return(false);
			Expect.Call(activity2.IsDeleted).Return(false);
			
			Expect.Call(_skill.Id).Return(null);
			Expect.Call(_skill.DefaultResolution).Return(15);
			Expect.Call(_view.SelectedActivity()).Return(activity2);

			Expect.Call(skill2.Activity).Return(activity1);
			Expect.Call(skill3.Activity).Return(activity1);
			Expect.Call(activity1.Equals(activity2)).Return(false).Repeat.Twice();

			_view.SetSelectedResolution(15);
			_view.SetResolutionEnableState(true);
			_mocks.ReplayAll();
			_target = new SkillGeneralPresenter(_view, _skill, new List<IActivity> { activity1, activity2 }, new List<ISkill> { skill2, skill3 });
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSetResolutionStatusOnActivityChanged()
		{
			Expect.Call(_skill.Id).Return(Guid.NewGuid());
			Expect.Call(_skill.DefaultResolution).Return(120);
			_view.SetSelectedResolution(120);
			_view.SetResolutionEnableState(false);
			_mocks.ReplayAll();
			_target = new SkillGeneralPresenter(_view, _skill, new List<IActivity>(), new List<ISkill> ());
			_target.OnActivityChanged();
			_mocks.VerifyAll();
		}
	}
}