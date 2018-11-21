using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SkillPages
{
    public partial class SkillEmailDistributions : BaseUserControl, IPropertyPage
    {
        private bool _inputContainsError;
       
        public SkillEmailDistributions()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
				this.maskedTimeSpanTextBox1.Size = new System.Drawing.Size(75, 22);
        }

        private void SetChildSkillVisible()
        {
            maskedTimeSpanTextBox1.Visible = false;
            percentTextBox1.Visible = false;
            labelShrinkage.Visible = false;
            labelEfficiencyPercentage.Visible = false;
            efficiencyPercentTextBox1.Visible = false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void timeSpanTextBoxServiceLevelTime_TimeSpanBoxTextChanged(object sender, EventArgs e)
        {
            if (maskedTimeSpanTextBox1.ValidatedStatus == ValidatedStatus.Error)
            {
                _inputContainsError = true;
                return;
            }

            _inputContainsError = false;
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            ISkill skill = aggregateRoot as ISkill;
            ISkillDayTemplate skillDayTemplate = (ISkillDayTemplate) skill.GetTemplate(TemplateTarget.Skill, DayOfWeek.Monday);
            if (skillDayTemplate.TemplateSkillDataPeriodCollection.Count > 0)
            {
                ITemplateSkillDataPeriod skillDataPeriodTemplate = skillDayTemplate.TemplateSkillDataPeriodCollection[0];
                maskedTimeSpanTextBox1.MaximumValue = TimeSpan.FromSeconds(skill.DefaultResolution * TimeDefinition.SecondsPerMinute * TimeDefinition.HoursPerDay); 
                maskedTimeSpanTextBox1.Value = (TimeSpan.FromSeconds(skillDataPeriodTemplate.ServiceAgreement.ServiceLevel.Seconds));
                integerTextBoxMinimumAgents.IntegerValue =
                    skillDataPeriodTemplate.SkillPersonData.MinimumPersons;
                integerTextBoxMaximumAgents.IntegerValue =
                    skillDataPeriodTemplate.SkillPersonData.MaximumPersons;
                percentTextBox1.DoubleValue = skillDataPeriodTemplate.Shrinkage.Value;
                efficiencyPercentTextBox1.DoubleValue = skillDataPeriodTemplate.Efficiency.Value;
            }
            if (skill is IChildSkill)
            {
                SetChildSkillVisible();
            }
            
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            if (_inputContainsError) return false;

            ISkill skill = aggregateRoot as ISkill;

            ServiceLevel serviceLevel = new ServiceLevel(new Percent(1d),
                                                         Convert.ToDouble(
                                                             maskedTimeSpanTextBox1.Value.TotalSeconds,
                                                             CultureInfo.CurrentCulture));
            var serviceAgreement = new ServiceAgreement(serviceLevel,new Percent(), new Percent());
            
            int staffMin = (int) integerTextBoxMinimumAgents.IntegerValue;
            int staffMax = (int) integerTextBoxMaximumAgents.IntegerValue;

            SkillPersonData skillPersonData = new SkillPersonData(staffMin, staffMax);

            Percent shrinkage = new Percent(percentTextBox1.DoubleValue);
            Percent efficiency = new Percent(efficiencyPercentTextBox1.DoubleValue);

            DateTime startDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, skill.TimeZone);
            startDateTime = startDateTime.Add(skill.MidnightBreakOffset);

            DateTime endDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.AddDays(1).Date, skill.TimeZone);
            endDateTime = endDateTime.Add(skill.MidnightBreakOffset);
            DateTimePeriod timePeriod = new DateTimePeriod(startDateTime, endDateTime);

            ITemplateSkillDataPeriod templateSkillDataPeriod = new TemplateSkillDataPeriod(serviceAgreement,
                                                                                          skillPersonData, timePeriod);
            templateSkillDataPeriod.Shrinkage = shrinkage;
            templateSkillDataPeriod.Efficiency = efficiency;
			
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