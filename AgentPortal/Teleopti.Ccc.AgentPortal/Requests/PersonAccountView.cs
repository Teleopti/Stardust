using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Cells;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Columns;
using Teleopti.Ccc.AgentPortalCode.Requests;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.Requests
{
    public partial class PersonAccountView : BaseUserControl, IPersonAccountView
    {
        private readonly PersonAccountPresenter _presenter;
        private SFGridColumnGridHelper<PersonAccountModel> _sfGridColumnGridHelper;

        protected PersonAccountView()
        {
            InitializeComponent();
        }

        public PersonAccountView(ITeleoptiOrganizationService service,PersonDto personDto) : this()
        {
            _presenter = new PersonAccountPresenter(this, service, personDto);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _presenter.Initialize();
            gridControlPersonAccounts.BeginUpdate();
            InitializeGrid();
            gridControlPersonAccounts.EndUpdate();
        }

        public void SetDate(DateTime dateTime)
        {
            dateTimePickerAdvSelectedDate.Value = dateTime;
        }

        public void DataLoaded()
        {
            gridControlPersonAccounts.RowCount = _presenter.ItemCount;
            gridControlPersonAccounts.Invalidate();
        }

        public string DescriptionHeader { get; set; }
        public string PeriodFromHeader { get; set; }
        public string PeriodToHeader { get; set; }
        public string TypeOfValueHeader { get; set; }
        public string AccruedHeader { get; set; }
        public string UsedHeader { get; set; }
        public string RemainingHeader { get; set; }

        public void SetDateText(string text)
        {
            autoLabelDateSelection.Text = text;
        }

        private void InitializeGrid()
        {
            if (_sfGridColumnGridHelper != null) return; //Already initialized!
            var colWidth = (gridControlPersonAccounts.Width / 7) - 20;
            var dateOnlyCellModel = new DateOnlyReadOnlyCellModel(gridControlPersonAccounts.Model);
            gridControlPersonAccounts.CellModels.Add("DateOnly",dateOnlyCellModel);
            gridControlPersonAccounts.CellModels.Add("DescriptionNameCell",new TextReadOnlyCellModel(gridControlPersonAccounts.Model));
            IList<SFGridColumnBase<PersonAccountModel>> gridColumns = new List<SFGridColumnBase<PersonAccountModel>>();

            gridColumns.Add(new SFGridRowHeaderColumn<PersonAccountModel>(""));
            gridColumns.Add(new SFGridStringColumn<PersonAccountModel>("Description", DescriptionHeader, colWidth));
            gridColumns.Add(new SFGridDateOnlyColumn<PersonAccountModel>("PeriodFrom", PeriodFromHeader, colWidth));
            gridColumns.Add(new SFGridDateOnlyColumn<PersonAccountModel>("EndDate", PeriodToHeader, colWidth));
            gridColumns.Add(new SFGridStringColumn<PersonAccountModel>("TypeOfValue", TypeOfValueHeader, colWidth));
            gridColumns.Add(new SFGridStringColumn<PersonAccountModel>("Accrued", AccruedHeader, colWidth));
            gridColumns.Add(new SFGridStringColumn<PersonAccountModel>("Used", UsedHeader, colWidth));
            gridColumns.Add(new SFGridStringColumn<PersonAccountModel>("Remaining", RemainingHeader, colWidth));
            _sfGridColumnGridHelper = new SFGridColumnGridHelper<PersonAccountModel>(gridControlPersonAccounts, new ReadOnlyCollection<SFGridColumnBase<PersonAccountModel>>(gridColumns), _presenter.Items);
            gridControlPersonAccounts.ResizeColsBehavior = ((GridResizeCellsBehavior.ResizeSingle | GridResizeCellsBehavior.InsideGrid) | GridResizeCellsBehavior.OutlineHeaders);
            gridControlPersonAccounts.SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
        }

        private void dateTimePickerAdvSelectedDate_ValueChanged(object sender, EventArgs e)
        {
            _presenter.ChangeDate(dateTimePickerAdvSelectedDate.Value);
        }

        private void gridControlPersonAccounts_RowsRemoving(object sender, GridRangeRemovingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
