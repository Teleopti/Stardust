using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Rotation = Teleopti.Ccc.Domain.Scheduling.Restriction.Rotation;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class RotationPage : BaseUserControl, ISettingPage
	{
		private const short FirstItemIndex = 0; // Index of the 1st item of the combo.
		private const short InvalidItemIndex = -1; // Index of combo when none selected.
		private const short ItemDiffernce = 1; // Represents items different.

		private readonly List<IDayOffTemplate> _dayOffList = new List<IDayOffTemplate>();

		private readonly IDictionary<RangeCategory, ICollection<GridRangeInfo>> _gridRangeDictionary =
			new Dictionary<RangeCategory, ICollection<GridRangeInfo>>();

		private readonly List<IShiftCategory> _shiftCategoryList = new List<IShiftCategory>();
		private SFGridDropDownColumn<RotationRestrictionView, IDayOffTemplate> _dayOffsColumn;

		private SFGridColumnBase<RotationRestrictionView> _earlyEndTimeColumn;
		private SFGridColumnBase<RotationRestrictionView> _earlyStartTimeColumn;
		private IList<SFGridColumnBase<RotationRestrictionView>> _gridColumns;
		private SFGridColumnGridHelper<RotationRestrictionView> _gridHelper;
		private SFGridColumnBase<RotationRestrictionView> _lateEndTimeColumn;
		private SFGridColumnBase<RotationRestrictionView> _lateStartTimeColumn;
		private IMessageBroker _messageBroker;
		private List<RotationRestrictionView> _restrictionViewList = new List<RotationRestrictionView>();
		private List<IRotation> _rotationList;
		private SFGridDropDownColumn<RotationRestrictionView, IShiftCategory> _shiftCategoriesColumn;
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

		private enum RangeCategory
		{
			TimeColumns,
			ShiftCategories,
			DayOffs
		}
		
		public IUnitOfWork UnitOfWork { get; private set; }

		public RotationRepository Repository { get; private set; }

		private int LastItemIndex
		{
			get { return comboBoxAdvRotations.Items.Count - ItemDiffernce; }
		}

		public IRotation SelectedRotation
		{
			get { return (IRotation) comboBoxAdvRotations.SelectedItem; }
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();

			toolTip1.SetToolTip(buttonDelete, UserTexts.Resources.DeleteRotation);
			toolTip1.SetToolTip(buttonNew, UserTexts.Resources.NewRotation);
		}

		public void InitializeDialogControl()
		{
			SetColors();
			SetTexts();

			InitMessageBroker();

			// Set HourMinutes to CellModels.
		    gridControlRotation.CellModels.Add(
		        "HourMinutesEmpty",
		        new TimeSpanDurationCellModel(gridControlRotation.Model) {AllowEmptyCell = true});

		    gridControlRotation.CellModels.Add(
		        "TimeOfDayCell",
		        new TimeSpanTimeOfDayCellModel(gridControlRotation.Model) {AllowEmptyCell = true});

			InitGrid();
			CreateColumns();
			CreateGridRanges();
		}

		public void LoadControl()
		{
			LoadShiftCategories();
			LoadDayOffs();
			LoadRotations();
		}

		public void SaveChanges()
		{
			if (_gridHelper == null)
				return;

			if (_gridHelper.ValidateGridData())
			{
				foreach (RotationRestrictionView rotationRestrictionView in _restrictionViewList)
				{
					// Connect each rotation view row to its corresponding domain object.
					rotationRestrictionView.AssignValuesToDomainObject();
				}
			}
			else
			{
				throw new ValidationException(UserTexts.Resources.Rotations);
			}
		}

		public void Unload()
		{
			UnregisterMessageBrooker();
			// Disposes or flag anything possible.
			comboBoxAdvRotations.SelectedIndexChanging -= ComboBoxAdvRotationsSelectedIndexChanging;
			comboBoxAdvRotations.SelectedIndexChanged -= ComboBoxAdvRotationsSelectedIndexChanged;
			textBoxDescription.Validating -= TextBoxDescriptionValidating;
			textBoxDescription.Validated -= TextBoxDescriptionValidated;
			buttonNew.Click -= ButtonNewClick;
			buttonDelete.Click -= ButtonDeleteClick;
			numericUpDownWeek.ValueChanged -= NumericUpDownWeekValueChanged;

			_rotationList = null;
			_restrictionViewList = null;
			_gridColumns = null;
			_gridHelper = null;
		}

		private void UnregisterMessageBrooker()
		{
			if (_messageBroker == null) return;
			_messageBroker.UnregisterEventSubscription(MessageBrokerDayOffMessage);
			_messageBroker.UnregisterEventSubscription(MessageBrokerShiftCategoryMessage);
			_messageBroker = null;
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			UnitOfWork = value;

			// Creates a new repository.
			Repository = new RotationRepository(UnitOfWork);
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
			return UserTexts.Resources.Rotations;
		}

		public void OnShow()
		{
		}

		private void InitMessageBroker()
		{
			_messageBroker = StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging;
			_messageBroker.RegisterEventSubscription(MessageBrokerDayOffMessage, typeof (IDayOffTemplate));
			_messageBroker.RegisterEventSubscription(MessageBrokerShiftCategoryMessage, typeof (IShiftCategory));
		}

		private void MessageBrokerDayOffMessage(object sender, EventMessageArgs e)
		{
			if (InvokeRequired)
			{
				Invoke(new EventHandler<EventMessageArgs>(MessageBrokerDayOffMessage), sender, e);
			}
			else
			{
				HandleDayOffMessage(e.Message);
				if (e.Message.DomainUpdateType != DomainUpdateType.NotApplicable)
				{
					RefreshRange(RangeCategory.DayOffs);
				}
			}
		}

		private void HandleDayOffMessage(IEventMessage message)
		{
			switch (message.DomainUpdateType)
			{
				case DomainUpdateType.Insert:
					AddDayOff(message);
					break;

				case DomainUpdateType.Update:
					UpdateDayOff(message);
					break;

				case DomainUpdateType.Delete:
					FindAndRemoveDayOff(message);
					break;

				case DomainUpdateType.NotApplicable:
					break;
				default:
					break;
			}
		}

		private IDayOffTemplate FindAndRemoveDayOff(IEventMessage message)
		{
			var instance = _dayOffList.FirstOrDefault(d => d.Id == message.DomainObjectId);
			if (instance != null)
			{
				_dayOffList.Remove(instance);
			}
			return instance;
		}

		private void AddDayOff(IEventMessage message)
		{
			var instance = new DayOffTemplateRepository(UnitOfWork)
				.Get(message.DomainObjectId);
			_dayOffList.Add(instance);
		}

		private void UpdateDayOff(IEventMessage message)
		{
			var instance = FindAndRemoveDayOff(message);
			if (UnitOfWork.Contains(instance))
			{
				UnitOfWork.Remove(instance);
			}
			AddDayOff(message);
		}

		private void MessageBrokerShiftCategoryMessage(object sender, EventMessageArgs e)
		{
			if (InvokeRequired)
			{
				Invoke(new EventHandler<EventMessageArgs>(MessageBrokerShiftCategoryMessage), sender, e);
			}
			else
			{
				HandleShiftCategoryMessage(e.Message);
				if (e.Message.DomainUpdateType != DomainUpdateType.NotApplicable)
				{
					RefreshRange(RangeCategory.ShiftCategories);
				}
			}
		}

		private void HandleShiftCategoryMessage(IEventMessage message)
		{
			switch (message.DomainUpdateType)
			{
				case DomainUpdateType.Insert:
					AddShiftCategory(message);
					break;

				case DomainUpdateType.Update:
					UpdateShiftCateogry(message);
					break;

				case DomainUpdateType.Delete:
					FindAndRemoveShiftCategory(message);
					break;

				case DomainUpdateType.NotApplicable:
					break;
				default:
					break;
			}
		}

		private IShiftCategory FindAndRemoveShiftCategory(IEventMessage message)
		{
			var instance = _shiftCategoryList.FirstOrDefault(d => d.Id == message.DomainObjectId);
			if (instance != null)
			{
				_shiftCategoryList.Remove(instance);
			}
			return instance;
		}

		private void AddShiftCategory(IEventMessage message)
		{
			var instance = new ShiftCategoryRepository(UnitOfWork)
				.Get(message.DomainObjectId);
			_shiftCategoryList.Add(instance);
		}

		private void UpdateShiftCateogry(IEventMessage message)
		{
			var instance = FindAndRemoveShiftCategory(message);
			if (UnitOfWork.Contains(instance))
			{
				UnitOfWork.Remove(instance);
			}

			AddShiftCategory(message);
		}

		public RotationPage()
		{
			InitializeComponent();

			// Binds events.
			comboBoxAdvRotations.SelectedIndexChanging += ComboBoxAdvRotationsSelectedIndexChanging;
			comboBoxAdvRotations.SelectedIndexChanged += ComboBoxAdvRotationsSelectedIndexChanged;
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
			string changed = _localizer.UpdatedByText(SelectedRotation, UserTexts.Resources.UpdatedByColon);
			autoLabelInfoAboutChanges.Text = changed;
		}

		private void ChangeToOverMidnight()
		{
			var earlyEndSelected = _gridHelper.HasColumnSelected(_earlyEndTimeColumn);
			var lateEndSelected = _gridHelper.HasColumnSelected(_lateEndTimeColumn);

			if (!earlyEndSelected && !lateEndSelected) return;
			ICollection<RotationRestrictionView> selectedList = _gridHelper.FindSelectedItems();
			foreach (var view in selectedList)
			{
			    if (lateEndSelected)
			    {
			        if (lateEndTimeWillBeValid(view))
			            view.LateEndTime = view.LateEndTime.Value.Add(TimeSpan.FromDays(1));
			    }
			    if (earlyEndSelected)
				{
                    if (earlyStartTimeWillBeValid(view))
                        view.EarlyEndTime = view.EarlyEndTime.Value.Add(TimeSpan.FromDays(1));

				}
			}
			// Refreshes the Grid.
			RefreshRange(RangeCategory.TimeColumns);
		}

	    private static bool lateEndTimeWillBeValid(RotationRestrictionView view)
	    {
	        return view.LateEndTime.HasValue && view.LateEndTime.Value < TimeSpan.FromDays(1);
	    }

	    private static bool earlyStartTimeWillBeValid(RotationRestrictionView view)
	    {
	        return view.EarlyEndTime.HasValue && view.EarlyEndTime.Value<TimeSpan.FromDays(1) && view.EarlyEndTime.Value.Add(TimeSpan.FromDays(1)) <= view.LateEndTime.GetValueOrDefault(TimeSpan.FromDays(2));
	    }

	    private void RefreshRange(RangeCategory category)
		{
			var gridRange = _gridRangeDictionary[category];
			foreach (var range in gridRange)
			{
				gridControlRotation.RefreshRange(range);
			}
		}

		private void SelectRotation()
		{
			if (SelectedRotation == null) return;
			numericUpDownWeek.Value =
				ScheduleRestrictionBaseView.GetWeek(SelectedRotation.RotationDays.Count - ItemDiffernce);
			textBoxDescription.Text = SelectedRotation.Name;
			SaveChanges();
			PrepareGridView();
		}

		private void InitGrid()
		{
			gridControlRotation.Rows.HeaderCount = 0;
			gridControlRotation.Cols.HeaderCount = 0;

			_gridColumns = null;

			gridControlRotation.SaveCellInfo += gridControlRotationSaveCellInfo;
			
			gridControlRotation.PrepareViewStyleInfo += gridControlRotationPrepareViewStyleInfo;
			gridControlRotation.MouseUp += gridControlRotationMouseUp;
			gridControlRotation.KeyUp += gridControlRotationKeyUp;
		}

		void gridControlRotationPrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
		{
			e.Style.BackColor = e.RowIndex == gridControlRotation.CurrentCell.RowIndex ? Color.LightGoldenrodYellow : Color.White;
		}

		void gridControlRotationKeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode.Equals(Keys.Down) || e.KeyCode.Equals(Keys.Up) || e.KeyCode.Equals(Keys.Enter))
				gridControlRotation.Invalidate();
		}

		void gridControlRotationMouseUp(object sender, MouseEventArgs e)
		{
			gridControlRotation.Invalidate();
		}

		void gridControlRotationSaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			gridControlRotation.Invalidate();
		}

		private void CreateColumns()
		{
			_gridColumns = new List<SFGridColumnBase<RotationRestrictionView>>
							{
								new SFGridRowHeaderColumn<RotationRestrictionView>(string.Empty),
								new SFGridDescriptionNameColumn<RotationRestrictionView>("Week", UserTexts.Resources.Week,
																						 50, true),
								new SFGridDescriptionNameColumn<RotationRestrictionView>("Day", UserTexts.Resources.WeekDay,
																						 100,
																						 true)
							};

			_earlyStartTimeColumn = new SFGridTimeOfDayColumn<RotationRestrictionView>("EarlyStartTime",
																						UserTexts.Resources.EarlyStartTime);
			_gridColumns.Add(_earlyStartTimeColumn);
	
			_lateStartTimeColumn = new SFGridTimeOfDayColumn<RotationRestrictionView>("LateStartTime",
																					  UserTexts.Resources.LateStartTime);
			_gridColumns.Add(_lateStartTimeColumn);

			_earlyEndTimeColumn = new SFGridTimeOfDayColumn<RotationRestrictionView>("EarlyEndTime",
																					 UserTexts.Resources.EarlyEndTime);
			_gridColumns.Add(_earlyEndTimeColumn);
			_lateEndTimeColumn = new SFGridTimeOfDayColumn<RotationRestrictionView>("LateEndTime", UserTexts.Resources.LateEndTime);
			_gridColumns.Add(_lateEndTimeColumn);

			_gridColumns.Add(new SFGridHourMinutesOrEmptyColumn<RotationRestrictionView>("MinimumWorkTime",
																						 UserTexts.Resources.MinWorkTime));
			_gridColumns.Add(new SFGridHourMinutesOrEmptyColumn<RotationRestrictionView>("MaximumWorkTime",
																						 UserTexts.Resources.MaxWorkTime));
			_shiftCategoriesColumn = new SFGridDropDownColumn<RotationRestrictionView, IShiftCategory>("ShiftCategory",
																										UserTexts.Resources.ShiftCategoryHeader,
																										_shiftCategoryList,
																										"Description",
																										typeof (
																											ShiftCategory
																											));
			_shiftCategoriesColumn.QueryComboItems += ShiftCategoriesColumnQueryComboItems;
			_gridColumns.Add(_shiftCategoriesColumn);
			_dayOffsColumn = new SFGridDropDownColumn<RotationRestrictionView, IDayOffTemplate>("DayOffTemplate",
																								UserTexts.Resources.DayOff,
																								_dayOffList,
																								"Description", 
																								typeof (DayOffTemplate));
			_dayOffsColumn.QueryComboItems += DayOffsColumnQueryComboItems;
			_gridColumns.Add(_dayOffsColumn);
		}

		private void DayOffsColumnQueryComboItems(object sender, GridQueryCellInfoEventArgs e)
		{
			e.Style.DataSource = _dayOffList;
		}

		private void ShiftCategoriesColumnQueryComboItems(object sender, GridQueryCellInfoEventArgs e)
		{
			e.Style.DataSource = _shiftCategoryList;
		}

		private void CreateGridRanges()
		{
			ICollection<GridRangeInfo> timeColumnsRange = new List<GridRangeInfo>();
			int colIndex = _gridColumns.IndexOf(_earlyStartTimeColumn);
			timeColumnsRange.Add(GridRangeInfo.Col(colIndex));
			colIndex = _gridColumns.IndexOf(_lateStartTimeColumn);
			timeColumnsRange.Add(GridRangeInfo.Col(colIndex));
			colIndex = _gridColumns.IndexOf(_earlyEndTimeColumn);
			timeColumnsRange.Add(GridRangeInfo.Col(colIndex));
			colIndex = _gridColumns.IndexOf(_lateEndTimeColumn);
			timeColumnsRange.Add(GridRangeInfo.Col(colIndex));

			_gridRangeDictionary.Add(RangeCategory.TimeColumns, timeColumnsRange);

			ICollection<GridRangeInfo> shiftCategoryColumnRange = new List<GridRangeInfo> {GridRangeInfo.Table()};

			_gridRangeDictionary.Add(RangeCategory.ShiftCategories, shiftCategoryColumnRange);

			ICollection<GridRangeInfo> dayOffColumnRange = new List<GridRangeInfo> {GridRangeInfo.Table()};

			_gridRangeDictionary.Add(RangeCategory.DayOffs, dayOffColumnRange);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void PrepareGridView()
		{
			_restrictionViewList.Clear();

			_restrictionViewList.AddRange(ScheduleRestrictionBaseView.Parse(SelectedRotation));

			if (_gridHelper!=null)
			{
				_gridHelper.Dispose();
			}
			_gridHelper = new SFGridColumnGridHelper<RotationRestrictionView>(
				gridControlRotation,
				new ReadOnlyCollection<SFGridColumnBase<RotationRestrictionView>>(_gridColumns),
				_restrictionViewList
				) {AllowExtendedCopyPaste = true};

			_gridHelper.PasteFromClipboardFinished += GridHelperPasteFromClipboardFinished;
		}

		private void ChangeRotationDays()
		{
			var weekCount = ScheduleRestrictionBaseView.GetWeek(SelectedRotation.DaysCount - ItemDiffernce);
			var value = numericUpDownWeek.Value;

			SaveChanges();

			if (value > weekCount)
			{
				// Add days to collection.
				var daysToAdd = ((int) value - weekCount)*(int) ScheduleRestrictionBaseView.DaysPerWeek;
				SelectedRotation.AddDays(daysToAdd);

				PrepareGridView();
			}
			else if (value < weekCount)
			{
				// Remove days from collection.
				var daysToRemove = (weekCount - (int) value)*(int) ScheduleRestrictionBaseView.DaysPerWeek;
				SelectedRotation.RemoveDays(daysToRemove);

				PrepareGridView();
			}
		}

		private void AddNewRotation()
		{
			SaveChanges();
			_rotationList.Add(CreateRotation());
			LoadRotations();
			comboBoxAdvRotations.SelectedIndex = LastItemIndex;
		}

		private void DeleteRotation()
		{
			if (SelectedRotation == null) return;
			if (ShowMyErrorMessage(string.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.AreYouSureYouWantToDeleteRotation,
							  SelectedRotation.Name), UserTexts.Resources.Message) != DialogResult.Yes) 
				return;

			Repository.Remove(SelectedRotation);
			// Removes from list.
			_rotationList.Remove(SelectedRotation);

			LoadRotations();
		}

		private bool ValidateRotationDescription()
		{
			var failed = string.IsNullOrEmpty(textBoxDescription.Text);
			if (failed)
			{
				textBoxDescription.SelectedText = SelectedRotation.Name;
			}

			return !failed;
		}

		private void ChangeRotationDescription()
		{
			SelectedRotation.Name = textBoxDescription.Text;
			SaveChanges();
			LoadRotations();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		public static DialogResult ShowMyErrorMessage(string message, string caption)
		{
			return ViewBase.ShowYesNoMessage(message, caption);
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

			gridControlRotation.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControlRotation.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
		}

		private void LoadShiftCategories()
		{
			if (!_shiftCategoryList.IsEmpty()) return;
			// Loads default category.
			_shiftCategoryList.Add(RotationRestrictionView.DefaultShiftCategory);

			// Loads all categories.
			var repository = new ShiftCategoryRepository(UnitOfWork);

			IList<IShiftCategory> list = repository.LoadAll();
			IEnumerable<IShiftCategory> sortedList = (from s in list
													  orderby s.Description.ShortName
													  select s).ToList();

			_shiftCategoryList.AddRange(sortedList);
		}

		private void LoadDayOffs()
		{
			if (!_dayOffList.IsEmpty()) return;
			// Loads default category.
			_dayOffList.Add(RotationRestrictionView.DefaultDayOff);

			// Loads all categories.
			var repository = new DayOffTemplateRepository(UnitOfWork);

			IList<IDayOffTemplate> list = repository.LoadAll();
			IEnumerable<IDayOffTemplate> sortedList = (from d in list
														orderby d.Description.ShortName
														select d).ToList();

			_dayOffList.AddRange(sortedList);
		}

		private void LoadRotations()
		{
			if (Disposing) return;
			if (_rotationList == null)
			{
				// Loads all rotations.
				IList<IRotation> unSortedList = Repository.LoadAllRotationsWithHierarchyData();
				//.OrderBy(a => a.Description.Name)
				// Sort my list on description.
				IEnumerable<IRotation> sortedList = (from s in unSortedList
													 orderby s.Name ascending
													 select s).ToList();

				_rotationList = new List<IRotation>(sortedList);
			}
			if (_rotationList.IsEmpty())
			{
				_rotationList.Add(CreateRotation());
			}

			// Removes binding from comboBoxAdvRotations.
			int selected = comboBoxAdvRotations.SelectedIndex;
			var selectedRotation = comboBoxAdvRotations.SelectedItem as IRotation;
			if (!IsWithinRange(selected)) selected = FirstItemIndex;

			// Rebinds list to comboBoxAdvRotations.
			comboBoxAdvRotations.DataSource = null;
			comboBoxAdvRotations.DisplayMember = "Name";
			comboBoxAdvRotations.DataSource = _rotationList;
			if (selectedRotation != null)
				if (((IDeleteTag)selectedRotation).IsDeleted != true)
					comboBoxAdvRotations.SelectedItem = selectedRotation;
				else
					comboBoxAdvRotations.SelectedIndex = selected;
			else
				comboBoxAdvRotations.SelectedIndex = selected;
		}

		private bool IsWithinRange(int index)
		{
			return index > InvalidItemIndex && index < _rotationList.Count && comboBoxAdvRotations.DataSource != null;
		}

		private IRotation CreateRotation()
		{
			// Formats the name.
			var description = PageHelper.CreateNewName(_rotationList, "Name", UserTexts.Resources.NewRotation);

			var newRotation = new Rotation(description.Name, (int) ScheduleRestrictionBaseView.DaysPerWeek);

			Repository.Add(newRotation);

			return newRotation;
		}

		private void TextBoxDescriptionValidating(object sender, CancelEventArgs e)
		{
			if (SelectedRotation != null)
			{
				e.Cancel = !ValidateRotationDescription();
			}
		}

		private void TextBoxDescriptionValidated(object sender, EventArgs e)
		{
			if (SelectedRotation != null)
			{
				ChangeRotationDescription();
			}
		}

		private void ComboBoxAdvRotationsSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !IsWithinRange(e.NewIndex);
		}

		private void ComboBoxAdvRotationsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedRotation == null) return;
			Cursor.Current = Cursors.WaitCursor;
			SelectRotation();
			ChangedInfo();
			Cursor.Current = Cursors.Default;
		}

		private void GridHelperPasteFromClipboardFinished(object sender, EventArgs e)
		{
			var columnSelected =
				_gridHelper.HasColumnSelected(_lateStartTimeColumn)
				|| _gridHelper.HasColumnSelected(_earlyEndTimeColumn)
				|| _gridHelper.HasColumnSelected(_lateStartTimeColumn);

			if (columnSelected)
			{
				RefreshRange(RangeCategory.TimeColumns);
			}
		}

		private void NumericUpDownWeekValueChanged(object sender, EventArgs e)
		{
			if (SelectedRotation == null) return;
			Cursor.Current = Cursors.WaitCursor;
			ChangeRotationDays();
			Cursor.Current = Cursors.Default;
		}

		private void ButtonNewClick(object sender, EventArgs e)
		{
			if (SelectedRotation == null) return;
			Cursor.Current = Cursors.WaitCursor;
			AddNewRotation();
			Cursor.Current = Cursors.Default;
		}

		private void ButtonDeleteClick(object sender, EventArgs e)
		{
			if (SelectedRotation == null) return;
			Cursor.Current = Cursors.WaitCursor;
			DeleteRotation();
			Cursor.Current = Cursors.Default;
		}

		private void ButtonAdvOvernightClick(object sender, EventArgs e)
		{
			if (SelectedRotation == null) return;
			Cursor.Current = Cursors.WaitCursor;
			ChangeToOverMidnight();
			Cursor.Current = Cursors.Default;
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			LoadControl();

			var selectedRotation = entity.SelectedEntityObject as IRotation;

			if (selectedRotation == null || !_rotationList.Contains(selectedRotation)) return;
			var index = _rotationList.IndexOf(selectedRotation);

			comboBoxAdvRotations.SelectedIndex = index;
		}

		public ViewType ViewType
		{
			get { return ViewType.Rotation; }
		}
	}
}