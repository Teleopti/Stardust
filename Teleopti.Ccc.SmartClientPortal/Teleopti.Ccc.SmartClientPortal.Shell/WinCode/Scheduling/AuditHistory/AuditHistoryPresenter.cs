using System;
using System.ComponentModel;
using log4net;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AuditHistory
{
    public class AuditHistoryPresenter
    {
        private readonly IAuditHistoryView _view;
        private const int Colcount = 1;
        private const int ChangedByColWidth = 200;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuditHistoryPresenter));

        public AuditHistoryPresenter(IAuditHistoryView view, IAuditHistoryModel model)
        {
            if(view == null) throw new ArgumentNullException(nameof(view));
            if(model == null) throw new ArgumentNullException(nameof(model));

            _view = view;
            Model = model;
        }

        public IAuditHistoryModel Model { get; }

       
        public void GridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            InParameter.NotNull("e", e);
			if (Model.PageRows.Count == 0) return;
			if (e.RowIndex - 1 > Model.PageRows.Count)
                return;

            if (e.RowIndex == 0 && e.ColIndex == 1)
            {
                e.Style.CellType = "TimeLineHeaderCell";
                e.Style.Text = string.Empty;
                e.Style.WrapText = false;
                e.Style.Tag = MergedOrDefaultPeriod();   
            }

            if (e.RowIndex > 0 && e.ColIndex == 0)
            {
                e.Style.CellType = "RevisionChangedByCell";
                e.Style.CellValue = Model.PageRows[e.RowIndex - 1];
            }

            if (e.RowIndex > 0 && e.ColIndex > 0)
            {
                e.Style.CellType = "RevisionChangeCell";
                e.Style.Tag = MergedOrDefaultPeriod();
                e.Style.CellValue = Model.PageRows[e.RowIndex - 1];
            }    
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "fontHeight*2")]
        public int GridQueryRowHeight(int index, int height, int fontHeight, int rows)
        {
            if (index > 0)
                return fontHeight * rows;

            return height;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "size-200")]
        public int GridQueryColWidth(int index, int size)
        {
            if (index == 0)
                return ChangedByColWidth;
            
            return size - ChangedByColWidth;
        }

        public int GridQueryColCount()
        {
            return Colcount;
        }

        public int GridQueryRowCount()
        {
            return Model.PageRows.Count;
        }

        public void Close()
        {
            _view.CloseView();
        }

        public void  Restore(IScheduleDay scheduleDay)
        {
            Model.SelectedScheduleDay = scheduleDay;
            _view.CloseView();
        }

        public void Load()
        {
            _view.EnableView = false;
            _view.ShowView();
            _view.StartBackgroundWork(AuditHistoryDirection.InitializeAndFirst);
        }

        public void DoWork(DoWorkEventArgs e)
        {
            InParameter.NotNull("e", e);
            var direction = (AuditHistoryDirection)e.Argument;
            switch (direction)
            {
                case AuditHistoryDirection.InitializeAndFirst:
                    Model.First();
                    break;
                case AuditHistoryDirection.Previous:
                    new PreviousPageCommand(Model).Execute();
                    break;
                case AuditHistoryDirection.Next:
                    new NextPageCommand(Model).Execute();
                    break;
            }
        }

        public void WorkCompleted(RunWorkerCompletedEventArgs e)
        {
            if (DataSourceException(e))
            {
				Logger.Warn("A data source related issue occured",e.Error);
                Close();
            }
            else
            {
                if (e.Error != null)
                {
                    Logger.Error("Error while initiating schedule history view",e.Error);
                }
                RefreshView();
            }
        }

        private void RefreshView()
        {
            _view.LinkLabelEarlierStatus = new NextPageCommand(Model).CanExecute();
            _view.LinkLabelLaterStatus = new PreviousPageCommand(Model).CanExecute();
				_view.LinkLabelEarlierVisibility = Model.NumberOfPages > 1;
				_view.LinkLabelLaterVisibility = Model.NumberOfPages > 1;
            _view.EnableView = true;
            _view.ShowDefaultCursor();
            _view.RefreshGrid();
            _view.UpdateHeaderText();
            _view.UpdatePageOfStatusText();
            _view.SelectFirstRowOnGrid();
            _view.SetRestoreButtonStatus();
        }

        private bool DataSourceException(AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                var dataSourceException = e.Error as DataSourceException;
                if (dataSourceException == null)
                    return false;

                _view.ShowDataSourceException(dataSourceException);

                return true;
            }

            return false;    
        }

        public void StartBackgroundWork(AuditHistoryDirection direction)
        {
            _view.ShowWaitCursor();
            _view.EnableView = false;
            _view.StartBackgroundWork(direction);
        }

        public void LinkLabelEarlierClicked()
        {
            StartBackgroundWork(AuditHistoryDirection.Next);
        }

        public void LinkLabelLaterClicked()
        {
            StartBackgroundWork(AuditHistoryDirection.Previous);
        }

        public DateTimePeriod MergedOrDefaultPeriod()
        {
            var start = DateTime.MaxValue;
            var end = DateTime.MinValue;

            foreach (var row in Model.PageRows)
            {
                var visualLayerCollection = row.ScheduleDay.ProjectionService().CreateProjection();
                var period = visualLayerCollection.Period();
                if (period == null) continue;
                
                var p = period.Value;
                if (p.StartDateTime < start) start = p.StartDateTime;
                if (p.EndDateTime > end) end = p.EndDateTime;
            }

            if (start < end)
            {
                if (start.Minute != 0) start = start.AddMinutes(start.Minute * -1);
                if (end.Minute != 0) end = end.AddMinutes(60 - end.Minute);
               
                return new DateTimePeriod(start.AddHours(-1), end.AddHours(1));
            }

            DateTimePeriod scheduleDayPeriod = Model.CurrentScheduleDay.Period;
            start = scheduleDayPeriod.StartDateTime.AddHours(7);
            end = start.AddHours(11);

            return new DateTimePeriod(start, end);
        }
    }
}
