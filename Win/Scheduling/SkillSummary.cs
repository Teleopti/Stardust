using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class SkillSummary : BaseDialogForm
	{
		private readonly IList<ISkill> _skills = new List<ISkill>();
		private IAggregateSkill _aggregateSkillSkill;

		public SkillSummary(IAggregateSkill aggregateSkillSkill, IList<ISkill> skills)
		{
			setupForm();
			_skills = skills;
			_aggregateSkillSkill = aggregateSkillSkill;
			textBoxSummeryName.Text = ((ISkill) _aggregateSkillSkill).Name;
			loadSkills();
		}
		public SkillSummary(IList<ISkill> skills)
		{
			setupForm();
			_skills = skills;
			loadSkills();
		}

		public IAggregateSkill AggregateSkillSkill { get { return _aggregateSkillSkill; }}

		private void setupForm()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			BackColor = ColorHelper.DialogBackColor();
			checkedListBoxSkill.Sorted = false;
			checkedListBoxSkill.DisplayMember = "Name";
		}

		private void loadSkills()
		{
			var sortedList = new List<ISkill>();
			if (_aggregateSkillSkill != null)
			{
				IList<ISkill> maxSeatSkillsToRemove = new List<ISkill>();
				foreach (ISkill skill in _aggregateSkillSkill.AggregateSkills)
				{
					if(skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
					{
						maxSeatSkillsToRemove.Add(skill);
						continue;
					}
						
					sortedList.Add(skill);
					checkedListBoxSkill.Items.Add(skill, true);
				}

				foreach (var skill in maxSeatSkillsToRemove)
				{
					_aggregateSkillSkill.RemoveAggregateSkill(skill);
				}
			}


			foreach (ISkill skill in _skills.OrderBy(s => s.Name).ToList())
			{
				if (!sortedList.Contains(skill) && skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
				{
					checkedListBoxSkill.Items.Add(skill, false);
				}
			}
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			if (textBoxSummeryName.Text.Length == 0)
			{
				ViewBase.ShowWarningMessage(UserTexts.Resources.EnterANameForTheGrouping, UserTexts.Resources.NoNameWasEntered);
				return;
			}

		   if (_aggregateSkillSkill == null)
				_aggregateSkillSkill = new Skill(textBoxSummeryName.Text, textBoxSummeryName.Text, Color.Pink, 15, _skills[0].SkillType);
			else
			{
				var skill = (ISkill) AggregateSkillSkill;
				skill.Name = textBoxSummeryName.Text;
				skill.Description = textBoxSummeryName.Text;
			}
			AggregateSkillSkill.ClearAggregateSkill();
			foreach (ISkill skill in checkedListBoxSkill.CheckedItems)
			{
				AggregateSkillSkill.AddAggregateSkill(skill);
			}
			AggregateSkillSkill.IsVirtual = true;
			setDefaultResolutionFromSmallestSkillResolution();

			DialogResult = DialogResult.OK;
		}

		private void setDefaultResolutionFromSmallestSkillResolution()
		{
			if (AggregateSkillSkill.AggregateSkills.Any())
			{
				((ISkill)AggregateSkillSkill).DefaultResolution =
					AggregateSkillSkill.AggregateSkills.Min(s => s.DefaultResolution);
			}
		}
	}
}
