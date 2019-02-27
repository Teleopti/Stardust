using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
    public partial class ContractScheduleControl : BaseUserControl, ISettingPage
    {
        private bool _isDirty = true;
        private IUnitOfWork _unitOfWork;
        private ContractScheduleRepository _contractSchRep;
        private List<IContractSchedule> _contractScheduleList;
        private readonly List<ContractScheduleWeekAdapter> _contractScheduleWeekList = new List<ContractScheduleWeekAdapter>();
        private ReadOnlyCollection<SFGridColumnBase<ContractScheduleWeekAdapter>> _contractScheduleWeekCols;
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
        private const short invalidItemIndex = -1;
        private const short firstItemIndex = 0;
        private const int weekColumnWidth = 55;
        private const int checkBoxColumnWidth = 60;

        private ContractScheduleRepository ContractSchRepository
        {
            get { return _contractSchRep ?? (_contractSchRep = ContractScheduleRepository.DONT_USE_CTOR(_unitOfWork)); }
        }
        private void changedInfo()
        {
            autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
            autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
            string changed = _localizer.UpdatedByText(SelectedContractSchedule, Resources.UpdatedByColon);
            autoLabelInfoAboutChanges.Text = changed;
        }

        private ContractSchedule SelectedContractSchedule
        {
            get { return comboBoxAdvScheduleCollection.SelectedItem as ContractSchedule; }
        }

        private static CultureInfo CurrentUiCulture
        {
            get
            {
                return
                    TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture;
            }
        }

        public ContractScheduleControl()
        {
            InitializeComponent();
        }

        private void comboBoxAdvScheduleCollectionSelectedIndexChanged(object sender, EventArgs e)
        {
            handleDataSourceToControlTransfer();

            changedInfo();
            prepareGrid();
        }

        private void buttonAdvAddNewContractScheduleWeekClick(object sender, EventArgs e)
        {
            if (SelectedContractSchedule != null)
            {
                addContractScheduleWeek();
            }
        }

        private void buttonNewContractScheduleClick(object sender, EventArgs e)
        {
            _isDirty = true;
            addNewContractSchedule();
        }

        private void buttonAdvDeleteContractScheduleClick(object sender, EventArgs e)
        {
            if (SelectedContractSchedule == null)
            {
                ClearControls();
                return;
            }

            string text = string.Format(
                CurrentCulture,
                Resources.AreYouSureYouWantToDeleteContractSchedule,
                SelectedContractSchedule.Description
                );

            string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

            DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
            if (response != DialogResult.Yes) return;

            ContractSchRepository.Remove(SelectedContractSchedule);
            _contractScheduleList.Remove(SelectedContractSchedule);

            loadContractSchedules();

            if (_contractScheduleList.Count > 0)
            {
                textBoxExtDescription.Text = SelectedContractSchedule.Description.ToString();
                changedInfo();
                prepareGrid();
            }
            else
            {
                ClearControls();
            }

        }

        private void textBoxExtDescriptionTextChanged(object sender, EventArgs e)
        {
            _isDirty = true;
        }

        private void textBoxExtDescriptionValidating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ValidateData()) return;
            textBoxExtDescription.Text = SelectedContractSchedule.Description.Name;
            e.Cancel = false;
        }

        private void textBoxExtDescriptionValidated(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxExtDescription.Text))
            {
                textBoxExtDescription.Text = SelectedContractSchedule.Description.Name;
            }
        }

        private void textBoxExtDescriptionLeave(object sender, EventArgs e)
        {
            if (SelectedContractSchedule == null)
            {
                //ShowMyErrorMessage(UserTexts.Resources.YouHaveToHaveAtLeastOneScoreCard, UserTexts.Resources.SaveError);
                return;
            }
            if (string.IsNullOrWhiteSpace(textBoxExtDescription.Text))
            {
                textBoxExtDescription.Focus();
                return;
            }
            SelectedContractSchedule.Description = new Description(textBoxExtDescription.Text);
            ContractSchedule itemToBeSelected = SelectedContractSchedule;
            loadContractSchedules();
            comboBoxAdvScheduleCollection.SelectedIndex = _contractScheduleList.IndexOf(itemToBeSelected);
        }

        private void contractScheduleGridSelectionChanged(object sender, EventArgs e)
        {
            _isDirty = true;
        }

        private void buttonAdvDeleteWeekClick(object sender, EventArgs e)
        {
            IList<int> selectedList = getSelectedRowsToBeDeleted();

            if (selectedList == null || selectedList.Count == 0)
                return;

            string text = string.Format(
                CurrentCulture,
                Resources.AreYouSureYouWantToDelete);

            string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);
            DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
            if (response != DialogResult.Yes) return;
            Cursor.Current = Cursors.WaitCursor;

            deleteWeek(selectedList);

            Cursor.Current = Cursors.Default;


        }

        private void deleteWeek(IList<int> selectedList)
        {
            IList<ContractScheduleWeekAdapter> source = _contractScheduleWeekList;
            IList<ContractScheduleWeekAdapter> toBeDeleted = new List<ContractScheduleWeekAdapter>();
            for (int i = 0; i <= (selectedList.Count - 1); i++)
            {
                toBeDeleted.Add(source[selectedList[i]]);
            }
            foreach (ContractScheduleWeekAdapter weekAdapter in toBeDeleted)
            {
                SelectedContractSchedule.RemoveContractScheduleWeek(weekAdapter.ContainedEntity);
                _contractScheduleWeekList.Remove(weekAdapter);
            }
            gridControlContractSchedule.RowCount = source.Count;

            gridControlContractSchedule.Invalidate();
        }

        private void contractScheduleGridKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                pasteWhenRangeSelected();
            }
        }

        public void Unload()
        {
        }

        public TreeFamily TreeFamily()
        {
            return new TreeFamily(Resources.Contract);
        }

        public string TreeNode()
        {
            return Resources.Contractschedules;
        }

        public void OnShow()
        {
        }

        public void InitializeDialogControl()
        {
            setColors();
            SetTexts();

            gridControlContractSchedule.BeginUpdate();
            //CreateScheduleHeaders();
            gridControlContractSchedule.RowCount = 0;
            gridControlContractSchedule.EndUpdate();

            _contractScheduleWeekCols = configureGrid();
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

            gridControlContractSchedule.BackColor = ColorHelper.GridControlGridInteriorColor();
            gridControlContractSchedule.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();

        }

        public void LoadControl()
        {
            loadContractSchedules();

            comboBoxAdvScheduleCollectionSelectedIndexChanged(this, new EventArgs());
            prepareGrid();
        }

        public void SaveChanges()
        {
            if (_isDirty && ValidateData())
            {
            }
            _isDirty = false;
        }

        public void SetUnitOfWork(IUnitOfWork value)
        {
            _unitOfWork = value;
            _contractSchRep = ContractScheduleRepository.DONT_USE_CTOR(value);
        }

        public void Persist()
        { }

        public bool ValidateData()
        {
            return !string.IsNullOrWhiteSpace(textBoxExtDescription.Text);
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            toolTip1.SetToolTip(buttonAdvDeleteContractSchedule, Resources.Delete);
            toolTip1.SetToolTip(buttonAddNewContractSchedule, Resources.AddNewContractSchedule);
            toolTip1.SetToolTip(buttonAdvAddWeek, Resources.AddNewWeekToSchedule);
            toolTip1.SetToolTip(buttonAdvDeleteWeek, Resources.Delete);
        }

        private void addNewContractSchedule()
        {
            var newContractSchedule = createContractSchedule();

            //
            _contractScheduleList.Add(newContractSchedule);
            ContractSchRepository.Add(newContractSchedule);

            loadContractSchedules();

            //Selected the added Scorecard
            comboBoxAdvScheduleCollection.SelectedIndex = _contractScheduleList.IndexOf(newContractSchedule);
            comboBoxAdvScheduleCollectionSelectedIndexChanged(this, new EventArgs());

            buttonAdvAddWeek.PerformClick();
        }

        private void pasteWhenRangeSelected()
        {
            ClipHandler clipHandler = GridHelper.ConvertClipboardToClipHandler();

            if (clipHandler.ClipList.Count <= 0) return;
            var rangelist = GridHelper.GetGridSelectedRanges(gridControlContractSchedule, true);

            foreach (GridRangeInfo range in rangelist)
            {

                if (range.IsTable)
                {
                    for (int row = 1; row <= gridControlContractSchedule.RowCount; row++)
                    {
                        for (int col = 1; col <= gridControlContractSchedule.ColCount; col++)
                        {
                            Paste(clipHandler.ClipList[0], row, col);
                        }
                    }
                }
                else
                {
                    if ((gridControlContractSchedule.RowCount - range.Top) < clipHandler.RowSpan())
                    {
                        int dif = clipHandler.RowSpan() - (gridControlContractSchedule.RowCount - range.Top + 1);
                        for (int i = 0; i < dif; i++)
                        {
                            buttonAdvAddWeek.PerformClick();
                        }
                    }

                    //loop all rows in selection, step with height in clip
                    for (int i = range.Top; i <= range.Bottom; i = i + clipHandler.RowSpan())
                    {
                        int row = i;

                        handlePaste(clipHandler, range, i, row);
                    }
                }

                gridControlContractSchedule.InvalidateRange(range);
            }
        }

        private void handlePaste(ClipHandler clipHandler, GridRangeInfo range, int i, int row)
        {
            for (int j = 1; j <= gridControlContractSchedule.ColCount; j = j + clipHandler.ColSpan())
            {
                int col = j;

                if (row <= gridControlContractSchedule.Rows.HeaderCount ||
                    col <= gridControlContractSchedule.Cols.HeaderCount) continue;
                foreach (Clip clip in clipHandler.ClipList)
                {
                    //check clip fits inside selected range, rows
                    if (GridHelper.IsPasteRangeOk(range, gridControlContractSchedule, clip, i, j))
                    {
                        Paste(clip, row + clip.RowOffset, col + clip.ColOffset);
                    }
                }
            }
        }

        public virtual void Paste(Clip clip, int rowIndex, int columnIndex)
        {
            if (columnIndex == int.MinValue)
            {
                throw new ArgumentOutOfRangeException("columnIndex", "columnIndex must be larger than Int32.MinValue");
            }

            GridStyleInfo gsi = gridControlContractSchedule[rowIndex, columnIndex];
            var clipValue = (string)clip.ClipObject;
            if (clipValue.Length <= gsi.MaxLength || gsi.MaxLength == 0)
                gsi.ApplyFormattedText(clipValue);
        }

        private ContractSchedule createContractSchedule()
        {
            // Formats the name.
            var newItemName = PageHelper.CreateNewName(_contractScheduleList, "Description.Name", Resources.NewContractSchedule);

            var newContractSchedule = new ContractSchedule(newItemName.Name);

            ContractSchRepository.Add(newContractSchedule);

            return newContractSchedule;
        }

        private void loadContractSchedules()
        {
            if (_contractScheduleList == null)
            {
                _contractScheduleList = new List<IContractSchedule>();

                foreach (IContractSchedule schedule in ContractSchRepository.FindAllContractScheduleByDescription())
                {
                    _contractScheduleList.Add(schedule);
                }
            }

            if (_contractScheduleList.IsEmpty())
            {
                _contractScheduleList.Add(createContractSchedule());
            }
            _contractScheduleList = _contractScheduleList.OrderBy(p => p.Description.Name).ToList();

            // Stores already selected index. 1st item, if none selected.
            int selected = comboBoxAdvScheduleCollection.SelectedIndex;
            if (selected == invalidItemIndex) selected = firstItemIndex;

            comboBoxAdvScheduleCollection.SelectedIndexChanged -= comboBoxAdvScheduleCollectionSelectedIndexChanged;

            // Binds list to comboBoxAdvScenarioCollection.
            comboBoxAdvScheduleCollection.DataSource = null;
            comboBoxAdvScheduleCollection.DataSource = _contractScheduleList;
            comboBoxAdvScheduleCollection.DisplayMember = "Description";

            if (selected >= comboBoxAdvScheduleCollection.Items.Count)
                selected = comboBoxAdvScheduleCollection.Items.Count - 1;

            comboBoxAdvScheduleCollection.SelectedIndexChanged += comboBoxAdvScheduleCollectionSelectedIndexChanged;

            comboBoxAdvScheduleCollection.SelectedIndex = selected;

            handleDataSourceToControlTransfer();
        }

        private static string getDayNameShort(DayOfWeek day)
        {
            return CurrentUiCulture.DateTimeFormat.GetShortestDayName(day);
        }

        private void handleDataSourceToControlTransfer()
        {
            if (SelectedContractSchedule != null)
            {
                textBoxExtDescription.Text = SelectedContractSchedule.Description.ToString();

            }
            else
            {
                ClearControls();
            }
        }

        public void ClearControls()
        {
            textBoxExtDescription.Text = string.Empty;

            comboBoxAdvScheduleCollection.SelectedIndexChanged -= comboBoxAdvScheduleCollectionSelectedIndexChanged;
            comboBoxAdvScheduleCollection.DataSource = null;
            comboBoxAdvScheduleCollection.SelectedIndexChanged += comboBoxAdvScheduleCollectionSelectedIndexChanged;

            gridControlContractSchedule.RowCount = 0;
            gridControlContractSchedule.Invalidate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.Win.Common.Configuration.Columns.SFGridColumnGridHelper`1<Teleopti.Ccc.WinCode.Settings.ContractScheduleWeekAdapter>")]
        private void prepareGrid()
        {

            if (SelectedContractSchedule != null)
            {
                parseWeekListToWeekAdapterList();
                gridControlContractSchedule.RowCount = 0;

                new SFGridColumnGridHelper<ContractScheduleWeekAdapter>(gridControlContractSchedule,
                                            _contractScheduleWeekCols,
                                            _contractScheduleWeekList,false);
            }

            gridControlContractSchedule.ColWidths[0] = weekColumnWidth;
            gridControlContractSchedule.Invalidate();
        }

        private static ReadOnlyCollection<SFGridColumnBase<ContractScheduleWeekAdapter>> configureGrid()
        {
            IList<SFGridColumnBase<ContractScheduleWeekAdapter>> gridColumns =
               new List<SFGridColumnBase<ContractScheduleWeekAdapter>> { new SFGridRowHeaderColumn<ContractScheduleWeekAdapter>(Resources.Week, weekColumnWidth) };

            IList<DayOfWeek> days =
                DateHelper.GetDaysOfWeek(
                    TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);

            foreach (var dayOfWeek in days)
            {
                gridColumns.Add(new SFGridCheckBoxColumn<ContractScheduleWeekAdapter>(dayOfWeek.ToString(), getDayNameShort(dayOfWeek), checkBoxColumnWidth));
            }

            return new ReadOnlyCollection<SFGridColumnBase<ContractScheduleWeekAdapter>>(gridColumns);
        }

        private void parseWeekListToWeekAdapterList()
        {
            _contractScheduleWeekList.Clear();
            IEnumerable<IContractScheduleWeek> weeks = SelectedContractSchedule.ContractScheduleWeeks.OrderBy(w => w.WeekOrder).ToList();
            foreach (ContractScheduleWeek week in weeks)
            {
                _contractScheduleWeekList.Add(new ContractScheduleWeekAdapter(week));
            }
        }

        private void addContractScheduleWeek()
        {
            //gridCoulmnHelperBase.GetSFGridColumnGridHelper<IShif>(gridShiftCategory).Add(ShiftCatReposiroty);
            var week = new ContractScheduleWeek();

            SelectedContractSchedule.AddContractScheduleWeek(week);
            parseWeekListToWeekAdapterList();
            gridControlContractSchedule.RowCount++;

            //Get the current cell and move down on the same col
            int colIndex = (gridControlContractSchedule.Model.CurrentCellInfo == null)
                               ? 1
                               : gridControlContractSchedule.Model.CurrentCellInfo.ColIndex;
            gridControlContractSchedule.CurrentCell.MoveTo(gridControlContractSchedule.RowCount, colIndex,
                                                 GridSetCurrentCellOptions.ScrollInView);

            gridControlContractSchedule.Invalidate();
        }

        private IList<int> getSelectedRowsToBeDeleted()
        {
            IList<int> selectedList = new List<int>();
            GridRangeInfoList selectedRangeInfoList = gridControlContractSchedule.Model.Selections.GetSelectedRows(false, false);
            foreach (GridRangeInfo rangeInfo in selectedRangeInfoList)
            {
                string[] rowSplit = rangeInfo.Info.Split(":".ToCharArray());
                if (!rangeInfo.IsTable)
                {
                    if (rangeInfo.Height > 1)
                    {
                        if (rowSplit.Length == 2)
                        {
                            int startIndex = getNumber(rowSplit[0]);
                            int endIndex = getNumber(rowSplit[1]);
                            for (int i = startIndex; i <= endIndex; i++)
                                selectedList.Add((i - 1));
                        }
                    }
                    else
                    {
                        foreach (string row in rowSplit)
                        {
                            int rowIndex = getNumber(row);
                            selectedList.Add(rowIndex - 1);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < gridControlContractSchedule.RowCount; i++)
                    {
                        selectedList.Add(i);
                    }
                }
            }
            return selectedList;
        }

        private static int getNumber(string name)
        {
            return Int32.Parse(name.Replace("R", ""), CultureInfo.CurrentCulture.NumberFormat);
        }

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
            throw new NotImplementedException();
        }

        public ViewType ViewType
        {
            get { return ViewType.ContractSchedule; }
        }
    }
}
