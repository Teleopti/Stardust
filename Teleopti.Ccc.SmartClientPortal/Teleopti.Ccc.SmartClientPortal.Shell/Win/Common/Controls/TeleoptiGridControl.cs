using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms;
using Teleopti.Ccc.Win.Forecasting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.Win;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
	public class TeleoptiGridControl : GridControl, ITeleoptiGridControl
	{
		internal static readonly Color ColorHolidayCell = ColorHelper.GridControlGridHolidayCellColor();
		internal static readonly Color ColorHolidayHeader = ColorHelper.GridControlGridHolidayHeaderColor();
		internal static readonly Color ColorEditableCell = ColorHelper.GridControlGridInteriorColor();
		internal static readonly Color ColorOutsideGridBackground = ColorHelper.GridControlGridExteriorColor();
		internal static readonly FontStyle FontClosedCell = GuiSettings.ClosedCellFontStyle();
		internal static readonly Color ColorFontClosedCell = GuiSettings.ClosedCellFontColor();
		private static readonly object Lock = new object();
		private static ForecasterSettings _currentSettings;
		private readonly IList<INumericCellModelWithDecimals> _numericCellModelWithDecimalses = new List<INumericCellModelWithDecimals>();
		private int _numberOfDecimals;
		private bool _teleoptiStyling;

		public event EventHandler<DataToChartEventArgs> DataToChart;

		protected void TriggerDataToChart(GridRangeInfo gridRangeInfo)
		{
			  var handler = DataToChart;
			if (handler != null &&
				gridRangeInfo.Height==1)
			{
				var eventArgs = new DataToChartEventArgs(
					Model[gridRangeInfo.Top, gridRangeInfo.Left].CellModel,
					Model[gridRangeInfo.Top, gridRangeInfo.Left - 1].CellValue.ToString(),
					gridRangeInfo.Top,
					GetRowDataForChart(gridRangeInfo));
				
				handler.Invoke(this,eventArgs);
			}
		}

		public void InitializeAllGridRowsToChart()
		{
			for (int i = ColHeaderCount; i <= RowCount; i++)
			{
				if (!IsChartRow(i)) continue;
				var range = GridRangeInfo.Cells(i, RowHeaderCount, i, ColCount);
				TriggerDataToChart(range);
			}
		}

		protected virtual bool IsChartRow(int rowIndex)
		{
			return true;
		}

		protected virtual IDictionary<DateTime, double> GetRowDataForChart(GridRangeInfo gridRangeInfo)
		{
			return new Dictionary<DateTime, double>();
		}

		public event EventHandler<GridRangePasteEventArgs> GridRangePasted;

		protected virtual void TriggerGridRangePasted(GridRangeInfo gridRangeInfo)
		{
			var handler = GridRangePasted;
			if (handler !=null)
			{
				var eventArgs = new GridRangePasteEventArgs();
				eventArgs.GridRange = gridRangeInfo;
				handler.Invoke(this, eventArgs);
			}
		}

		public virtual WorkingInterval WorkingInterval { get { return WorkingInterval.Day; } }
		public virtual TimeSpan ChartResolution { get { return TimeSpan.FromHours(1); } }
		public virtual DateTime FirstDate { get { return Domain.InterfaceLegacy.Domain.DateHelper.MinSmallDateTime; } }
		public virtual DateTime LastDate { get { return Domain.InterfaceLegacy.Domain.DateHelper.MinSmallDateTime; } }

		public TeleoptiGridControl()
		{
			initializeDefaultValues();
			initializeGrid();

			ClipboardPaste += teleoptiGridControlClipboardPaste;
			KeyDown += teleoptiGridControlKeyDown;
			
		}

		[Browsable(true)]
		public bool TeleoptiStyling
		{
			get { return _teleoptiStyling; }
			set
			{
				if(value)
					GridHelper.GridStyle(this);

					_teleoptiStyling = value;
			}
		}

		
		public static ForecasterSettings CurrentForecasterSettings()
		{
			lock (Lock)
			{
				if (_currentSettings == null)
				{
					using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
					{
						_currentSettings = PersonalSettingDataRepository.DONT_USE_CTOR(uow).FindValueByKey("Forecaster",
																								 new ForecasterSettings());
						return _currentSettings;
					}
				}
				return _currentSettings;
			}
		}

		private void initializeDefaultValues()
		{

			if (!StateHolderReader.IsInitialized)
			{
				//For use in editor
				_numberOfDecimals = 1;
				return;
			}

			_numberOfDecimals = CurrentForecasterSettings().NumericCellVariableDecimals;
			
		}

		private void initializeGrid()
		{
			CellModels.Add("NumericServiceTargetLimitedCell", initializeCallServiceTargetNumericCell());
			CellModels.Add("NumericWorkloadIntradayTaskLimitedCell", initializeCallWorkloadIntradayTaskNumericCell());
			CellModels.Add("NumericWorkloadDayTaskLimitedCell", initializeCallWorkloadDayTaskNumericCell());
			CellModels.Add("NumericWorkloadWeekTaskLimitedCell", initializeCallWorkloadWeekTaskNumericCell());
			CellModels.Add("NumericWorkloadMonthTaskLimitedCell", initializeCallWorkloadMonthTaskNumericCell());
			CellModels.Add("NumericCell", initializeCallNumericCell());
			CellModels.Add("NullableNumericCell", initializeNullableNumericCell());
			CellModels.Add("NumericTwoDecimalCell", initializeCallNumericTwoDecimalCell());
			CellModels.Add("PercentCell", initializeCallPercentCell());
			CellModels.Add("MultiSitePercentCell", initializeMultiSitePercentCell());
			CellModels.Add("PercentWithNegativeCell", initializePercentWithNegativeCell());
			CellModels.Add("PercentShrinkageCell", initializeShrinkagePercentCell());
			CellModels.Add("PercentEfficiencyCell", initializeEfficiencyPercentCell());
			CellModels.Add("ServicePercentCell", initializeServicePercentCell());
			CellModels.Add("TimeSpanTotalSecondsReadOnlyCell", initializeTotalSecondsReadOnlyCell());
			CellModels.Add("NumericReadOnlyCell", initializeCallNumericReadOnlyCell());
			CellModels.Add("PercentReadOnlyCell", initializeCallPercentReadOnlyCell());
			CellModels.Add("TimeSpanTotalSecondsCell", initializeTotalSeconds());
			CellModels.Add("PositiveTimeSpanTotalSecondsCell", initializePositiveTotalSeconds());
			CellModels.Add("IntegerCell", initializeIntegerCell());
			CellModels.Add("PositiveIntegerCell", initializePositiveIntegerCell());
			CellModels.Add("IntegerMinMaxAgentCell", initializeCallMinMaxAgentIntegercCell());
			CellModels.Add("TimeSpanReadOnlyCell",initializeTimeSpanReadOnlyCell());
			CellModels.Add("TimeSpanLongHourMinutesCellModel", InitializeTimeSpanLongHourMinutesCellModel());
			CellModels.Add("TimeSpanLongHourMinutesOnlyPositiveCellModel", initializeTimeSpanLongHourMinutesPositiveCellModel());
			CellModels.Add("TimeSpanLongHourMinuteSecondOnlyPositiveCellModel", initializeTimeSpanLongHourMinuteSecondPositiveCellModel());
			CellModels.Add("TimeSpanLongHourMinutesStaticCellModel", initializeTimeSpanLongHourMinutesStaticCellModel());
			CellModels.Add("IntegerReadOnlyCell", initializeNumericNoDecimalsReadOnlyCell());
			CellModels.Add("PercenFromPercentReadOnlyCellModel", initializeCallPercentReadOnlyPercentCell());

			BackColor = ColorEditableCell;
			Properties.BackgroundColor = ColorOutsideGridBackground;
		}

		private NullableNumericCellModel initializeNullableNumericCell()
		{
			var cellModel = new NullableNumericCellModel(Model) { NumberOfDecimals = _numberOfDecimals, MinValue = 0, MaxValue = 99999999d };
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}

		private GridCellModelBase initializeTimeSpanReadOnlyCell()
		{
			return new TimeSpanDurationStaticCellModel(Model);
		}

		private TimeSpanDurationCellModel InitializeTimeSpanLongHourMinutesCellModel()
		{
			return new TimeSpanDurationCellModel(Model);
		}

		private GridCellModelBase initializeTimeSpanLongHourMinuteSecondPositiveCellModel()
		{
			return new TimeSpanDurationCellModel(Model) {OnlyPositiveValues = true, DisplaySeconds = true};
		}

		private TimeSpanDurationCellModel initializeTimeSpanLongHourMinutesPositiveCellModel()
		{
			return new TimeSpanDurationCellModel(Model) {OnlyPositiveValues = true};
		}

		private TimeSpanDurationStaticCellModel initializeTimeSpanLongHourMinutesStaticCellModel()
		{
			return new TimeSpanDurationStaticCellModel(Model) {DisplaySeconds = true};
		}

		private NumericCellModel initializeCallNumericTwoDecimalCell()
		{
			return new NumericCellModel(Model) {NumberOfDecimals = 2, MinValue = 0, MaxValue = 999};
		}

		//6 digits + decimals
		private NumericCellModel initializeCallWorkloadIntradayTaskNumericCell()
		{
			var cellModel = new NumericCellModel(Model)
			{
				NumberOfDecimals = _numberOfDecimals,
				MinValue = 0,
				MaxValue = 999999
			};
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}

		//7 digits + decimals
		private NumericCellModel initializeCallWorkloadDayTaskNumericCell()
		{
			var cellModel = new NumericCellModel(Model)
			{
				NumberOfDecimals = _numberOfDecimals,
				MinValue = 0,
				MaxValue = 9999999
			};
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}

		//8 digits + decimals
		private NumericCellModel initializeCallWorkloadWeekTaskNumericCell()
		{
			var cellModel = new NumericCellModel(Model)
			{
				NumberOfDecimals = _numberOfDecimals,
				MinValue = 0,
				MaxValue = 99999999
			};
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}

		//9 digits + decimals
		private NumericCellModel initializeCallWorkloadMonthTaskNumericCell()
		{
			var cellModel = new NumericCellModel(Model)
			{
				NumberOfDecimals = _numberOfDecimals,
				MinValue = 0,
				MaxValue = 999999999
			};
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}

		private NumericCellModel initializeCallServiceTargetNumericCell()
		{
			var cellModel = new NumericCellModel(Model) {NumberOfDecimals = _numberOfDecimals, MinValue = 0.1, MaxValue = 5000};
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}
		
		private IntegerCellModel initializeCallMinMaxAgentIntegercCell()
		{
			return new IntegerCellModel(Model) {MinValue = 0, MaxValue = 2000};
		}

		private GridCellModelBase initializeCallPercentReadOnlyCell()
		{
			return new PercentReadOnlyCellModel(Model) {NumberOfDecimals = 0};
		}

		private GridCellModelBase initializeIntegerCell()
		{
			return new IntegerCellModel(Model);
		}

		private GridCellModelBase initializePositiveIntegerCell()
		{
			return new IntegerCellModel(Model) {OnlyPositiveValues = true};
		}

		private GridCellModelBase initializeCallPercentReadOnlyPercentCell()
		{
			return new PercentReadOnlyCellModel(Model) {NumberOfDecimals = 0};
		}

		private GridCellModelBase initializeCallNumericReadOnlyCell()
		{
			var cellModel = new NumericReadOnlyCellModel(Model) {NumberOfDecimals = _numberOfDecimals};
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}

		private GridCellModelBase initializeNumericNoDecimalsReadOnlyCell()
		{
			return new NumericReadOnlyCellModel(Model) {NumberOfDecimals = 0};
		}

		private GridCellModelBase initializeTotalSecondsReadOnlyCell()
		{
			var cellModel = new TimeSpanTotalSecondsStaticCellModel(Model)
			{
				NumberOfDecimals = _numberOfDecimals
			};
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}

		private GridCellModelBase initializeCallPercentCell()
		{
			return new PercentCellModel(Model) {NumberOfDecimals = 0};
		}

		private GridCellModelBase initializeMultiSitePercentCell()
		{
			return new PercentCellModel(Model) {NumberOfDecimals = 2, MinMax = new MinMax<double>(0.00, 1)};
		}

		private GridCellModelBase initializePercentWithNegativeCell()
		{
			return new PercentCellModel(Model)
			{
				NumberOfDecimals = 1,
				MinMax = new MinMax<double>(-1.0d, 10.0d)
			};
		}

		private GridCellModelBase initializeServicePercentCell()
		{
			return new PercentCellModel(Model) {NumberOfDecimals = 0, MinMax = new MinMax<double>(0.01, 1)};
		} 

		private GridCellModelBase initializeShrinkagePercentCell()
		{
			return new PercentCellModel(Model)
			{
				NumberOfDecimals = 0,
				MinMax = new MinMax<double>(0.00, 0.99)
			};
		}

		private GridCellModelBase initializeEfficiencyPercentCell()
		{
			return new PercentCellModel(Model) {NumberOfDecimals = 0, MinMax = new MinMax<double>(0.01, 1)};
		}

		private NumericCellModel initializeCallNumericCell()
		{
			var cellModel = new NumericCellModel(Model) {NumberOfDecimals = _numberOfDecimals};
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}

		private TimeSpanTotalSecondsCellModel initializeTotalSeconds()
		{
			var cellModel = new TimeSpanTotalSecondsCellModel(Model) {NumberOfDecimals = _numberOfDecimals};
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}

		private TimeSpanTotalSecondsCellModel initializePositiveTotalSeconds()
		{
			var cellModel = new TimeSpanTotalSecondsCellModel(Model)
			{
				NumberOfDecimals = _numberOfDecimals,
				OnlyPositiveValues = true
			};
			_numericCellModelWithDecimalses.Add(cellModel);
			return cellModel;
		}

		private void teleoptiGridControlClipboardPaste(object sender, GridCutPasteEventArgs e)
		{
			e.IgnoreCurrentCell = true;
			HandlePaste();
			e.Handled = true;
		}

		void teleoptiGridControlKeyDown(object sender, KeyEventArgs e)
		{
			GridHelper.HandleSelectionKeys(this, e);
		}

		protected override bool ProcessDialogKey(Keys key)
		{
			if (key == Keys.Tab)
			{
				Parent.Parent.SelectNextControl(this, true, true, true, true);
				return true;
			}
			return base.ProcessDialogKey(key);
		}

		public virtual void HandlePaste()
		{
			ClipHandler clipHandler = GridHelper.ConvertClipboardToClipHandler();

			if (clipHandler.ClipList.Count > 0)
			{		
				BeginUpdate();
				OnBeforePaste();
				GridRangeInfoList rangelist = GridHelper.GetGridSelectedRanges(this, true);

				int clipRowSpan = clipHandler.RowSpan();
				int clipColSpan = clipHandler.ColSpan();

				foreach (GridRangeInfo range in rangelist)
				{
					//loop all rows in selection, step with height in clip
					for (int i = range.Top; i <= range.Bottom; i = i + clipRowSpan)
					{
						int row = i;

						//loop all columns in selection, step with in clip
						for (int j = range.Left; j <= range.Right; j = j + clipColSpan)
						{
							int col = j;

							if (row > Rows.HeaderCount && col > Cols.HeaderCount)
							{

								foreach (Clip clip in clipHandler.ClipList)
								{
									//check clip fits inside selected range, rows
									if (GridHelper.IsPasteRangeOk(range, this, clip, i, j))
									{
										Paste(clip, row + clip.RowOffset, col + clip.ColOffset);
									}
								}
							}
						}
					}
				}
				EndUpdate();
				if (rangelist.Count > 0)
				{
					int firstRow = rangelist.ActiveRange.Top;
					int firstCol = rangelist.ActiveRange.Left;
					int lastRow = firstRow + clipRowSpan - 1;
					int lastCol = firstCol + clipColSpan - 1;
					if (lastRow > RowCount)
					{
						lastRow = RowCount;
					}
					if (lastCol > ColCount)
					{
						lastCol = ColCount;
					}
					GridRangeInfo pastedRange = GridRangeInfo.Auto(firstRow, firstCol, lastRow, lastCol);
					RefreshRange(pastedRange);
					TriggerGridRangePasted(pastedRange);
				}
				OnAfterPaste();
				Refresh();
			}
		}

		public virtual void Paste(Clip clip, int rowIndex, int columnIndex)
		{
			if (columnIndex == int.MinValue)
			{
				throw new ArgumentOutOfRangeException("columnIndex", "columnIndex must be larger than Int32.MinValue");
			}
			
			GridStyleInfo gsi = this[rowIndex, columnIndex];
			var clipValue = (string) clip.ClipObject;
			if (clipValue.Length <= gsi.MaxLength || gsi.MaxLength == 0)
				gsi.ApplyFormattedText(clipValue);
		}

		protected static TextManager GetManager(AbstractDetailView owner)
		{
			var manager = new TextManager(owner.SkillType);
			return manager;
		}

		public void AddCoveredRange(GridRangeInfo rangeInfo)
		{
			CoveredRanges.Add(rangeInfo);
		}

		protected virtual void OnBeforePaste()
		{
		}

		protected virtual void OnAfterPaste()
		{
		}

		protected CellInfo GetCellInfo(GridStyleInfo style,int colIndex,int rowIndex)
		{
			return new CellInfo
					   {
						   ColCount = ColCount,
						   ColHeaderCount = ColHeaderCount,
						   ColIndex = colIndex,
						   Handled = false,
						   RowCount = RowCount,
						   RowHeaderCount = RowHeaderCount,
						   RowIndex = rowIndex,
						   Style = style
					   };
		}

		public void ChangeNumberOfDecimals(int changeCount)
		{
			foreach (INumericCellModelWithDecimals numericCellModelWithDecimals in _numericCellModelWithDecimalses)
			{
				var currentNumberOfDecimals = numericCellModelWithDecimals.NumberOfDecimals;
				numericCellModelWithDecimals.NumberOfDecimals = (currentNumberOfDecimals+changeCount).LimitRange(0,9);
			}
			Refresh(true);
		}

		protected int ColHeaderCount
		{
			get { return Rows.HeaderCount + 1; }
		}

		protected int RowHeaderCount
		{
			get { return Cols.HeaderCount + 1; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ClipboardPaste -= teleoptiGridControlClipboardPaste;
				KeyDown -= teleoptiGridControlKeyDown;
				ResetVolatileData();
			}
			base.Dispose(disposing);
		}
	}
}
