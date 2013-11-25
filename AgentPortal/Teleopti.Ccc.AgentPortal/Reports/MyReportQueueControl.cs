using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Cells;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Columns;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.Reports
{
    public partial class MyReportQueueControl : BaseUserControl
    {
        #region Fields

        private readonly List<AgentQueueStatDetailsDto> _source = new List<AgentQueueStatDetailsDto>();
        private SFGridColumnGridHelper<AgentQueueStatDetailsDto> _columnGridHelper;

        #endregion

        public MyReportQueueControl()
        {
            InitializeComponent();
            TimeSpanTicksHourMinutesCellModel model = new TimeSpanTicksHourMinutesCellModel(gridQueue.Model);
            model.UseSeconds = true;
            gridQueue.CellModels.Add("HourMinutes",model );
            if (!DesignMode)
            {
                SetTexts();
            }
        }

        private void MyReportQueueControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                LoadSourceCollection();
                BindForGrid();
            }
        }

        /// <summary>
        /// Loads the source collection.
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-10-14
        /// </remarks>
        private void LoadSourceCollection()
        {
            IList<AgentQueueStatDetailsDto> agentQueueStatDetailsDtos = MyReportControl.StateHolder.QueueCollection;

            if (agentQueueStatDetailsDtos != null)
            {
                foreach (AgentQueueStatDetailsDto dto in agentQueueStatDetailsDtos)
                {
                    _source.Add(dto);
                }
            }
        }

        /// <summary>
        /// Binds for grid.
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-10-14
        /// </remarks>
        private void BindForGrid()
        {
            //bind for the grid.
            ReadOnlyCollection<SFGridColumnBase<AgentQueueStatDetailsDto>> configGrid = ConfigureGrid();
            _columnGridHelper = new SFGridColumnGridHelper<AgentQueueStatDetailsDto>(gridQueue, configGrid, _source);
            //set column widths.
            gridQueue.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);

            // Unbind unnecessary event handlers
            gridQueue.KeyDown -= _columnGridHelper.grid_KeyDown;
            gridQueue.ClipboardPaste -= _columnGridHelper.grid_ClipboardPaste;
            gridQueue.SaveCellInfo -= _columnGridHelper.grid_SaveCellInfo;
        }

        /// <summary>
        /// Configures the grid.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-10-13
        /// </remarks>
        private ReadOnlyCollection<SFGridColumnBase<AgentQueueStatDetailsDto>> ConfigureGrid()
        {
            IList<SFGridColumnBase<AgentQueueStatDetailsDto>> gridColumns = new List<SFGridColumnBase<AgentQueueStatDetailsDto>>();

            gridQueue.Rows.HeaderCount = 0;
            // Grid must have a Header column
            gridColumns.Add(new SFGridRowHeaderColumn<AgentQueueStatDetailsDto>(string.Empty));

            gridColumns.Add(new SFGridStringColumn<AgentQueueStatDetailsDto>("QueueName", UserTexts.Resources.Name, 150));
            gridColumns.Add(new SFGridStringColumn<AgentQueueStatDetailsDto>("AnsweredContacts", UserTexts.Resources.AnsweredContacts, 150));
            gridColumns.Add(new SFGridHourMinutesColumn<AgentQueueStatDetailsDto>("AverageTalkTime", UserTexts.Resources.AverageTalkTime, 150));
            gridColumns.Add(new SFGridHourMinutesColumn<AgentQueueStatDetailsDto>("AfterContactWorkTime", UserTexts.Resources.AverageAfterCallWork, 150));
            gridColumns.Add(new SFGridHourMinutesColumn<AgentQueueStatDetailsDto>("AverageHandlingTime", UserTexts.Resources.AverageHandlingTime, 150));
            
            gridQueue.RowCount = gridRowCount();
            gridQueue.ColCount = gridColumns.Count - 1;  //col index starts on 0
            gridQueue.ColHiddenEntries.Add(new GridColHidden(0));
            return new ReadOnlyCollection<SFGridColumnBase<AgentQueueStatDetailsDto>>(gridColumns);
        }

        private int gridRowCount()
        {
            return _source.Count + gridQueue.Rows.HeaderCount;
        }

        /// <summary>
        /// Refreshes the control.
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-10-14
        /// </remarks>
        public void RefreshControl()
        {

            _columnGridHelper.SourceList.Clear();
            LoadSourceCollection();

            gridQueue.RowCount = _source.Count;
            gridQueue.Invalidate();
        }

    }
}
