using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class ShiftCategorySettingsControl : BaseUserControl, ISettingPage
	{
		private readonly IList<ShiftCategoryModel> _shiftCatAdapterList;
		private ShiftCategoryRepository _shiftCatergoryRepository;
		private IUnitOfWork _unitOfWork;
		private readonly string _newShiftCatName = string.Empty;
		SFGridColumnGridHelper<ShiftCategoryModel> _gridHelper;
		private const string seperatorChar = ":";
		private const string newShiftCategoryNameFormat = "<{0} {1}>";
		private const string descriptionNameCell = "DescriptionNameCell";
		private const string colorPickerCell = "ColorPickerCell";
		private const string longName = "Name";
		private const string shortName = "ShortName";
		private const string displayColor = "DisplayColor";
		private const int nameColumnWidth = 180;
		private const int shortNameColumnWidth = 130;
		private const int colorNameColumnWidth = 140;

		private ShiftCategoryRepository ShiftCatReposiroty
		{
			get { return _shiftCatergoryRepository ?? (_shiftCatergoryRepository = ShiftCategoryRepository.DONT_USE_CTOR(_unitOfWork)); }
		}

		public ShiftCategorySettingsControl()
		{
			InitializeComponent();

			_newShiftCatName = Resources.NewShiftCategory;
			_shiftCatAdapterList = new List<ShiftCategoryModel>();
		}

		private void buttonNewShiftCategoryClick(object sender, EventArgs e)
		{
			_shiftCatAdapterList.Add(createShiftCategory());
			gridControlShiftCategory.RowCount++;

			//Get the current cell and move down on the same col
			int colIndex = (gridControlShiftCategory.Model.CurrentCellInfo == null)
							   ? 1
							   : gridControlShiftCategory.Model.CurrentCellInfo.ColIndex;
			gridControlShiftCategory.Focus();
			gridControlShiftCategory.CurrentCell.MoveTo(gridControlShiftCategory.RowCount, colIndex,
												 GridSetCurrentCellOptions.ScrollInView);

			gridControlShiftCategory.Invalidate();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Common.Configuration.ShiftCategorySettingsControl.ShowMyErrorMessage(System.String,System.String)")]
		private void buttonAdvDeleteShiftCategoryClick(object sender, EventArgs e)
		{
			IList<int> selectedList = getSelectedRowsToBeDeleted();
			if (selectedList == null || selectedList.Count <= 0) return;

			string text = string.Format(
				CurrentCulture,
				Resources.AreYouSureYouWantToDelete);

			string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);
			DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
			if (response != DialogResult.Yes) return;
			Cursor.Current = Cursors.WaitCursor;

			deleteShiftCategory(selectedList);

			Cursor.Current = Cursors.Default;
		}

		private void deleteShiftCategory(IList<int> selectedList)
		{
			IList<ShiftCategoryModel> source = getSource<ShiftCategoryModel>();
			IList<ShiftCategoryModel> toBeDeleted = new List<ShiftCategoryModel>();
			for (int i = 0; i <= (selectedList.Count - 1); i++)
			{
				toBeDeleted.Add(source[selectedList[i]]);
			}
			foreach (ShiftCategoryModel activity in toBeDeleted)
			{
				source.Remove(activity);
				ShiftCatReposiroty.Remove(activity.ContainedEntity);
			}
			gridControlShiftCategory.RowCount = source.Count;
			gridControlShiftCategory.Invalidate();
		}

		private void toolStripMenuItemAddFromClipboardClick(object sender, EventArgs e)
		{
			handlePasteNew();
		}

		public void Unload()
		{
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			gridControlShiftCategory.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControlShiftCategory.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
			labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void LoadControl()
		{
			loadSourceList();

			ReadOnlyCollection<SFGridColumnBase<ShiftCategoryModel>> shiftCatColumns = configureShiftCategoryGrid();

			_gridHelper = new SFGridColumnGridHelper<ShiftCategoryModel>(gridControlShiftCategory,
											shiftCatColumns,
											getSource<ShiftCategoryModel>(), false ) {AllowExtendedCopyPaste = true};

			gridControlShiftCategory.ColWidths[1] = nameColumnWidth;
			gridControlShiftCategory.ColWidths[2] = shortNameColumnWidth;
			gridControlShiftCategory.ColWidths[3] = colorNameColumnWidth;
		}

		private int getNextShiftCatergoryId()
		{
			string newShiftCategoryString = Resources.NewShiftCategory;
			int nextId;
			var itemIds = new List<int>();
			const string pattern = @"(\d+\.?\d*|\.\d+)";

			foreach (var shiftCatAdapter in _shiftCatAdapterList)
			{
				var str = shiftCatAdapter.Name;
				if (!Regex.IsMatch(str, newShiftCategoryString)) continue;
				MatchCollection matchCollection = Regex.Matches(str, pattern);
				if (matchCollection.Count > 0)
				{
					itemIds.Add(
						Convert.ToInt32(matchCollection[matchCollection.Count - 1].Value, CultureInfo.CurrentCulture));
				}
			}
			itemIds.Sort();

			if (itemIds.Count == 0)
			{
				nextId = 1;
			}
			else
			{
				nextId = itemIds[itemIds.Count - 1] + 1;
			}
			return nextId;
		}

		private ShiftCategoryModel createShiftCategory()
		{
			int nextCount = getNextShiftCatergoryId();

			// Formats the name.
			string name =
				string.Format(CultureInfo.InvariantCulture, newShiftCategoryNameFormat, _newShiftCatName, nextCount);
			string categoryShortName = Resources.NewShiftCategoryShortName;
			var desc = new Description(name, categoryShortName);

			IShiftCategory newShiftCat = new ShiftCategory(name) {Description = desc, DisplayColor = Color.DodgerBlue};

			ShiftCatReposiroty.Add(newShiftCat);

			return new ShiftCategoryModel(newShiftCat);
		}

		public void SaveChanges()
		{}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Scheduling);
		}

		public string TreeNode()
		{
			return Resources.ShiftCategoryHeader;
		}

		public void OnShow()
		{
			
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			_unitOfWork = value;
		}

		public void Persist()
		{}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			toolTip1.SetToolTip(buttonNewShiftCategory, Resources.AddShiftCategory);
			toolTip1.SetToolTip(buttonAdvDeleteShiftCategory, Resources.Delete);
		}


		private void loadSourceList()
		{
			var shiftCatList = ShiftCatReposiroty.LoadAll().OrderBy(s => s.Description.Name).ToList();
			_shiftCatAdapterList.Clear();

			foreach (IShiftCategory category in shiftCatList)
			{
				_shiftCatAdapterList.Add(new ShiftCategoryModel(category));
			}
		}

		private IList<T> getSource<T>()
		{
			var source = (IList<T>)_shiftCatAdapterList;
			return source;
		}

		private ReadOnlyCollection<SFGridColumnBase<ShiftCategoryModel>> configureShiftCategoryGrid()
		{
			IList<SFGridColumnBase<ShiftCategoryModel>> gridColumns =
				new List<SFGridColumnBase<ShiftCategoryModel>>();

			gridControlShiftCategory.CellModels.Add(descriptionNameCell,
											 new DescriptionNameCellModel(gridControlShiftCategory.Model));
			gridControlShiftCategory.CellModels.Add(colorPickerCell, new ColorPickerCellModel(gridControlShiftCategory.Model));
			gridControlShiftCategory.Rows.HeaderCount = 0;

			gridColumns.Add(new SFGridRowHeaderColumn<ShiftCategoryModel>(string.Empty));
			gridColumns.Add(new SFGridEditableTextColumn<ShiftCategoryModel>(longName, 50, Resources.Name));
			gridColumns.Add(new SFGridEditableTextColumn<ShiftCategoryModel>(shortName, 2, Resources.ShortName));
			gridColumns.Add(new SFGridColorPickerColumn<ShiftCategoryModel>(displayColor, Resources.Color));

			gridColumns.AppendAuditColumns();

			return new ReadOnlyCollection<SFGridColumnBase<ShiftCategoryModel>>(gridColumns);
		}

		private IList<int> getSelectedRowsToBeDeleted()
		{
			IList<int> selectedList = new List<int>();
			GridRangeInfoList selectedRangeInfoList = gridControlShiftCategory.Model.Selections.GetSelectedRows(false, false);
			foreach (GridRangeInfo rangeInfo in selectedRangeInfoList)
			{
				string[] rowSplit = rangeInfo.Info.Split(seperatorChar.ToCharArray());
				if(!rangeInfo.IsTable)
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
				   for(int i=0 ; i<gridControlShiftCategory.RowCount ; i++)
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

		private void handlePasteNew()
		{
			var clipHandler = GridHelper.ConvertClipboardToClipHandler();
			var s = new GridRangeStyle(gridControlShiftCategory.RowCount +1 , 1);

			int dif = clipHandler.RowSpan();// -(gridShiftCategory.RowCount - range.Top + 1);
			for (int i = 0; i < dif; i++)
			{
				buttonNewShiftCategory.PerformClick();
			}

			GridRangeInfo range = s.Range;
			
			//loop all rows in selection, step with height in clip
			for (int i = range.Top; i <= range.Bottom; i = i + clipHandler.RowSpan())
			{
				int row = i;

				handlePaste(clipHandler, range, i, row);
			}
		}

		private void handlePaste(ClipHandler clipHandler, GridRangeInfo range, int i, int row)
		{
			for (int j = 1; j <= gridControlShiftCategory.ColCount; j = j + clipHandler.ColSpan())
			{
				int col = j;

				if (row <= gridControlShiftCategory.Rows.HeaderCount || col <= gridControlShiftCategory.Cols.HeaderCount)
					continue;
				foreach (Clip clip in clipHandler.ClipList)
				{
					//check clip fits inside selected range, rows
					if (GridHelper.IsPasteRangeOk(range, gridControlShiftCategory, clip, i, j))
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

			GridStyleInfo gsi = gridControlShiftCategory[rowIndex, columnIndex];
			var clipValue = (string)clip.ClipObject;
			if (clipValue.Length <= gsi.MaxLength || gsi.MaxLength == 0)
				gsi.ApplyFormattedText(clipValue);
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.ShiftCategorySettings; }
		}

		private void shiftCategorySettingsControlLayout(object sender, LayoutEventArgs e)
		{
			gridControlShiftCategory.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
		}
	}
}
