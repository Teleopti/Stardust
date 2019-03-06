using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class AvailabilityPage : BaseUserControl, ISettingPage
	{
		private const short invalidItemIndex = -1;                  // Index of combo when none selected.
		private const short firstItemIndex = 0;                     // Index of the 1st item of the combo.
		private const short itemDifference = 1;                      // Represents items different.

		private IList<IAvailabilityRotation> _availabilityList;
		private List<AvailabilityRestrictionView> _restrictionViewList = new List<AvailabilityRestrictionView>();

		// Columns that need special attention.
		private SFGridColumnBase<AvailabilityRestrictionView> _lateEndTimeColumn;
		private SFGridColumnBase<AvailabilityRestrictionView> _earlyStartTimeColumn;

		private SFGridColumnGridHelper<AvailabilityRestrictionView> _gridHelper;
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
		private ReadOnlyCollection<SFGridColumnBase<AvailabilityRestrictionView>> _gridColumns;

		public IUnitOfWork UnitOfWork { get; private set; }

		public AvailabilityRepository Repository { get; private set; }

		private int LastItemIndex
		{
			get { return comboBoxAdvAvailabilities.Items.Count - itemDifference; }
		}

		public IAvailabilityRotation SelectedAvailability
		{
			get { return (IAvailabilityRotation)comboBoxAdvAvailabilities.SelectedItem; }
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();

			toolTip1.SetToolTip(buttonDelete, UserTexts.Resources.DeleteAvailability);
			toolTip1.SetToolTip(buttonNew, UserTexts.Resources.NewAvailability);
			toolTip1.SetToolTip(buttonAdvOvernight, UserTexts.Resources.ChangeToOverMidnight);
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();

			// Set HourMinutes to CellModels.
			gridControlAvailability.CellModels.Add(
				"HourMinutesEmpty",
				new TimeSpanDurationCellModel(gridControlAvailability.Model){AllowEmptyCell = true}
				);

			gridControlAvailability.CellModels.Add(
				"TimeOfDayCell",
				new TimeSpanTimeOfDayCellModel(gridControlAvailability.Model){AllowEmptyCell = true}
				);

			InitGrid();
			createColumns();
		}

		public void LoadControl()
		{
			loadAvailabilities();
		}

		public void SaveChanges()
		{
			if (_gridHelper == null)
				return;

			if(_gridHelper.ValidateGridData())
			{
				foreach(AvailabilityRestrictionView availabilityRestriction in _restrictionViewList)
				{
					availabilityRestriction.AssignValuesToDomainObject();
				}
			}
			else
			{
				throw new ValidationException(UserTexts.Resources.Availabilities);
			}
		
		}

		public void Unload()
		{
			// Disposes or flag anything possible.
			textBoxDescription.Validated -= textBoxDescriptionValidated;
			textBoxDescription.Validating -= textBoxDescriptionValidating;
			numericUpDownWeek.ValueChanged -= numericUpDownWeekValueChanged;

			Repository = null;
			 _availabilityList = null;
			 _restrictionViewList = null;
			_gridColumns = null;
			if (_gridHelper == null) return;
			_gridHelper.NewSourceEntityWanted -= gridHelper_NewSourceEntityWanted;
			_gridHelper = null;
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			UnitOfWork = value;
			// Creates a new repository.
			Repository = AvailabilityRepository.DONT_USE_CTOR(UnitOfWork);
		}

		public void Persist()
		{
			SaveChanges();
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(UserTexts.Resources.Restrictions);
		}

		public string TreeNode()
		{
			return UserTexts.Resources.Availabilities;
		}

		public void OnShow()
		{
		}

		public AvailabilityPage()
		{
			InitializeComponent();

			// Bind events.
			comboBoxAdvAvailabilities.SelectedIndexChanging += comboBoxAdvAvailabilitiesSelectedIndexChanging;
			comboBoxAdvAvailabilities.SelectedIndexChanged += comboBoxAdvAvailabilitiesSelectedIndexChanged;
			textBoxDescription.Validating += textBoxDescriptionValidating;
			textBoxDescription.Validated += textBoxDescriptionValidated;
			buttonNew.Click += buttonNewClick;
			buttonDelete.Click += buttonDeleteClick;

			numericUpDownWeek.Minimum = firstItemIndex + itemDifference;
			numericUpDownWeek.ValueChanged += numericUpDownWeekValueChanged;
		}

		private void changedInfo()
		{
			autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
			autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
			string changed = _localizer.UpdatedByText(SelectedAvailability, UserTexts.Resources.UpdatedByColon);
			autoLabelInfoAboutChanges.Text = changed;
		}

		private void changeToOverMidnight()
		{
			bool lateEndSelected = _gridHelper.HasColumnSelected(_lateEndTimeColumn);
			if (lateEndSelected)
			{
				ICollection<AvailabilityRestrictionView> selectedList = _gridHelper.FindSelectedItems();
				
				foreach (AvailabilityRestrictionView view in selectedList)
				{
					if (lateEndTimeWillBeValid(view))
						view.LateEndTime = view.LateEndTime.Value.Add(TimeSpan.FromDays(1));
				}
			}
			// Refreshes the Grid.
			refreshRange();
		}

		private static bool lateEndTimeWillBeValid(AvailabilityRestrictionView view)
		{
			return view.LateEndTime.HasValue && view.LateEndTime.Value < TimeSpan.FromDays(1);
		}

		private void refreshRange()
		{
			int colIndex = _gridColumns.IndexOf(_lateEndTimeColumn);

			gridControlAvailability.RefreshRange(GridRangeInfo.Col(colIndex));
		}

		private void selectAvailability()
		{
			numericUpDownWeek.Value = ScheduleRestrictionBaseView.GetWeek(SelectedAvailability.AvailabilityDays.Count - itemDifference);
			textBoxDescription.Text = SelectedAvailability.Name;
			SaveChanges();
			prepareGridView();
		}

		private void InitGrid()
		{
			gridControlAvailability.Rows.HeaderCount = 0;
			gridControlAvailability.Cols.HeaderCount = 0;

			_gridColumns = null;

			gridControlAvailability.SaveCellInfo += gridControlAvailabilitySaveCellInfo;
			gridControlAvailability.PrepareViewStyleInfo += gridControlAvailabilityPrepareViewStyleInfo;
			gridControlAvailability.MouseUp += gridControlAvailabilityMouseUp;
			gridControlAvailability.KeyUp += gridControlAvailabilityKeyUp;
		}

		void gridControlAvailabilityPrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
		{
			e.Style.BackColor = e.RowIndex == gridControlAvailability.CurrentCell.RowIndex ? Color.LightGoldenrodYellow : Color.White;
		}

		void gridControlAvailabilityKeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode.Equals(Keys.Down) || e.KeyCode.Equals(Keys.Up) || e.KeyCode.Equals(Keys.Enter))
				gridControlAvailability.Invalidate();
		}

		void gridControlAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			gridControlAvailability.Invalidate();
		}
		void gridControlAvailabilitySaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			gridControlAvailability.Invalidate();
		}

		private void createColumns()
		{
			IList<SFGridColumnBase<AvailabilityRestrictionView>> columnList =
				new List<SFGridColumnBase<AvailabilityRestrictionView>>
					{
						new SFGridRowHeaderColumn<AvailabilityRestrictionView>(string.Empty),
						new SFGridReadOnlyTextColumn<AvailabilityRestrictionView>("Week", 50, UserTexts.Resources.Week),
						new SFGridReadOnlyTextColumn<AvailabilityRestrictionView>("Day", 100, UserTexts.Resources.WeekDay)
					};

			// Grid must have a Header column

			_earlyStartTimeColumn = new SFGridTimeOfDayColumn<AvailabilityRestrictionView>("EarlyStartTime", UserTexts.Resources.EarlyStartTime)
									{
										CellValidator = new EarlyStartTimeCellValidator<AvailabilityRestrictionView>()
									};
			columnList.Add(_earlyStartTimeColumn);

			_lateEndTimeColumn = new SFGridTimeOfDayColumn<AvailabilityRestrictionView>("LateEndTime", UserTexts.Resources.LateEndTime)
									{
										CellValidator = new LateEndTimeCellValidator<AvailabilityRestrictionView>()
									};
			columnList.Add(_lateEndTimeColumn);

			// Working time columns.
			columnList.Add(new SFGridHourMinutesOrEmptyColumn<AvailabilityRestrictionView>("MinimumWorkTime", UserTexts.Resources.MinWorkTime));
			columnList.Add(new SFGridHourMinutesOrEmptyColumn<AvailabilityRestrictionView>("MaximumWorkTime", UserTexts.Resources.MaxWorkTime));

			// Is available column.
			columnList.Add(new SFGridCheckBoxColumn<AvailabilityRestrictionView>("IsAvailable", UserTexts.Resources.Available));

			// Adds column list.
			_gridColumns = new ReadOnlyCollection<SFGridColumnBase<AvailabilityRestrictionView>>(columnList);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void prepareGridView()
		{
			_restrictionViewList.Clear();

			_restrictionViewList.AddRange(ScheduleRestrictionBaseView.Parse(SelectedAvailability));

			_gridHelper = new SFGridColumnGridHelper<AvailabilityRestrictionView>(
				gridControlAvailability,
				_gridColumns,
				_restrictionViewList,
				false) {AllowExtendedCopyPaste = true};

			// HACK: Handles event to get rid of fxcop.
			_gridHelper.NewSourceEntityWanted += gridHelper_NewSourceEntityWanted;
			_gridHelper.PasteFromClipboardFinished += gridHelperPasteFromClipboardFinished;
		}

		private void changeAvailabilityDays()
		{
			int weekCount = ScheduleRestrictionBaseView.GetWeek(SelectedAvailability.DaysCount - itemDifference);
			decimal value = numericUpDownWeek.Value;

			SaveChanges();

			if (value > weekCount)
			{
				// Add days to collection.
				int daysToAdd = ((int)value - weekCount) * (int)ScheduleRestrictionBaseView.DaysPerWeek;
				SelectedAvailability.AddDays(daysToAdd);

				prepareGridView();
			}
			else if (value < weekCount)
			{
				// Remove days from collection.
				int daysToRemove = (weekCount - (int)value) * (int)ScheduleRestrictionBaseView.DaysPerWeek;
				SelectedAvailability.RemoveDays(daysToRemove);

				prepareGridView();
			}
		}

		private void addNewAvailability()
		{
			SaveChanges();
			IAvailabilityRotation newAvailability = CreateAvailability();
			_availabilityList.Add(newAvailability);

			loadAvailabilities();

			comboBoxAdvAvailabilities.SelectedIndex = LastItemIndex;
		}

		private void deleteAvailability()
		{
			if (SelectedAvailability == null) return;
			// Marks for remove.
			Repository.Remove(SelectedAvailability);
			// Removes from list.
			_availabilityList.Remove(SelectedAvailability);

			loadAvailabilities();
		}

		private bool validateAvailabilityDescription()
		{
			bool failed = string.IsNullOrEmpty(textBoxDescription.Text);
			if (failed)
			{
				textBoxDescription.SelectedText = SelectedAvailability.Name;
			}

			return !failed;
		}

		private void changeAvailabilityDescription()
		{
			SelectedAvailability.Name = textBoxDescription.Text;
			SaveChanges();
			loadAvailabilities();
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

			gridControlAvailability.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControlAvailability.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
		}

		private void loadAvailabilities()
		{
			if (Disposing) return;
			if (_availabilityList == null)
			{
				// Loads all availabilities.
				IList<IAvailabilityRotation> unSortedList = Repository.LoadAllAvailabilitiesWithHierarchyData();
				IEnumerable<IAvailabilityRotation> sortedList = (from s in unSortedList
																 orderby s.Name ascending
																 select s);

				_availabilityList = new List<IAvailabilityRotation>(sortedList);
			}
			if (_availabilityList.IsEmpty())
			{
				_availabilityList.Add(CreateAvailability());
			}

			// Removes binding from comboBoxAdvAvailabilities.
			int selected = comboBoxAdvAvailabilities.SelectedIndex;
			var selectedAvailability = comboBoxAdvAvailabilities.SelectedItem as IAvailabilityRotation;
			if (!isWithinRange(selected)) selected = firstItemIndex;

			// Rebinds list to comboBoxAdvAvailabilities.
			comboBoxAdvAvailabilities.DataSource = null;
			comboBoxAdvAvailabilities.DisplayMember = "Name";
			comboBoxAdvAvailabilities.DataSource = _availabilityList;
			if (selectedAvailability != null)
				if (((IDeleteTag)selectedAvailability).IsDeleted != true)
					comboBoxAdvAvailabilities.SelectedItem = selectedAvailability;
				else
					comboBoxAdvAvailabilities.SelectedIndex = selected;
			else
				comboBoxAdvAvailabilities.SelectedIndex = selected;
		}

	   private bool isWithinRange(int index)
		{
			return index > invalidItemIndex && index < _availabilityList.Count && comboBoxAdvAvailabilities.DataSource != null;
		}

		private IAvailabilityRotation CreateAvailability()
		{
			// Formats the name.
			Description description = PageHelper.CreateNewName(_availabilityList, "Name", UserTexts.Resources.NewAvailability);
			IAvailabilityRotation newAvailability = new AvailabilityRotation(description.Name, (int)ScheduleRestrictionBaseView.DaysPerWeek);

			Repository.Add(newAvailability);

			return newAvailability;
		}

		private void textBoxDescriptionValidating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (SelectedAvailability != null)
			{
				e.Cancel = !validateAvailabilityDescription();
			}
		}

		private void textBoxDescriptionValidated(object sender, EventArgs e)
		{
			if (SelectedAvailability != null)
			{
				changeAvailabilityDescription();
			}
		}

		private void comboBoxAdvAvailabilitiesSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !isWithinRange(e.NewIndex);
		}

		private void comboBoxAdvAvailabilitiesSelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedAvailability == null) return;
			Cursor.Current = Cursors.WaitCursor;
			selectAvailability();
			changedInfo();
			Cursor.Current = Cursors.Default;
		}

		private void gridHelper_NewSourceEntityWanted(object sender, SFGridColumnGridHelperEventArgs<AvailabilityRestrictionView> e)
		{
		}

	   private void gridHelperPasteFromClipboardFinished(object sender, EventArgs e)
		{
			bool columnSelected = _gridHelper.HasColumnSelected(_earlyStartTimeColumn);
			if (columnSelected)
			{
				refreshRange();
			}
		}

	   private void numericUpDownWeekValueChanged(object sender, EventArgs e)
		{
		if (SelectedAvailability == null) return;
		Cursor.Current = Cursors.WaitCursor;
		changeAvailabilityDays();
		Cursor.Current = Cursors.Default;
		}

		private void buttonNewClick(object sender, EventArgs e)
		{
			if (SelectedAvailability == null) return;
			Cursor.Current = Cursors.WaitCursor;
			addNewAvailability();
			Cursor.Current = Cursors.Default;
		}

		private void buttonDeleteClick(object sender, EventArgs e)
		{
			if (SelectedAvailability == null) return;
			string text = string.Format(
				CurrentCulture,
				UserTexts.Resources.AreYouSureYouWantToDeleteAvailability,
				SelectedAvailability.Name
				);
			DialogResult response = ViewBase.ShowYesNoMessage(text,UserTexts.Resources.ConfirmDelete);
			if (response != DialogResult.Yes) return;
			Cursor.Current = Cursors.WaitCursor;
			deleteAvailability();
			Cursor.Current = Cursors.Default;
		}

		private void buttonAdvOvernightClick(object sender, EventArgs e)
		{
			if (SelectedAvailability == null) return;
			Cursor.Current = Cursors.WaitCursor;
			changeToOverMidnight();
			Cursor.Current = Cursors.Default;
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{

			var selectedAvailability = entity.SelectedEntityObject as IAvailabilityRotation;
			if (selectedAvailability == null || !_availabilityList.Contains(selectedAvailability)) return;
			var index = _availabilityList.IndexOf(selectedAvailability);

			comboBoxAdvAvailabilities.SelectedIndex = index;
		}

		public ViewType ViewType
		{
			get { return ViewType.Availability; }
		}

		//bugfix #33189: Syncfusion grid crash when cutting ctrl+x
		private void gridControlAvailability_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.X)
				e.Handled = true;
		}
	}
}
