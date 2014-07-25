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
		private const short invalidItemIndex = -1;
		private const short firstItemIndex = 0;
		private const short itemDiffernce = 1;
		private const short shortNameMaxLength = 2;
		private readonly ILocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

		public int LastItemIndex
		{
			get
			{
				return comboBoxAdvDaysOffCollection.Items.Count - itemDiffernce;
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
				textBoxExtPayrollCode.Validated += textBoxExtPayrollCodeValidated;
			}
			textBoxExtShortName.MaxLength = shortNameMaxLength;
			comboBoxAdvDaysOffCollection.SelectedIndexChanging += comboBoxAdvDaysOffCollectionSelectedIndexChanging;
			comboBoxAdvDaysOffCollection.SelectedIndexChanged += comboBoxAdvDaysOffCollectionSelectedIndexChanged;
			textBoxDescription.Validating += textBoxDescriptionValidating;
			textBoxDescription.Validated +=textBoxDescriptionValidated;
			textBoxExtShortName.Validated +=textBoxExtShortNameValidated;
			timeSpanTextBoxAnchor.Validated += timeSpanTextBoxAnchorValidated;
			timeSpanTextBoxFlexibility.Validated += timeSpanTextBoxFlexibilityValidated;
			timeSpanTextBoxTargetLength.Validated += timeSpanTextBoxTargetLengthValidated;
			buttonNew.Click += buttonNewClick;
			buttonDelete.Click += buttonDeleteClick;
		}

		private void comboBoxAdvDaysOffCollectionSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !isWithinRange(e.NewIndex);
		}

		private void comboBoxAdvDaysOffCollectionSelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedDayOff != null)
			{
				Cursor.Current = Cursors.WaitCursor;

				selectDayOff();

				Cursor.Current = Cursors.Default;
			}
		}

		private void textBoxDescriptionValidating(object sender, CancelEventArgs e)
		{
			if (SelectedDayOff != null)
			{
				e.Cancel = !validatDayOffDescription();
			}
		}

		private void textBoxDescriptionValidated(object sender, EventArgs e)
		{
			changeDayOffTemplate();
		}

		private void textBoxExtShortNameValidated(object sender, EventArgs e)
		{
			changeDayOffTemplate();
		}

		private void textBoxExtPayrollCodeValidated(object sender, EventArgs e)
		{
			changeDayOffTemplate();
		}

		private void timeSpanTextBoxTargetLengthValidated(object sender, EventArgs e)
		{
			changeDayOffTemplate();
		}

		private void timeSpanTextBoxFlexibilityValidated(object sender, EventArgs e)
		{
			changeDayOffTemplate();
		}

		private void timeSpanTextBoxAnchorValidated(object sender, EventArgs e)
		{
			changeDayOffTemplate();
		}

		private void buttonNewClick(object sender, EventArgs e)
		{
			if (SelectedDayOff != null)
			{
				Cursor.Current = Cursors.WaitCursor;

				addNewDayOff();

				Cursor.Current = Cursors.Default;
			}
		}

		private void buttonDeleteClick(object sender, EventArgs e)
		{
		    if (SelectedDayOff == null) return;
		    var text = string.Format(
		        CurrentCulture,
		        Resources.AreYouSureYouWantToDeleteDayOff,
		        SelectedDayOff.Description
		        );

		    var caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

		    var response = ViewBase.ShowConfirmationMessage(text, caption);

		    if (response != DialogResult.Yes) return;
		    Cursor.Current = Cursors.WaitCursor;
		    deleteDaysOff();
		    Cursor.Current = Cursors.Default;
		}

		private DayOffTemplate createDayOff()
		{
			var description = PageHelper.CreateNewName(_dayOffList, "Description.Name", Resources.NewDayOff);

			var newDayOff = new DayOffTemplate(description) {Description = description, Anchor = new TimeSpan(12, 0, 0)};

			// Defaults as ruled by SPI 8807.
			newDayOff.SetTargetAndFlexibility(new TimeSpan(24, 0, 0), new TimeSpan(6, 0, 0));

			Repository.Add(newDayOff);

			return newDayOff;
		}

		private void addNewDayOff()
		{
			_dayOffList.Add(createDayOff());
			loadDayOffs();

			comboBoxAdvDaysOffCollection.SelectedIndex = LastItemIndex;
		}

		private void deleteDaysOff()
		{
			if (SelectedDayOff != null)
			{
				Repository.Remove(SelectedDayOff);
				_dayOffList.Remove(SelectedDayOff);

				loadDayOffs();
			}
		}

		private void loadDayOffs()
		{
		    if (Repository == null) return;
		    if (_dayOffList == null)
		    {
		        _dayOffList = new List<IDayOffTemplate>(Repository.FindAllDayOffsSortByDescription());
		    }

		    if (_dayOffList.IsEmpty())
		    {
		        _dayOffList.Add(createDayOff());
		    }

		    var selected = comboBoxAdvDaysOffCollection.SelectedIndex;

		    if (!isWithinRange(selected))
		    {
		        selected = firstItemIndex;
		    }

		    // Bind the day of list to the combo box
		    comboBoxAdvDaysOffCollection.DataSource = null;
		    comboBoxAdvDaysOffCollection.DisplayMember = "Description.Name";
		    comboBoxAdvDaysOffCollection.DataSource = _dayOffList;

		    comboBoxAdvDaysOffCollection.SelectedIndex = selected;
		}

		private void selectDayOff()
		{
		    if (SelectedDayOff == null) return;
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

		    var changed = _localizer.UpdatedByText(SelectedDayOff, Resources.UpdatedByColon);
		    autoLabelInfoAboutChanges.Text = changed;
		}

		private void setColors()
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

		private void changeDayOffTemplate()
		{
		    if (SelectedDayOff == null) return;
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

		    loadDayOffs();
		}

		private bool validatDayOffDescription()
		{
			bool failed = string.IsNullOrEmpty(textBoxDescription.Text);

			if (failed)
			{
				textBoxDescription.Text = SelectedDayOff.Description.Name;
			}

			return !failed;
		}

		private bool isWithinRange(int index)
		{
			return
				index > invalidItemIndex && index < _dayOffList.Count && 
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
			setColors();
			SetTexts();
		}

		public void LoadControl()
		{
			loadDayOffs();
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
