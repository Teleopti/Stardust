using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Columns;
using Teleopti.Ccc.AgentPortal.Requests.FormHandler;
using Teleopti.Ccc.AgentPortalCode.Requests.RequestMaster;

namespace Teleopti.Ccc.AgentPortal.Requests.RequestMaster
{
    public partial class RequestMasterView : BaseUserControl, IRequestMasterView
    {
        private readonly RequestMasterPresenter _presenter;
        private SFGridColumnGridHelper<RequestDetailRow> _sfGridColumnGridHelper;
        private IList<RequestDetailRow> _dataSource;
        private const int MinGridWidth = 840;

        public RequestMasterView(RequestMasterModel model)
        {
            InitializeComponent();
            _presenter = new RequestMasterPresenter(this, model);
            _presenter.Initialize();
            gridControlRequestMaster.Visible = false;
        }

        private void RequestMasterView_Load(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                gridControlRequestMaster.Width = Parent.Width;
                if (gridControlRequestMaster.Width < MinGridWidth)
                    gridControlRequestMaster.Width = MinGridWidth;
            }
            initializeGrid();
            gridControlRequestMaster.Visible = true;
            contextMenuStripExRequestGrid.Items.Add(UserTexts.Resources.Open);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<RequestDetailRow> DataSource
        {
            get{ return _dataSource; }
            set
            {
                _dataSource = value;
                if (_sfGridColumnGridHelper!=null) reloadDataSource(value);
            }
        }

        public string MessageHeader { get; set; }
        public string RequestDateHeader { get; set; }
        public string RequestTypeHeader { get; set; }
        public string RequestStatusHeader { get; set; }
        public string DetailsHeader { get; set; }
        public string SubjectHeader { get; set; }
        public string LastChangedHeader { get; set; }
        public int CurrentSelectedRow { get;set; }

        private void initializeGrid()
        {
            var colWidth = (gridControlRequestMaster.Width-30) / 7;
            IList<SFGridColumnBase<RequestDetailRow>> gridColumns = new List<SFGridColumnBase<RequestDetailRow>>();

            gridColumns.Add(new SFGridRowHeaderColumn<RequestDetailRow>(""));
            gridColumns.Add(new SFGridStringColumn<RequestDetailRow>("RequestDate", RequestDateHeader, colWidth-16));
            gridColumns.Add(new SFGridStringColumn<RequestDetailRow>("RequestType", RequestTypeHeader, colWidth-17));
            gridColumns.Add(new SFGridRequestStatusColumn<RequestDetailRow>("RequestStatus", RequestStatusHeader, colWidth-17));
            gridColumns.Add(new SFGridStringColumn<RequestDetailRow>("Details", DetailsHeader, colWidth+50));
            gridColumns.Add(new SFGridStringColumn<RequestDetailRow>("Subject", SubjectHeader, colWidth));
            gridColumns.Add(new SFGridStringCellColumn<RequestDetailRow>("Message", MessageHeader, colWidth));
            gridColumns.Add(new SFGridHourMinutesColumn<RequestDetailRow>("LastChanged", LastChangedHeader, colWidth));
            _sfGridColumnGridHelper = new SFGridColumnGridHelper<RequestDetailRow>(gridControlRequestMaster, new ReadOnlyCollection<SFGridColumnBase<RequestDetailRow>>(gridColumns), (List<RequestDetailRow>) DataSource);
            gridControlRequestMaster.ResizeColsBehavior = ((GridResizeCellsBehavior.ResizeSingle | GridResizeCellsBehavior.InsideGrid)| GridResizeCellsBehavior.OutlineHeaders);
            gridControlRequestMaster.SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
        }

        private void reloadDataSource(IList<RequestDetailRow> newSource)
        {
            _sfGridColumnGridHelper.SourceList.Clear();
            foreach (RequestDetailRow details in newSource)
            {
                _sfGridColumnGridHelper.SourceList.Add(details);
            }
			gridControlRequestMaster.Selections.Clear();
            gridControlRequestMaster.Refresh();
        }

        private void gridControlRequestMaster_CellClick(object sender, GridCellClickEventArgs e)
        {
            CurrentSelectedRow = e.RowIndex - (gridControlRequestMaster.Cols.HeaderCount + 1); //syncfusion header index?!?!
            if (e.RowIndex != 0 || e.ColIndex == 0)
            {
                e.Cancel = true;
                return;
            }

            contextMenuStripExRequestGrid.Close();
            string sortMember = gridControlRequestMaster[1, e.ColIndex].DisplayMember;
            ListSortDirection dir = _sfGridColumnGridHelper.SortDirection(sortMember);
            _presenter.SortByColumn(sortMember, dir);
        }

        private void gridControlRequestMaster_CellDoubleClick(object sender, GridCellClickEventArgs e)
        {
            if (e.RowIndex <= 0)
            {
                contextMenuStripExRequestGrid.Close();
                return;
            }
            modifyRequest();
        }

        private void modifyRequest()
        {
            IList<RequestDetailRow> selectedRows = _sfGridColumnGridHelper.GetSelectedObjects();
            if (selectedRows.Count == 0) return;

            RequestDetailRow detailRow = selectedRows[0];
            if (detailRow == null) return;

            new PersonRequestFormHandler(this).ShowRequestScreen(detailRow.PersonRequest);
            gridControlRequestMaster.BeginUpdate();
            _presenter.LoadDataSource();
            gridControlRequestMaster.EndUpdate();
        }

        private void gridControlRequestMaster_ResizingColumns(object sender, GridResizingColumnsEventArgs e)
        {
            if (e.Reason == GridResizeCellsReason.DoubleClick)
            {
                gridControlRequestMaster.ColWidths.ResizeToFit(gridControlRequestMaster.Selections.Ranges[0], GridResizeToFitOptions.IncludeCellsWithinCoveredRange);
                e.Cancel = true;
            }
        }

        private void contextMenuStripExRequestGrid_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            modifyRequest();
        }

        private void contextMenuStripExRequestGrid_Opening(object sender, CancelEventArgs e)
        {
            contextMenuStripExRequestGrid.Items[0].Enabled = true;
            var rows = _sfGridColumnGridHelper.GetSelectedObjects();
            
            //if more than 1 row selectd disable menu
            if (rows.Count != 1)
                contextMenuStripExRequestGrid.Items[0].Enabled = false;
        }

        public void ModifySelectedRequest()
        {
            modifyRequest();
        }
    }
}