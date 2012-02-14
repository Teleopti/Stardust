using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;

namespace Teleopti.Ccc.AgentPortal.Requests.ShiftTrade
{
    public partial class ShiftTradeVisualView : BaseUserControl, IShiftTradeVisualView
    {
        private readonly ShiftTradeVisualPresenter _presenter;
        private IList<ShiftTradeDetailModel> _shiftTradeDetailModels;
        private ShiftTradeVisualGridPresenter _gridPresenter;
        
        public ShiftTradeVisualView()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                gridControlVisual.Properties.BackgroundColor = UserTexts.ThemeSettings.Default.StandardOfficeFormBackground;
            }
        }

        public ShiftTradeVisualView(ShiftTradeModel shiftTradeModel) : this()
        {
            _presenter = new ShiftTradeVisualPresenter(this, shiftTradeModel);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _presenter.Initialize();
        }

        public void SetDataSource(IList<ShiftTradeDetailModel> shiftTradeDetailModels)
        {
            gridControlVisual.BeginUpdate();
            _shiftTradeDetailModels = shiftTradeDetailModels;

            if (_gridPresenter == null)
            {
                _gridPresenter = new ShiftTradeVisualGridPresenter(gridControlVisual, _shiftTradeDetailModels);
            }
            else
            {
                _gridPresenter.UpdateDataSource(_shiftTradeDetailModels);
            }
            gridControlVisual.EndUpdate();

            gridControlVisual.Model.MergeCells.EvaluateMergeCells(GridRangeInfo.Table());
        }

        public void OnRefresh()
        {
            _presenter.OnRefresh();
        }

        public void EnableContent(bool enabled)
        {
            toolStripMenuItemRemoveDays.Enabled = enabled;
            toolStripMenuItemRemoveDays.Visible = enabled;
        }

        public ICollection<DateTime> SelectedDates()
        {
            ICollection<DateTime> selectedDates = new Collection<DateTime>();
            foreach (var shiftTradeDetailModel in _gridPresenter.SelectedShiftTradeDetailModels())
            {
                if (!selectedDates.Contains(shiftTradeDetailModel.TradeDate))
                {
                    selectedDates.Add(shiftTradeDetailModel.TradeDate);
                }
            }
            return selectedDates;
        }

        public event EventHandler RemoveSelectedDays;

        private void toolStripMenuItemRemoveDays_Click(object sender, EventArgs e)
        {
        	var handler = RemoveSelectedDays;
            if (handler!=null)
            {
                handler.Invoke(this,EventArgs.Empty);
            }
        }
    }
}
