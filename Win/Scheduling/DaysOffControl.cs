using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class DaysOffControl : BaseUserControl, ISettingPage
    {
        private List<IDayOffTemplate> _dayOffList;
        private const short InvalidItemIndex = -1;
        private const short FirstItemIndex = 0;
        private const short ItemDiffernce = 1;
        private const short ShortNameMaxLength = 2;
        private readonly ILocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

        public int LastItemIndex
        {
            get
            {
                return comboBoxAdvDaysOffCollection.Items.Count - ItemDiffernce;
            }
        }

        public IDayOffTemplate SelectedDayOff
        {
            get
            {
                return comboBoxAdvDaysOffCollection.SelectedItem as IDayOffTemplate;
            }
        }

        public IDayOffTemplateRepository Repository { get; private set; }

        public IUnitOfWork UnitOfWork { get; private set; }

        public DaysOffControl()
        {
            InitializeComponent();

            if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.PayrollIntegration))
			{
				autoLabelPayrollCodeColon.Visible = false;
				textBoxExtPayrollCode.Visible = false;
			}
			else
			{
				autoLabelPayrollCodeColon.Visible = true;
				textBoxExtPayrollCode.Visible = true;
				textBoxExtPayrollCode.Validated += TextBoxExtPayrollCodeValidated;
			}
            textBoxExtShortName.MaxLength = ShortNameMaxLength;
            comboBoxAdvDaysOffCollection.SelectedIndexChanging += ComboBoxAdvDaysOffCollectionSelectedIndexChanging;
            comboBoxAdvDaysOffCollection.SelectedIndexChanged += ComboBoxAdvDaysOffCollectionSelectedIndexChanged;
            textBoxDescription.Validating += TextBoxDescriptionValidating;
			textBoxDescription.Validated +=TextBoxDescriptionValidated;
			textBoxExtShortName.Validated +=TextBoxExtShortNameValidated;
			timeSpanTextBoxAnchor.Validated += TimeSpanTextBoxAnchorValidated;
			timeSpanTextBoxFlexibility.Validated += TimeSpanTextBoxFlexibilityValidated;
			timeSpanTextBoxTargetLength.Validated += TimeSpanTextBoxTargetLengthValidated;
			buttonNew.Click += ButtonNewClick;
			buttonDelete.Click += ButtonDeleteClick;
        }

		private void ComboBoxAdvDaysOffCollectionSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
        {
            e.Cancel = !IsWithinRange(e.NewIndex);
        }

        private void ComboBoxAdvDaysOffCollectionSelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedDayOff != null)
            {
                Cursor.Current = Cursors.WaitCursor;

                SelectDayOff();

                Cursor.Current = Cursors.Default;
            }
        }

        private void TextBoxDescriptionValidating(object sender, CancelEventArgs e)
        {
			if (SelectedDayOff != null)
			{
				e.Cancel = !ValidatDayOffDescription();
			}
        }

        private void TextBoxDescriptionValidated(object sender, EventArgs e)
        {
			ChangeDayOffTemplate();
        }

        private void TextBoxExtShortNameValidated(object sender, EventArgs e)
        {
            ChangeDayOffTemplate();
        }

		private void TextBoxExtPayrollCodeValidated(object sender, EventArgs e)
		{
			ChangeDayOffTemplate();
		}

		/// <summary>
		/// Handles the Validated event of the timeSpanTextBoxTargetLength control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void TimeSpanTextBoxTargetLengthValidated(object sender, EventArgs e)
		{
            ChangeDayOffTemplate();
		}

		private void TimeSpanTextBoxFlexibilityValidated(object sender, EventArgs e)
		{
            ChangeDayOffTemplate();
		}

		private void TimeSpanTextBoxAnchorValidated(object sender, EventArgs e)
		{
            ChangeDayOffTemplate();
		}

        private void ButtonNewClick(object sender, EventArgs e)
        {
			if (SelectedDayOff != null)
			{
				Cursor.Current = Cursors.WaitCursor;

				AddNewDayOff();

				Cursor.Current = Cursors.Default;
			}
        }

		private void ButtonDeleteClick(object sender, EventArgs e)
		{
			if (SelectedDayOff != null)
			{
				string text = string.Format(
					CurrentCulture,
					Resources.AreYouSureYouWantToDeleteDayOff,
					SelectedDayOff.Description
					);

				string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

				DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);

				if (response == DialogResult.Yes)
				{
					Cursor.Current = Cursors.WaitCursor;
					DeleteDaysOff();
					Cursor.Current = Cursors.Default;
				}
			}
		}

        private DayOffTemplate CreateDayOff()
        {
			Description description = PageHelper.CreateNewName(_dayOffList, "Description.Name", Resources.NewDayOff);

			var newDayOff = new DayOffTemplate(description) {Description = description, Anchor = new TimeSpan(12, 0, 0)};

            // Defaults as ruled by SPI 8807.
        	newDayOff.SetTargetAndFlexibility(new TimeSpan(24, 0, 0), new TimeSpan(6, 0, 0));

			Repository.Add(newDayOff);

			return newDayOff;
        }

        private void AddNewDayOff()
        {
			_dayOffList.Add(CreateDayOff());
			LoadDayOffs();

            comboBoxAdvDaysOffCollection.SelectedIndex = LastItemIndex;
        }

        private void DeleteDaysOff()
        {
            if (SelectedDayOff != null)
            {
				Repository.Remove(SelectedDayOff);
                _dayOffList.Remove(SelectedDayOff);

                LoadDayOffs();
            }
        }

        private void LoadDayOffs()
        {
            if (Repository != null)  //we are unloading the form
            {
                if (_dayOffList == null)
                {
                    _dayOffList = new List<IDayOffTemplate>(Repository.FindAllDayOffsSortByDescription());
			    }

                if (_dayOffList.IsEmpty())
                {
                    _dayOffList.Add(CreateDayOff());
                }

                int selected = comboBoxAdvDaysOffCollection.SelectedIndex;

                if (!IsWithinRange(selected))
                {
                    selected = FirstItemIndex;
                }

                // Bind the day of list to the combo box
                comboBoxAdvDaysOffCollection.DataSource = null;
                comboBoxAdvDaysOffCollection.DisplayMember = "Description.Name";
                comboBoxAdvDaysOffCollection.DataSource = _dayOffList;

                comboBoxAdvDaysOffCollection.SelectedIndex = selected;
            }
        }

        private void SelectDayOff()
        {
            if (SelectedDayOff != null)
            {
                // Displays the data off data
				textBoxDescription.Text = SelectedDayOff.Description.Name;
				textBoxExtShortName.Text = SelectedDayOff.Description.ShortName;
				textBoxExtPayrollCode.Text = SelectedDayOff.PayrollCode;

				timeSpanTextBoxAnchor.Text = SelectedDayOff.Anchor.ToString();
				timeSpanTextBoxFlexibility.Text = SelectedDayOff.Flexibility.ToString();
				timeSpanTextBoxTargetLength.Text = SelectedDayOff.TargetLength.ToString();

				timeSpanTextBoxAnchor.SetInitialResolution(SelectedDayOff.Anchor);
				timeSpanTextBoxFlexibility.SetInitialResolution(SelectedDayOff.Flexibility);
				timeSpanTextBoxTargetLength.SetInitialResolution(SelectedDayOff.TargetLength);

                string changed = _localizer.UpdatedByText(SelectedDayOff, Resources.UpdatedByColon);
                autoLabelInfoAboutChanges.Text = changed;
            }
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

            gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
            labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

            tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
            autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
        }

        private void ChangeDayOffTemplate()
        {
            if (SelectedDayOff != null)
            {
                if (!string.Equals(SelectedDayOff.Description.Name, textBoxDescription.Text, StringComparison.CurrentCulture))
                {
                    SelectedDayOff.Description = new Description(textBoxDescription.Text, SelectedDayOff.Description.ShortName);
                }

                if (!string.Equals(SelectedDayOff.Description.ShortName, textBoxExtShortName.Text, StringComparison.CurrentCulture))
                {
                    SelectedDayOff.Description = new Description(SelectedDayOff.Description.Name, textBoxExtShortName.Text);
                }

                if (SelectedDayOff.Anchor != timeSpanTextBoxAnchor.Value)
                {
                    SelectedDayOff.Anchor = timeSpanTextBoxAnchor.Value;
                }

                if (SelectedDayOff.TargetLength != timeSpanTextBoxTargetLength.Value)
                {
                    SelectedDayOff.SetTargetAndFlexibility(timeSpanTextBoxTargetLength.Value, timeSpanTextBoxFlexibility.Value);
                }

                if (SelectedDayOff.Flexibility != timeSpanTextBoxFlexibility.Value)
                {
                    SelectedDayOff.SetTargetAndFlexibility(timeSpanTextBoxTargetLength.Value, timeSpanTextBoxFlexibility.Value);
                }

				if (!string.Equals(SelectedDayOff.PayrollCode, textBoxExtPayrollCode.Text, StringComparison.CurrentCulture))
				{
					SelectedDayOff.PayrollCode = textBoxExtPayrollCode.Text;
				}

                LoadDayOffs();
            }
        }

        private bool ValidatDayOffDescription()
        {
			bool failed = string.IsNullOrEmpty(textBoxDescription.Text);

            if (failed)
            {
				textBoxDescription.Text = SelectedDayOff.Description.Name;
            }

            return !failed;
        }

        private bool IsWithinRange(int index)
        {
            return
				index > InvalidItemIndex && index < _dayOffList.Count && 
				comboBoxAdvDaysOffCollection.DataSource != null;
        }

		protected override void SetCommonTexts()
        {
            base.SetCommonTexts();

            toolTip1.SetToolTip(buttonDelete, Resources.DeleteDayOff);
            toolTip1.SetToolTip(buttonNew, Resources.NewDayOff);
        }

        public void InitializeDialogControl()
        {
            SetColors();
            SetTexts();
        }

        public void LoadControl()
        {
            LoadDayOffs();
        }

        public void SaveChanges()
        {}

        public void Unload()
        {
            _dayOffList = null;
            Repository = null;
        }

        public TreeFamily TreeFamily()
        {
            return new TreeFamily(Resources.Scheduling);
        }

        public string TreeNode()
        {
            return Resources.DaysOff;
        }

    	public void OnShow()
    	{
    	}

        public void SetUnitOfWork(IUnitOfWork value)
        {
            UnitOfWork = value;

            // Creates a new repository.
            Repository = new DayOffTemplateRepository(UnitOfWork);
        }

        public void Persist()
        {}

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
            throw new NotImplementedException();
        }

        public ViewType ViewType
        {
            get { return ViewType.DaysOff; }
        }
    }
}
