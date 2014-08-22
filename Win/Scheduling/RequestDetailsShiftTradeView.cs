using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class RequestDetailsShiftTradeView : BaseUserControl
	{
	    private readonly RequestDetailsShiftTradePresenter _presenter;

		public RequestDetailsShiftTradeView()
		{
			InitializeComponent();
			if (!DesignMode)
            {
				SetTexts();
                gridControlSchedule.Properties.BackgroundColor = UserTexts.ThemeSettings.Default.StandardOfficeFormBackground;
			}
		}

		public RequestDetailsShiftTradeView(IPersonRequestViewModel model, IScheduleDictionary schedules) : this()
		{
			_presenter = new RequestDetailsShiftTradePresenter(model, schedules, new RangeProjectionService());
			_presenter.Initialize();
			gridControlSchedule.BeginUpdate();

			gridControlSchedule.QueryCellInfo += gridControlSchedules_QueryCellInfo;
			gridControlSchedule.Resize += gridControlSchedule_Resize;
			gridControlSchedule.VerticalScroll +=new ScrollEventHandler(gridControlSchedule_VerticalScroll);

            gridControlSchedule.BackColor = ColorHelper.GridControlGridInteriorColor();
            gridControlSchedule.Properties.BackgroundColor = ColorHelper.GridControlGridExteriorColor();

            var timeZoneInfo = _presenter.TimeZone;

            gridControlSchedule.Model.CellModels.Add("ProjectionHeaderCell", new VisualProjectionColumnHeaderCellModel(gridControlSchedule.Model, timeZoneInfo));
			gridControlSchedule.Model.CellModels.Add("ProjectionCell", new VisualProjectionNoHScrollBarCellModel(gridControlSchedule.Model, timeZoneInfo));
			gridControlSchedule.ColWidths[2] = gridControlSchedule.ViewLayout.HscrollAreaBounds.Width;

			gridControlSchedule.ColCount = 2;
			gridControlSchedule.RowCount = _presenter.RowCount;
			gridControlSchedule.Rows.HeaderCount = 0;
			gridControlSchedule.Cols.HeaderCount = 1;
			gridControlSchedule.Cols.FrozenCount = 1;
			gridControlSchedule.DefaultRowHeight = 25;
			gridControlSchedule.RowHeights.SetSize(0, 35);
			gridControlSchedule.Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeRowsInColumn;

			gridControlSchedule.Cols.Size.ResizeToFit(GridRangeInfo.Cols(0, 1), GridResizeToFitOptions.IncludeHeaders);
			gridControlSchedule.ColWidths[2] = gridControlSchedule.ViewLayout.HscrollAreaBounds.Width;

			GridHelper.GridStyle(gridControlSchedule);
			gridControlSchedule.EndUpdate();
		}

		private void gridControlSchedule_VerticalScroll(object sender, ScrollEventArgs e)
		{
			gridControlSchedule.Refresh();
			gridControlSchedule.InvalidateRange(GridRangeInfo.Table());
		}
		
		private void gridControlSchedule_Resize(object sender, EventArgs e)
		{
			gridControlSchedule.ColWidths[2] = gridControlSchedule.ViewLayout.HscrollAreaBounds.Width;
		}

        private void gridControlSchedules_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (_presenter == null)
            {
                e.Style.CellValue = string.Empty;
                return;
            }
			
            if (_presenter.ParticipantList != null)
            {
				if (e.RowIndex > _presenter.RowCount) return;
                if (e.ColIndex > 1 && e.RowIndex > 0)
				{
					e.Style.Tag = _presenter.CurrentPeriods[e.RowIndex - 1];
                    ContactPersonViewModel personViewModel = _presenter.ParticipantList[e.RowIndex - 1];
					if (_presenter.IsDayOff(personViewModel, _presenter.CurrentPeriods[e.RowIndex - 1].StartDateTime.Date))
                    {
                        e.Style.CellValue = UserTexts.Resources.DayOff;
                        e.Style.Enabled = false;
                        e.Style.Font.Bold = true;
                        e.Style.Font.Italic = true;
                        e.Style.BackColor = Color.LightGray;
                        e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
                        e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                    }
                    else
                    {
						IEnumerable<IVisualLayer> visualLayers = _presenter.GetVisualLayersForPerson(personViewModel, _presenter.CurrentPeriods[e.RowIndex - 1]);
                        if (!visualLayers.IsEmpty())
                        {
                            e.Style.CellType = "ProjectionCell";
                            e.Style.CellValue = visualLayers;
                        }
                    }
				}
				if (e.ColIndex == 0 && e.RowIndex > 0)
				{
					e.Style.CellValue = TimeZoneHelper.ConvertFromUtc(_presenter.CurrentPeriods[e.RowIndex - 1].StartDateTime).ToShortDateString();
				}
                if (e.ColIndex == 1 && e.RowIndex > 0)
                {
                    ContactPersonViewModel personViewModel = _presenter.ParticipantList[e.RowIndex - 1];
                    e.Style.CellValue = personViewModel.FullName;
                }
			}
			
            if (e.ColIndex > 1 && e.RowIndex == 0)
            {
                e.Style.CellType = "ProjectionHeaderCell";
                e.Style.Text = string.Empty;
            	e.Style.WrapText = false;
            	e.Style.Tag = _presenter.DisplayPeriod;
			}

            e.Handled = true;
        }
		
		public override void Refresh()
		{
			gridControlSchedule.InvalidateRange(GridRangeInfo.Table());
			base.Refresh();
		}
	}
}
