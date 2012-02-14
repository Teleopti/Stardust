﻿using System;
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
    public partial class SkillSummary : BaseRibbonForm
    {
        private readonly IList<ISkill> _skills = new List<ISkill>();
        private IAggregateSkill _aggregateSkillSkill;

        public SkillSummary(IAggregateSkill aggregateSkillSkill, IList<ISkill> skills)
        {
            SetupForm();
            _skills = skills;
            _aggregateSkillSkill = aggregateSkillSkill;
            textBoxSummeryName.Text = ((ISkill) _aggregateSkillSkill).Name;
            LoadSkills();
        }
        public SkillSummary(IList<ISkill> skills)
        {
            SetupForm();
            _skills = skills;
            LoadSkills();
        }

        /// <summary>
        /// Gets the virtual skill.
        /// </summary>
        /// <value>The virtual skill.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-20
        /// </remarks>
        public IAggregateSkill AggregateSkillSkill { get { return _aggregateSkillSkill; }}

        private void SetupForm()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            BackColor = ColorHelper.DialogBackColor();
            checkedListBoxSkill.Sorted = false;
            checkedListBoxSkill.DisplayMember = "Name";
        }

        /// <summary>
        /// Loads the skills, first checked items alphabetical, then unchecked alphabetical.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-23
        /// </remarks>
        private void LoadSkills()
        {
            List<ISkill> sortedList = new List<ISkill>();
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

        private void buttonAdvOk_Click(object sender, EventArgs e)
        {
            if (textBoxSummeryName.Text.Length == 0)
            {
                MessageBox.Show(UserTexts.Resources.EnterANameForTheGrouping, UserTexts.Resources.NoNameWasEntered, MessageBoxButtons.OK,
                   MessageBoxIcon.Exclamation,
                   MessageBoxDefaultButton.Button1,
                   (RightToLeft == RightToLeft.Yes)
                       ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                       : 0);
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
