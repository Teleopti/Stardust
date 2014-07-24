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
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class AvailabilityPage : BaseUserControl, ISettingPage
	{
		private const short InvalidItemIndex = -1;                  // Index of combo when none selected.
		private const short FirstItemIndex = 0;                     // Index of the 1st item of the combo.
		private const short ItemDiffernce = 1;                      // Represents items different.

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
			get { return comboBoxAdvAvailabilities.Items.Count - ItemDiffernce; }
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
			SetColors();
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
			CreateColumns();
		}

		public void LoadControl()
		{
			LoadAvailabilities();
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
			textBoxDescription.Validated -= TextBoxDescriptionValidated;
			textBoxDescription.Validating -= TextBoxDescriptionValidating;
			numericUpDownWeek.ValueChanged -= NumericUpDownWeekValueChanged;

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
			Repository = new AvailabilityRepository(UnitOfWork);
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
			comboBoxAdvAvailabilities.SelectedIndexChanging += ComboBoxAdvAvailabilitiesSelectedIndexChanging;
			comboBoxAdvAvailabilities.SelectedIndexChanged += ComboBoxAdvAvailabilitiesSelectedIndexChanged;
			textBoxDescription.Validating += TextBoxDescriptionValidating;
			textBoxDescription.Validated += TextBoxDescriptionValidated;
			buttonNew.Click += ButtonNewClick;
			buttonDelete.Click += ButtonDeleteClick;
			//
			// numericUpDownWeek
			//
			numericUpDownWeek.Minimum = FirstItemIndex + ItemDiffernce;
			numericUpDownWeek.ValueChanged += NumericUpDownWeekValueChanged;
		}

		private void ChangedInfo()
		{
			autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
			autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
			string changed = _localizer.UpdatedByText(SelectedAvailability, UserTexts.Resources.UpdatedByColon);
			autoLabelInfoAboutChanges.Text = changed;
		}

		private void ChangeToOverMidnight()
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
			RefreshRange();
		}

	    private static bool lateEndTimeWillBeValid(AvailabilityRestrictionView view)
	    {
	        return view.LateEndTime.HasValue && view.LateEndTime.Value < TimeSpan.FromDays(1);
	    }

	    private void RefreshRange()
		{
			int colIndex = _gridColumns.IndexOf(_lateEndTimeColumn);

			gridControlAvailability.RefreshRange(GridRangeInfo.Col(colIndex));
		}

		private void SelectAvailability()
		{
			numericUpDownWeek.Value = ScheduleRestrictionBaseView.GetWeek(SelectedAvailability.AvailabilityDays.Count - ItemDiffernce);
			textBoxDescription.Text = SelectedAvailability.Name;
			SaveChanges();
			PrepareGridView();
		}

		private void InitGrid()
		{
			gridControlAvailability.Rows.HeaderCount = 0;
			gridControlAvailability.Cols.HeaderCount = 0;

			_gridColumns = null;

			gridControlAvailability.SaveCellInfo += gridControlAvailability_SaveCellInfo;
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
		void gridControlAvailability_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			gridControlAvailability.Invalidate();
		}

		private void CreateColumns()
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
		private void PrepareGridView()
		{
			_restrictionViewList.Clear();

			_restrictionViewList.AddRange(ScheduleRestrictionBaseView.Parse(SelectedAvailability));

			_gridHelper = new SFGridColumnGridHelper<AvailabilityRestrictionView>(
				gridControlAvailability,
				_gridColumns,
				_restrictionViewList
				) {AllowExtendedCopyPaste = true};

			// HACK: Handles event to get rid of fxcop.
			_gridHelper.NewSourceEntityWanted += gridHelper_NewSourceEntityWanted;
			_gridHelper.PasteFromClipboardFinished += GridHelperPasteFromClipboardFinished;
		}

		private void ChangeAvailabilityDays()
		{
			int weekCount = ScheduleRestrictionBaseView.GetWeek(SelectedAvailability.DaysCount - ItemDiffernce);
			decimal value = numericUpDownWeek.Value;

			SaveChanges();

			if (value > weekCount)
			{
				// Add days to collection.
				int daysToAdd = ((int)value - weekCount) * (int)ScheduleRestrictionBaseView.DaysPerWeek;
				SelectedAvailability.AddDays(daysToAdd);

				PrepareGridView();
			}
			else if (value < weekCount)
			{
				// Remove days from collection.
				int daysToRemove = (weekCount - (int)value) * (int)ScheduleRestrictionBaseView.DaysPerWeek;
				SelectedAvailability.RemoveDays(daysToRemove);

				PrepareGridView();
			}
		}

		private void AddNewAvailability()
		{
			SaveChanges();
			IAvailabilityRotation newAvailability = CreateAvailability();
			_availabilityList.Add(newAvailability);

			LoadAvailabilities();

			comboBoxAdvAvailabilities.SelectedIndex = LastItemIndex;
		}

		private void DeleteAvailability()
		{
			if (SelectedAvailability == null) return;
			// Marks for remove.
			Repository.Remove(SelectedAvailability);
			// Removes from list.
			_availabilityList.Remove(SelectedAvailability);

			LoadAvailabilities();
		}

		private bool ValidateAvailabilityDescription()
		{
			bool failed = string.IsNullOrEmpty(textBoxDescription.Text);
			if (failed)
			{
				textBoxDescription.SelectedText = SelectedAvailability.Name;
			}

			return !failed;
		}

		private void ChangeAvailabilityDescription()
		{
			SelectedAvailability.Name = textBoxDescription.Text;
			SaveChanges();
			LoadAvailabilities();
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

			gridControlAvailability.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControlAvailability.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
		}

		private void LoadAvailabilities()
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
			if (!IsWithinRange(selected)) selected = FirstItemIndex;

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

	   private bool IsWithinRange(int index)
		{
			return index > InvalidItemIndex && index < _availabilityList.Count && comboBoxAdvAvailabilities.DataSource != null;
		}

		private IAvailabilityRotation CreateAvailability()
		{
			// Formats the name.
			Description description = PageHelper.CreateNewName(_availabilityList, "Name", UserTexts.Resources.NewAvailability);
			IAvailabilityRotation newAvailability = new AvailabilityRotation(description.Name, (int)ScheduleRestrictionBaseView.DaysPerWeek);

			Repository.Add(newAvailability);

			return newAvailability;
		}

		private void TextBoxDescriptionValidating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (SelectedAvailability != null)
			{
				e.Cancel = !ValidateAvailabilityDescription();
			}
		}

		private void TextBoxDescriptionValidated(object sender, EventArgs e)
		{
			if (SelectedAvailability != null)
			{
				ChangeAvailabilityDescription();
			}
		}

		private void ComboBoxAdvAvailabilitiesSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !IsWithinRange(e.NewIndex);
		}

		private void ComboBoxAdvAvailabilitiesSelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedAvailability == null) return;
			Cursor.Current = Cursors.WaitCursor;
			SelectAvailability();
			ChangedInfo();
			Cursor.Current = Cursors.Default;
		}

		private void gridHelper_NewSourceEntityWanted(object sender, SFGridColumnGridHelperEventArgs<AvailabilityRestrictionView> e)
		{
		}

	   private void GridHelperPasteFromClipboardFinished(object sender, EventArgs e)
		{
			bool columnSelected = _gridHelper.HasColumnSelected(_earlyStartTimeColumn);
			if (columnSelected)
			{
				RefreshRange();
			}
		}

	   private void NumericUpDownWeekValueChanged(object sender, EventArgs e)
		{
		if (SelectedAvailability == null) return;
		Cursor.Current = Cursors.WaitCursor;
		ChangeAvailabilityDays();
		Cursor.Current = Cursors.Default;
		}

		private void ButtonNewClick(object sender, EventArgs e)
		{
			if (SelectedAvailability == null) return;
			Cursor.Current = Cursors.WaitCursor;
			AddNewAvailability();
			Cursor.Current = Cursors.Default;
		}

		private void ButtonDeleteClick(object sender, EventArgs e)
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
			DeleteAvailability();
			Cursor.Current = Cursors.Default;
		}

		private void ButtonAdvOvernightClick(object sender, EventArgs e)
		{
			if (SelectedAvailability == null) return;
			Cursor.Current = Cursors.WaitCursor;
			ChangeToOverMidnight();
			Cursor.Current = Cursors.Default;
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			LoadControl();

			var selectedAvailability = entity.SelectedEntityObject as IAvailabilityRotation;
			if (selectedAvailability == null || !_availabilityList.Contains(selectedAvailability)) return;
			var index = _availabilityList.IndexOf(selectedAvailability);

			comboBoxAdvAvailabilities.SelectedIndex = index;
		}

		public ViewType ViewType
		{
			get { return ViewType.Availability; }
		}
	}
}
