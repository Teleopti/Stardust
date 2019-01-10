using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.SkillPages;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SkillPages
{
	public partial class SkillGeneral : BaseUserControl, IPropertyPage, ISkillGeneralView
    {
        private readonly IAbstractPropertyPages _propertyPages;
		private readonly IRepositoryFactory _repositoryFactory = new RepositoryFactory();
		private SkillGeneralPresenter _presenter;

        protected SkillGeneral()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                errorProvider1.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
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

            IEnumerable<ISkillType> skillTypeList;
            IList<IActivity> activityList;
			IEnumerable<ISkill> skills;
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ISkillTypeRepository skillTypeRepository = new SkillTypeRepository(unitOfWork);
                skillTypeList = skillTypeRepository.LoadAll();
				ISkillRepository rep = _repositoryFactory.CreateSkillRepository(unitOfWork);
                skills = rep.LoadAll();

	            using (unitOfWork.DisableFilter(QueryFilter.Deleted))
	            {
		            IActivityRepository activityRepository = _repositoryFactory.CreateActivityRepository(unitOfWork);
		            activityList = activityRepository.LoadAllSortByName();
	            }
            }

			_presenter = new SkillGeneralPresenter(this, skill, activityList, skills);
        	_presenter.SetActivitiesList();

            comboBoxSkillType.DataSource = skillTypeList.OrderBy(st => st.Description.Name).ToList();
            comboBoxSkillType.DisplayMember = "Description";

            comboBoxTimeZones.DisplayMember = "DisplayName";
            foreach (TimeZoneInfo timeZoneInfo in TimeZoneInfo.GetSystemTimeZones())
            {
                TimeZoneInfo tzi = ((timeZoneInfo));
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
	        if (skill.SkillType.ForecastSource.Equals(ForecastSource.Chat))
            {
                numericUpDownMaxParallel.Visible = true;
                label1.Visible = true;
		        numericUpDownMaxParallel.Enabled = true;
		        numericUpDownMaxParallel.Value = skill.MaxParallelTasks;
	        }
	        else
	        {
	            numericUpDownMaxParallel.Visible = false;
	            label1.Visible = false;
	        }

			if (skill.SkillType.ForecastSource.Equals(ForecastSource.Chat) || skill.SkillType.ForecastSource.Equals(ForecastSource.InboundTelephony))
			{
				percentTextBoxAbandonRate.Visible = true;
				labelAbandonRate.Visible = true;
				percentTextBoxAbandonRate.Enabled = true;
				percentTextBoxAbandonRate.DoubleValue = skill.AbandonRate.Value;
				percentIntervalAbandonRate.Visible = true;
			}
			else
			{
				percentTextBoxAbandonRate.Visible = false;
				labelAbandonRate.Visible = false;
				percentIntervalAbandonRate.Visible = false;
			}
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

            comboBoxAdvIntervalLength.DataSource = intervalLengths;
            comboBoxAdvIntervalLength.DisplayMember = "Text";
            IntervalLengthItem selectedIntervalLengthItem =
                intervalLengths.FirstOrDefault(length => length.Minutes == defaultLength);
            comboBoxAdvIntervalLength.SelectedItem = selectedIntervalLengthItem;
        }

        private bool isResolutionValid(ISkill skill)
        {
            var childSkill = skill as IChildSkill;
            if (childSkill != null)
            {
                var expectedResolution = childSkill.ParentSkill.DefaultResolution;
                var selectedIntervalLengthItem = (IntervalLengthItem) comboBoxAdvIntervalLength.SelectedItem;
                var resolution = selectedIntervalLengthItem.Minutes;
                if (resolution != expectedResolution)
                {
                    errorProvider1.SetError(comboBoxAdvIntervalLength,
                                            string.Format(TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture,
                                                UserTexts.Resources.
                                                    TheIntervalLengthOfThisSubskillShouldBeTheSameAsItsParentSkillCommaWhichIsParameterMinutesDot,
                                                expectedResolution));
                    return false;
                }
                errorProvider1.Clear();
            }
            return true;
        }

	    public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            var thisSkill = aggregateRoot as ISkill;
            try
            {
				thisSkill.ChangeName(textBoxName.Text.Trim());
            }
            catch (ArgumentException)
            {
                ViewBase.ShowErrorMessage(UserTexts.Resources.SkillNameIsInvalid, UserTexts.Resources.Skill);
                return false;
            }
            if (!isResolutionValid(thisSkill)) return false;
            var activity = (IActivity)comboBoxSkillActivity.SelectedItem;
            if (activity == null)
            {
                ViewBase.ShowErrorMessage(UserTexts.Resources.ActivityCanNotBeEmptyDot, UserTexts.Resources.Skill);
                return false;
            }
            thisSkill.Activity = activity;
            var selectedIntervalLengthItem = (IntervalLengthItem)comboBoxAdvIntervalLength.SelectedItem;
            int resolution = selectedIntervalLengthItem.Minutes;
            thisSkill.DefaultResolution = resolution;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ISkillRepository rep = _repositoryFactory.CreateSkillRepository(uow);
                var skills = rep.LoadAll();
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
            var multisiteSkill = thisSkill as IMultisiteSkill;
            if (multisiteSkill != null)
            {
                foreach (var childSkill in multisiteSkill.ChildSkills)
                {
                    childSkill.Activity = activity;
                }
            }
            thisSkill.TimeZone = (TimeZoneInfo)comboBoxTimeZones.SelectedItem;
            thisSkill.MidnightBreakOffset = office2007OutlookTimePickerMidnightOffsetBreak.TimeValue();
		    thisSkill.MaxParallelTasks = (int)numericUpDownMaxParallel.Value;
			
			thisSkill.AbandonRate = new Percent(percentTextBoxAbandonRate.DoubleValue);
			

			if (office2007OutlookTimePickerMidnightOffsetBreak.Enabled)
			{
				createDefaultTemplates(thisSkill);
			}

            return true;
        }

		private static void createDefaultTemplates(ISkill thisSkill)
		{
			var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date.Add(thisSkill.MidnightBreakOffset), thisSkill.TimeZone);
			var timePeriod = new DateTimePeriod(startDateUtc, startDateUtc.AddDays(1));
			foreach (var template in thisSkill.TemplateWeekCollection)
			{
				var templateSkillDataPeriods =
					template.Value.TemplateSkillDataPeriodCollection.Select(
						skillDataPeriod =>
						new TemplateSkillDataPeriod(skillDataPeriod.ServiceAgreement, skillDataPeriod.SkillPersonData, timePeriod))
					        .Cast<ITemplateSkillDataPeriod>()
					        .ToList();

				template.Value.SetSkillDataPeriodCollection(new ReadOnlyCollection<ITemplateSkillDataPeriod>(templateSkillDataPeriods));
			}

			var multisiteSkill = thisSkill as IMultisiteSkill;
			if (multisiteSkill == null) return;

			foreach (var template in multisiteSkill.TemplateMultisiteWeekCollection.Values)
			{
				template.SetMultisitePeriodCollection(new List<ITemplateMultisitePeriod>
					{
						new TemplateMultisitePeriod(timePeriod, new Dictionary<IChildSkill, Percent>())
					});
			}
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
	        textBoxName.Focus();
        }

        private void comboBoxSkillActivity_SelectedIndexChanged(object sender, EventArgs e)
        {
			_presenter.OnActivityChanged();
        }

        private void AdjustWidthComboBox_DropDown(object sender, EventArgs e)
        {
            var senderComboBox = (ComboBoxAdv)sender;
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

		private void label2_Click(object sender, EventArgs e)
		{

		}
	}
}
