using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Linq;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.SkillPages;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
	public partial class SkillGeneral : BaseUserControl, IPropertyPage, ISkillGeneralView
    {
        private readonly IAbstractPropertyPages _propertyPages;
        private readonly IRepositoryFactory _repositoryFactory = new RepositoryFactory();
		private SkillGeneralPresenter _presenter;

        protected SkillGeneral()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public SkillGeneral(IAbstractPropertyPages propertyPages)
            : this()
        {
            _propertyPages = propertyPages;
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            var skill = aggregateRoot as ISkill;
            if (skill==null) throw new ArgumentNullException("aggregateRoot","The supplied root must be of type: ISkill.");

            ICollection<ISkillType> skillTypeList;
            IList<IActivity> activityList;
        	IList<ISkill> skills;
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ISkillTypeRepository skillTypeRepository = _repositoryFactory.CreateSkillTypeRepository(unitOfWork);
                skillTypeList = skillTypeRepository.FindAll();
				ISkillRepository rep = _repositoryFactory.CreateSkillRepository(unitOfWork);
                skills = rep.LoadAll();

                // *************************************************************************
                // Warning !!
                // Must be the last repository.
                unitOfWork.DisableFilter(QueryFilter.Deleted);
                IActivityRepository activityRepository = _repositoryFactory.CreateActivityRepository(unitOfWork);
                activityList = activityRepository.LoadAllSortByName();
                // *************************************************************************

            }

			_presenter = new SkillGeneralPresenter(this, skill, activityList, skills);
        	_presenter.SetActivitiesList();

            comboBoxSkillType.DataSource = skillTypeList.OrderBy(st => st.Description.Name).ToList();
            comboBoxSkillType.DisplayMember = "Description";

            comboBoxTimeZones.DisplayMember = "DisplayName";
            foreach (TimeZoneInfo timeZoneInfo in TimeZoneInfo.GetSystemTimeZones())
            {
                ICccTimeZoneInfo tzi = (new CccTimeZoneInfo(timeZoneInfo));
                comboBoxTimeZones.Items.Add(tzi);
                if (skill.TimeZone.Id == tzi.Id)
                    comboBoxTimeZones.SelectedItem = tzi;
            }

            textBoxName.Text = skill.Name;
            textBoxDescription.Text = skill.Description;
            

            pictureBoxDisplayColor.BackColor = skill.DisplayColor;
            if (skill.SkillType == null)
            {
                skill.SkillType = (ISkillType)comboBoxSkillType.SelectedItem;
            }
            comboBoxSkillType.SelectedItem = skill.SkillType;
            if (skill.Activity == null)
            {
                skill.Activity = (IActivity)comboBoxSkillActivity.SelectedItem;
            }
            comboBoxSkillActivity.SelectedItem = skill.Activity;
			comboBoxSkillType.Enabled = false;

            office2007OutlookTimePickerMidnightOffsetBreak.TimeIntervalInDropDown = 60;
            office2007OutlookTimePickerMidnightOffsetBreak.CreateAndBindList(new TimeSpan(0, 0, 0), new TimeSpan(8, 0, 0));
 
            office2007OutlookTimePickerMidnightOffsetBreak.SetTimeValue(skill.MidnightBreakOffset);

            updateTotalOpeningHours();
        }

		public void SetActivityList(IList<IActivity> activities)
		{
			comboBoxSkillActivity.DataSource = activities.OrderBy(act => act.Description.Name).ToList();
			comboBoxSkillActivity.DisplayMember = "Name";
		}

		public void SetResolutionEnableState(bool enabled)
		{
			comboBoxAdvIntervalLength.Enabled = enabled;
		}

		public IActivity SelectedActivity()
		{
			return (IActivity)comboBoxSkillActivity.SelectedItem;
		}

		public void SetSelectedResolution(int resolution)
		{
			initIntervalLengthComboBox(resolution);
		}

		private void initIntervalLengthComboBox(int defaultLength)
        {
            var intervalLengths = new List<IntervalLengthItem>();
           
            intervalLengths.Add(new IntervalLengthItem(5));
            intervalLengths.Add(new IntervalLengthItem(10));
            intervalLengths.Add(new IntervalLengthItem(15));
            intervalLengths.Add(new IntervalLengthItem(30));
            intervalLengths.Add(new IntervalLengthItem(60));
            intervalLengths.Add(new IntervalLengthItem(120));
            intervalLengths.Add(new IntervalLengthItem(180));
            intervalLengths.Add(new IntervalLengthItem(240));
            intervalLengths.Add(new IntervalLengthItem(360));
            intervalLengths.Add(new IntervalLengthItem(480));
            intervalLengths.Add(new IntervalLengthItem(720));
            intervalLengths.Add(new IntervalLengthItem(1440));


            comboBoxAdvIntervalLength.DataSource = intervalLengths;
            comboBoxAdvIntervalLength.DisplayMember = "Text";
            IntervalLengthItem selectedIntervalLengthItem =
                intervalLengths.FirstOrDefault(length => length.Minutes == defaultLength);
            comboBoxAdvIntervalLength.SelectedItem = selectedIntervalLengthItem;
        }

		public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            var thisSkill = aggregateRoot as ISkill;
            try
            {
                thisSkill.Name = textBoxName.Text.Trim();
            }
            catch (ArgumentException)
            {
                MessageBoxAdv.Show(string.Concat(UserTexts.Resources.SkillNameIsInvalid, "  "), "", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0));
                return false;
            }

            var activity = (IActivity)comboBoxSkillActivity.SelectedItem;
            if (activity == null)
            {
                MessageBoxAdv.Show(UserTexts.Resources.ActivityCanNotBeEmptyDot, "", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0));
                return false;
            }
            thisSkill.Activity = activity;
            var selectedIntervalLengthItem = (IntervalLengthItem)comboBoxAdvIntervalLength.SelectedItem;
            int resolution = selectedIntervalLengthItem.Minutes;
            thisSkill.DefaultResolution = resolution;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ISkillRepository rep = _repositoryFactory.CreateSkillRepository(uow);
                IList<ISkill> skills = rep.LoadAll();
                foreach (ISkill skill in skills)
                {
                    if (skill.Activity.Equals(activity))
                    {
                        if (skill.DefaultResolution != thisSkill.DefaultResolution)
                        {
                            MessageDialogs.ShowWarning(this, "xxAllSkillsBasedOnThisActivityMustHaveSameResolution", "xxInvalidResolution");
                            return false;
                        }
                        break;
                    }
                }
            }

            thisSkill.Description = textBoxDescription.Text;
            thisSkill.DisplayColor = pictureBoxDisplayColor.BackColor;
            thisSkill.SkillType = (ISkillType)comboBoxSkillType.SelectedItem;
            thisSkill.Activity = activity;
            thisSkill.TimeZone = (ICccTimeZoneInfo)comboBoxTimeZones.SelectedItem;
            thisSkill.MidnightBreakOffset = office2007OutlookTimePickerMidnightOffsetBreak.TimeValue();

            if (office2007OutlookTimePickerMidnightOffsetBreak.Enabled)
            {
                foreach (KeyValuePair<int, ISkillDayTemplate> template in thisSkill.TemplateWeekCollection)
                {
                    IList<ITemplateSkillDataPeriod> templateSkillDataPeriods = new List<ITemplateSkillDataPeriod>();
                    DateTime startDateUtc =
                        thisSkill.TimeZone.ConvertTimeToUtc(
                            SkillDayTemplate.BaseDate.Date.Add(thisSkill.MidnightBreakOffset), thisSkill.TimeZone);
                    var timePeriod = new DateTimePeriod(startDateUtc, startDateUtc.AddDays(1));

                    foreach (
                        ITemplateSkillDataPeriod skillDataPeriod in template.Value.TemplateSkillDataPeriodCollection)
                    {
                        templateSkillDataPeriods.Add(new TemplateSkillDataPeriod(skillDataPeriod.ServiceAgreement,
                                                                                 skillDataPeriod.SkillPersonData,
                                                                                 timePeriod));
                    }
                    template.Value.SetSkillDataPeriodCollection(
                        new ReadOnlyCollection<ITemplateSkillDataPeriod>(templateSkillDataPeriods));
                }
            }
            return true;
        }

        #region IPropertyPage Members

        public string PageName
        {
            get { return UserTexts.Resources.General; }
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
            comboBoxSkillType.Enabled = false;
            comboBoxTimeZones.Enabled = false;
            comboBoxAdvIntervalLength.Enabled = false;
            office2007OutlookTimePickerMidnightOffsetBreak.Enabled = false;
        }

        #endregion

        private void buttonChangeColor_Click(object sender, EventArgs e)
        {
            switch (colorDialogSkillColor.ShowDialog())
            {
                case DialogResult.OK:
                    {
                        pictureBoxDisplayColor.BackColor = colorDialogSkillColor.Color;
                        break;
                    }
                case DialogResult.Cancel:
                    {
                        break;
                    }
            }
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            var eventArgs = new WizardNameChangedEventArgs(((TextBox)sender).Text);
            _propertyPages.TriggerNameChanged(eventArgs);
        }

        private void comboBoxSkillActivity_SelectedIndexChanged(object sender, EventArgs e)
        {
			_presenter.OnActivityChanged();
        }

        private void AdjustWidthComboBox_DropDown(object sender, EventArgs e)
        {
            var senderComboBox = (ComboBox)sender;
            int width = senderComboBox.DropDownWidth;
            Graphics g = senderComboBox.CreateGraphics();
            Font font = senderComboBox.Font;
            int vertScrollBarWidth =
                (senderComboBox.Items.Count > senderComboBox.MaxDropDownItems)
                ? SystemInformation.VerticalScrollBarWidth : 0;

            foreach (object s in senderComboBox.Items)
            {
                string ss = senderComboBox.GetItemText(s);
                int newWidth = (int)g.MeasureString(ss, font).Width + vertScrollBarWidth;
                if (width < newWidth)
                {
                    width = newWidth;
                }
            }
            senderComboBox.DropDownWidth = width;
        }

        private void office2007OutlookTimePickerMidnightOffsetBreak_SelectedValueChanged(object sender, EventArgs e)
        {
            updateTotalOpeningHours();
        }

        private void updateTotalOpeningHours()
        {
            labelTotalOpeningHours.Text = string.Format(CultureInfo.CurrentCulture, "{0} - {0}", office2007OutlookTimePickerMidnightOffsetBreak.Text);
		}

		
    }
}
