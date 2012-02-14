﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Payroll.Overtime
{
    public partial class MultiplicatorControlView : BaseUserControl, ISettingPage
    {

        private SFGridColumnGridHelper<IMultiplicator> _gridColumnHelper;
        private IList<IMultiplicator> _sourceList;
        private IMultiplicatorRepository _multiplicatorRep;
        private IList<MultiplicatorTypeView> _multiplicatorTypeViewCollection;
        private const string DoubleSpace = "  ";
        private const int EmptySourceCount = 0;
        private const int ColumnListCountMappingValue = 1;
        private const int EmptyHeaderCount = 0;
        private const int DefaultCellColumnIndex = 0;
        private const int DefaultCellRowIndex = 0;
        public IUnitOfWork UnitOfWork { get; private set; }

        public ViewType ViewType
        {
            get { return ViewType.Multiplicator; }
        }

        public MultiplicatorControlView()
        {
            InitializeComponent();
        }

        private void buttonAddAdvMultipplicator_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var addNewForm = new ManageMultiplicatorForm(GetNextEntityName());
            addNewForm.MultiplicatorAdded += AddMultiplicatorForm_AfterDefinitionSetAdded;

            SetSelectedCellWhenNoSourceAvailable();
            addNewForm.Show(this);
            
            Cursor.Current = Cursors.Default;
        }

        private void buttonAdvDeleteMultipplicator_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            // Deletes the selected Multiplicator
            DeleteSelectedMultiplicator();

            Cursor.Current = Cursors.Default;
        }

        private string GetNextEntityName()
        {
            Description description = PageHelper.CreateNewName(_gridColumnHelper.SourceList, "Description.Name", Resources.NewMultiplicator);
            return description.Name;
        }

        private void DeleteSelectedMultiplicator()
        {
            if (IsReadyToDelete())
            {
                IList<IMultiplicator> itemToDelete =
                    _gridColumnHelper.FindSelectedItems();

                foreach (IMultiplicator multiplicator in itemToDelete)
                {
                    _gridColumnHelper.SourceList.Remove(multiplicator);
                    _multiplicatorRep.Remove(multiplicator);
                }

                gridMultiplicator.RowCount = _gridColumnHelper.SourceList.Count;
                gridMultiplicator.Invalidate();
            }
        }

        private void CreateGrid()
        {
            GridHelper.GridStyle(gridMultiplicator);

            ReadOnlyCollection<SFGridColumnBase<IMultiplicator>> multiplicatorColumns = configureGrid<IMultiplicator>();

            _gridColumnHelper = new SFGridColumnGridHelper<IMultiplicator>(gridMultiplicator,
                                                                           multiplicatorColumns,
                                                                           new List<IMultiplicator>(_sourceList));
                                    _gridColumnHelper.AllowExtendedCopyPaste = true;

            gridMultiplicator.Model.ColWidths.ResizeToFit(GridRangeInfo.Cols(0, gridMultiplicator.ColCount));
        }

        private bool IsReadyToDelete()
        {
            bool isReady = false;

            if (IsDataAvailable())
            {
                if (ShowMyErrorMessage(Resources.MultiplicatorDeleteConfirmation,
                    Resources.Message) == DialogResult.Yes)
                {
                    isReady = true;
                }
            }

            return isReady;
        }

        private bool IsDataAvailable()
        {
            return (_gridColumnHelper.SourceList != null) && (_gridColumnHelper.SourceList.Count > EmptySourceCount);
        }

        private ReadOnlyCollection<SFGridColumnBase<T>> configureGrid<T>() where T : IEntity
        {
            // Holds he column list for the grid control
            IList<SFGridColumnBase<T>> gridColumns = new List<SFGridColumnBase<T>>();

            // Adds the cell models to the grid control
            AddCellmodels();
            // Set the header count for the grid control
            gridMultiplicator.Rows.HeaderCount = EmptyHeaderCount;
            // Adds the header column for the grid control
            gridColumns.Add(new SFGridRowHeaderColumn<T>(string.Empty));

            CreateColumnsForMultipplicatorGrid(gridColumns);

            SFGridColumnBase<T>.AppendAuditColumns(gridColumns);

            gridMultiplicator.RowCount = GridRowCount();
            gridMultiplicator.ColCount = (gridColumns.Count - ColumnListCountMappingValue);

            return new ReadOnlyCollection<SFGridColumnBase<T>>(gridColumns);
        }

        private void AddCellmodels()
        {
            // Adds the cell models to the grid control
            gridMultiplicator.CellModels.Add("DoubleCell", new DoubleCellModel(gridMultiplicator.Model, 2));
            gridMultiplicator.CellModels.Add("DescriptionNameCell", new DescriptionNameCellModel(gridMultiplicator.Model));
            gridMultiplicator.CellModels.Add("DescriptionShortNameCellModel",
                                       new DescriptionShortNameCellModel(gridMultiplicator.Model));

            gridMultiplicator.CellModels.Add("ColorPickerCell", new ColorPickerCellModel(gridMultiplicator.Model));
            gridMultiplicator.CellModels.Add("StaticDropDownCell", new DropDownCellStaticModel(gridMultiplicator.Model));
            gridMultiplicator.CellModels.Add("IgnoreCell", new IgnoreCellModel(gridMultiplicator.Model));
        }

        private static void CreateColumnsForMultipplicatorGrid<T>(ICollection<SFGridColumnBase<T>> gridColumns) where T : IEntity
        {
            gridColumns.Add(new SFGridDescriptionNameColumn<T>("Description", Resources.Name));
            gridColumns.Add(new SFGridDescriptionShortNameColumn<T>("Description", Resources.ShortName, 100, false, 2));
            gridColumns.Add(new SFGridColorPickerColumn<T>("DisplayColor", Resources.Color, null));
            gridColumns.Add(new SFGridReadOnlyEnumColumn<T>("MultiplicatorType", Resources.MultiplicatorType));
            //gridColumns.Add(new SFGridDropDownColumn<T, MultiplicatorTypeView>("MultiplicatorType", "xxMultiplicatorType", _multiplicatorTypeViewCollection, "DisplayName", null));
            gridColumns.Add(new SFGridNumericCellColumn<T>("MultiplicatorValue", Resources.MultiplicatorValue, null, "DoubleCell",50));
            gridColumns.Add(new SFGridEditableTextColumn<T>("ExportCode", 20, Resources.ExportCode){AllowEmptyValue = true});

        }

        private int GridRowCount()
        {
            int _sourceListCount = _sourceList.Count;
            int gridHeaderCount = gridMultiplicator.Rows.HeaderCount;

            return (_sourceListCount + gridHeaderCount);
        }

        private DialogResult ShowMyErrorMessage(string message, string caption)
        {
            return MessageBox.Show(
                   string.Concat(message, DoubleSpace),
                   caption,
                   MessageBoxButtons.YesNo,
                   MessageBoxIcon.Error,
                   MessageBoxDefaultButton.Button1,
                   (RightToLeft == RightToLeft.Yes)
                       ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                       : 0);
        }

        private void MultiplicatorTypeCollection()
        {
            _multiplicatorTypeViewCollection = new List<MultiplicatorTypeView>();
            IList<KeyValuePair<MultiplicatorType, string>> multiplicatorTypeCollection = LanguageResourceHelper.TranslateEnumToList<MultiplicatorType>();
            foreach (KeyValuePair<MultiplicatorType, string> keyValuePair in multiplicatorTypeCollection)
            {
                var multiplicatorTypeView = new MultiplicatorTypeView(keyValuePair.Value, keyValuePair.Key);
                _multiplicatorTypeViewCollection.Add(multiplicatorTypeView);
            }
        }

        private void LoadMultiplicators()
        {
            _sourceList = _multiplicatorRep.LoadAllSortByName();
        }

        private void SetSelectedCellWhenNoSourceAvailable()
        {
            if (!IsDataAvailable())
            {
                gridMultiplicator.CurrentCell.MoveTo(DefaultCellRowIndex, DefaultCellColumnIndex);
            }
        }

        public virtual void Paste(Clip clip, int rowIndex, int columnIndex)
        {
            if (columnIndex == int.MinValue)
            {
                throw new ArgumentOutOfRangeException("columnIndex", "columnIndex must be larger than Int32.MinValue");
            }

            GridStyleInfo gridStyleInfo = gridMultiplicator[rowIndex, columnIndex];
            var clipValue = (string)clip.ClipObject;

            if (clipValue.Length <= gridStyleInfo.MaxLength || gridStyleInfo.MaxLength == 0)
            {
                gridStyleInfo.ApplyFormattedText(clipValue);
            }
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            toolTip1.SetToolTip(buttonAdvDeleteMultiplicator, Resources.DeleteMultiplicator);
            toolTip1.SetToolTip(buttonAdvAddMultiplicator, Resources.AddMultiplicator);
        }

        public void InitializeDialogControl()
        {
            SetColors();
            SetTexts();
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

            gradientPanelHeader.BackgroundColor = ColorHelper.OptionsDialogHeaderGradientBrush();
            labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

            tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            gridMultiplicator.BackColor = ColorHelper.GridControlGridInteriorColor();
            gridMultiplicator.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
        }

        public void LoadControl()
        {
            MultiplicatorTypeCollection();
            LoadMultiplicators();
            CreateGrid();
        }

        public void SaveChanges()
        {}

        public void Unload()
        {
        }

        /// <summary>
        /// The name of the Parent if represented in a TreeView.
        /// </summary>
        public TreeFamily TreeFamily()
        {
            return new TreeFamily(Resources.PayrollSettings);
        }

        /// <summary>
        /// The name of the Node if represented in a TreeView.
        /// </summary>
        public string TreeNode()
        {
            return Resources.Multiplicator;
        }

    	public void OnShow()
    	{
    	}

        public void SetUnitOfWork(IUnitOfWork value)
        {
            UnitOfWork = value;
            // Creates a new repository.
            _multiplicatorRep = new MultiplicatorRepository(UnitOfWork);
        }

        public void Persist()
        {}

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
            throw new NotImplementedException();
        }

        private void AddMultiplicatorForm_AfterDefinitionSetAdded(object sender, MultiplicatorAddedEventArgs e)
        {
            if (e.Multiplicator != null)
            {
                _gridColumnHelper.SourceList.Add(e.Multiplicator);
                _multiplicatorRep.Add(e.Multiplicator);

                gridMultiplicator.RowCount = _gridColumnHelper.SourceList.Count;
                gridMultiplicator.Invalidate();
            }
        }
    }
}
