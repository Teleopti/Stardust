using System;
using System.Globalization;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
    public partial class SkillDistributions : BaseUserControl, IPropertyPage
    {
        public SkillDistributions()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            moreOrLessButtonOptions.MoreText = UserTexts.Resources.Advanced;
        }

        private void moreOrLessButtonOptions_StateChanged(object sender, EventArgs e)
        {
            panelMoreOptions.Visible = ((MoreOrLessButton)sender).StateAsBoolean;
        }

        private void SetChildSkillVisible()
        {
            panelMoreOptions.Visible = true;
            moreOrLessButtonOptions.Visible = false;
            integerTextBoxServiceLevelSeconds.Visible = false;
            serviceLevelPercentTextBox.Visible = false;
            shrinkagePercentTextBox.Visible = false;
            efficiencyPercentTextBox.Visible = false;
            minimumOccupancyPercentTextBox.Visible = false;
            maximunOccupancyPercentTextBox.Visible = false;
            labelMaximumOccupancy.Visible = false;
            labelMinimumOccupancy.Visible = false;
            labelServiceLevelPercentage.Visible = false;
            labelServiceLevelTarget.Visible = false;
            labelShrinkage.Visible = false;
            labelEfficiency.Visible = false;
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            ISkill skill = aggregateRoot as ISkill;
            ISkillDayTemplate skillDayTemplate = (ISkillDayTemplate) skill.GetTemplate(TemplateTarget.Skill, DayOfWeek.Monday);
            if (skillDayTemplate.TemplateSkillDataPeriodCollection.Count > 0)
            {
                ITemplateSkillDataPeriod skillDataPeriodTemplate = skillDayTemplate.TemplateSkillDataPeriodCollection[0];

                serviceLevelPercentTextBox.DoubleValue =
                    skillDataPeriodTemplate.ServiceAgreement.ServiceLevel.Percent.Value;
                integerTextBoxServiceLevelSeconds.IntegerValue =
                    Convert.ToInt32(skillDataPeriodTemplate.ServiceAgreement.ServiceLevel.Seconds);
                minimumOccupancyPercentTextBox.DoubleValue = skillDataPeriodTemplate.ServiceAgreement.MinOccupancy.Value;
                maximunOccupancyPercentTextBox.DoubleValue = skillDataPeriodTemplate.ServiceAgreement.MaxOccupancy.Value;
                integerTextBoxMinimumAgents.IntegerValue =
                    skillDataPeriodTemplate.SkillPersonData.MinimumPersons;
                integerTextBoxMaximumAgents.IntegerValue =
                    skillDataPeriodTemplate.SkillPersonData.MaximumPersons;
                shrinkagePercentTextBox.DoubleValue = skillDataPeriodTemplate.Shrinkage.Value;
                efficiencyPercentTextBox.DoubleValue = skillDataPeriodTemplate.Efficiency.Value;
            }
            if (skill is IChildSkill)
            {
                SetChildSkillVisible();
            }
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            ISkill skill = aggregateRoot as ISkill;

            double serviceLevelPercent =
                Convert.ToDouble(serviceLevelPercentTextBox.DoubleValue);

            ServiceLevel serviceLevel = new ServiceLevel(new Percent(serviceLevelPercent),
                                                         Convert.ToDouble(
                                                             integerTextBoxServiceLevelSeconds.IntegerValue,
                                                             CultureInfo.CurrentCulture));
            ServiceAgreement serviceAgreement = new ServiceAgreement();
            serviceAgreement.ServiceLevel = serviceLevel;
            serviceAgreement.MinOccupancy = new Percent(minimumOccupancyPercentTextBox.DoubleValue);
            serviceAgreement.MaxOccupancy = new Percent(maximunOccupancyPercentTextBox.DoubleValue);

            int staffMin = (int) integerTextBoxMinimumAgents.IntegerValue;
            int staffMax = (int) integerTextBoxMaximumAgents.IntegerValue;

            SkillPersonData skillPersonData = new SkillPersonData(staffMin, staffMax);

            //Percent shrinkage =
            //    new Percent(Convert.ToDouble(integerTextBoxShrinkage.IntegerValue, CultureInfo.CurrentCulture)/100);
            Percent shrinkage = new Percent(shrinkagePercentTextBox.DoubleValue);

            Percent efficiency = new Percent(efficiencyPercentTextBox.DoubleValue);

            DateTime startDateTime = skill.TimeZone.ConvertTimeToUtc(SkillDayTemplate.BaseDate, skill.TimeZone);
            startDateTime = startDateTime.Add(skill.MidnightBreakOffset);

            DateTime endDateTime = skill.TimeZone.ConvertTimeToUtc(
                SkillDayTemplate.BaseDate.AddDays(1), skill.TimeZone);
            endDateTime = endDateTime.Add(skill.MidnightBreakOffset);
            DateTimePeriod timePeriod = new DateTimePeriod(startDateTime, endDateTime);

            ITemplateSkillDataPeriod templateSkillDataPeriod = new TemplateSkillDataPeriod(serviceAgreement,
                                                                                          skillPersonData, timePeriod);
            templateSkillDataPeriod.Shrinkage = shrinkage;
            templateSkillDataPeriod.Efficiency = efficiency;
            IList<ITemplateSkillDataPeriod> skillDataPeriods = new List<ITemplateSkillDataPeriod>();
            skillDataPeriods.Add((ITemplateSkillDataPeriod) templateSkillDataPeriod.Clone());

            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof (DayOfWeek)))
            {
                ISkillDayTemplate skillDayTemplate =
                    (ISkillDayTemplate) skill.GetTemplate(TemplateTarget.Skill, dayOfWeek);
                
                skillDayTemplate.SetSkillDataPeriodCollection(new List<ITemplateSkillDataPeriod>
                                                                  {
                                                                      (ITemplateSkillDataPeriod)
                                                                      templateSkillDataPeriod.Clone()
                                                                  });
                skill.SetTemplateAt((int) dayOfWeek, skillDayTemplate);
            }

            return true;
        }

        public string PageName
        {
            get { return UserTexts.Resources.Templates; }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-19
        /// </remarks>
        public void SetEditMode()
        {
        }
    }
}