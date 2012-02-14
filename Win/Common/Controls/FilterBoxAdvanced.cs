using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.Win.Common.Controls
{
    /// <summary>
    /// a popupbox for making complex filterrules
    /// </summary>
    public partial class FilterBoxAdvanced : BaseRibbonForm
    {
        private IList<FilterAdvancedSetting> _itemsToFilter;
        

        /// <summary>
        /// Occurs when [apply or ok is clicked].
        /// </summary>
        public event EventHandler<FilterBoxAdvancedEventArgs> FilterClicked;

    	public FilterBoxAdvanced()
        {
            InitializeComponent();
            SetTexts();
            SetColors();
            initalizeFilters();
            Load += FilterBoxAdvanced_Load;
        }

        private void initalizeFilters()
        {
            FilterAdvancedTupleItem requestTypeAbsenceItem = new FilterAdvancedTupleItem(Resources.RequestTypeAbsence, typeof(AbsenceRequest).ToString());
            FilterAdvancedTupleItem requestTypeTextRequestItem = new FilterAdvancedTupleItem(Resources.RequestTypeText, typeof(TextRequest).ToString());
            FilterAdvancedTupleItem requestTypeShiftTradeItem = new FilterAdvancedTupleItem(Resources.RequestTypeShiftTrade, typeof(ShiftTradeRequest).ToString());

            IList<FilterAdvancedTupleItem> requestTypeList = new List<FilterAdvancedTupleItem> { requestTypeAbsenceItem, requestTypeTextRequestItem, requestTypeShiftTradeItem };

            FilterAdvancedTupleItem statusApproved = new FilterAdvancedTupleItem(Resources.Approved, Resources.Approved);
            FilterAdvancedTupleItem statusPending = new FilterAdvancedTupleItem(Resources.Pending, Resources.Pending);
            FilterAdvancedTupleItem statusDenied = new FilterAdvancedTupleItem(Resources.Denied, Resources.Denied);
            IList<FilterAdvancedTupleItem> statusList = new List<FilterAdvancedTupleItem> { statusApproved, statusPending, statusDenied };

            FilterAdvancedTupleItem operandEquals = new FilterAdvancedTupleItem("=", RequestViewOperand.Equals);
            FilterAdvancedTupleItem operandGreaterThen = new FilterAdvancedTupleItem(">", RequestViewOperand.GreaterThen);
            FilterAdvancedTupleItem operandLesserThen = new FilterAdvancedTupleItem("<", RequestViewOperand.LessThen);

            FilterAdvancedTupleItem filterOnRequestType = new FilterAdvancedTupleItem(Resources.RequestType, RequestViewAdapterField.RequestTypeOfToString);
            FilterAdvancedTupleItem filterOnSeniority = new FilterAdvancedTupleItem(Resources.Seniority, RequestViewAdapterField.Seniority);
            FilterAdvancedTupleItem filterOnStatus = new FilterAdvancedTupleItem(Resources.Status, RequestViewAdapterField.StatusText);

            _itemsToFilter = new List<FilterAdvancedSetting>();
            _itemsToFilter.Add(new FilterAdvancedSetting(filterOnRequestType, new List<FilterAdvancedTupleItem> { operandEquals }, FilterCriteriaType.List, requestTypeList));
            _itemsToFilter.Add(new FilterAdvancedSetting(filterOnSeniority, new List<FilterAdvancedTupleItem> { operandEquals, operandGreaterThen, operandLesserThen }, FilterCriteriaType.Number, null));
            _itemsToFilter.Add(new FilterAdvancedSetting(filterOnStatus, new List<FilterAdvancedTupleItem> { operandEquals }, FilterCriteriaType.List, statusList));
        }
        /// <summary>
        /// Sets the colors.
        /// </summary>
        private void SetColors()
        {
            gradientPanel3.BackColor = ColorHelper.ControlPanelColor;
            gradientPanel1.BackgroundColor = ColorHelper.ControlGradientPanelBrush();
            gradientPanel2.BackgroundColor = ColorHelper.ControlGradientPanelBrush();
        }

        /// <summary>
        /// Handles the Load event of the FilterBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void FilterBoxAdvanced_Load(object sender, EventArgs e)
        {
            AddStartItem();          
        }

        /// <summary>
        /// Adds the start item.
        /// </summary>
        /// <remarks>
        private void AddStartItem()
        {
            var startitem = new FilterBoxItemAdvanced(AvailableSettings()) { PreHeader = SetPreheaderText()};
            startitem.RemoveMe += itemRemoveMe;
            //startitem.HideFirstItemRemoveButton();
            this.flowLayoutPanel1.Controls.Add(startitem);
        }

    	/// <summary>
        /// Sets the preheader text.
        /// </summary>
        /// <returns></returns>
        private string SetPreheaderText()
        {
        	if(flowLayoutPanel1.Controls.Count ==0)
                return UserTexts.Resources.FilterOn;
        	return UserTexts.Resources.AndThenOn;
        }

    	/// <summary>
        /// Handles the Click event of the buttonAdvAdd control.
        /// adds a FilterBoxAdvancedItem in the list and binds the remove event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonAdvAdd_Click(object sender, EventArgs e)
        {
            IList<FilterAdvancedSetting> availableSettings = AvailableSettings();
            if (availableSettings.Count > 0)
            {
                var item = new FilterBoxItemAdvanced(availableSettings) { PreHeader = SetPreheaderText() };
                item.RemoveMe += itemRemoveMe;
                flowLayoutPanel1.Controls.Add(item);
            }
        }

        /// <summary>
        /// the individual FilterBoxAdvancedItem invokes this eventhandler
        /// it removes the item that wants to be removed
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void itemRemoveMe(object sender, EventArgs e)
        {
            foreach (var item in flowLayoutPanel1.Controls.OfType<FilterBoxItemAdvanced>())
            {
                if (item == (FilterBoxItemAdvanced) sender)
                {
                    flowLayoutPanel1.Controls.Remove(item);
                    return;
                }
            }

        }

        /// <summary>
        /// invoke the filter event of the FilterBoxAdvanced without closing the form
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonAdvApply_Click(object sender, EventArgs e)
        {
            InvokeFilter();
        }

        /// <summary>
        /// Invokes the filter event
        /// collects a list of filter rules from its FilterBoxAdvancedItems
        /// </summary>
        private void InvokeFilter()
        {
            IList<FilterBoxAdvancedFilter> list = new List<FilterBoxAdvancedFilter>();

            foreach (var item in flowLayoutPanel1.Controls.OfType<FilterBoxItemAdvanced >())
            {
                list.Add(item.SelectedFilterSetting());
            }

        	var handler = FilterClicked;
            if (handler!= null)
                handler.Invoke(this, new FilterBoxAdvancedEventArgs(list));
        }

        /// <summary>
        /// Returns a list with settings not already used
        /// </summary>
        /// <returns></returns>
        public IList<FilterAdvancedSetting> AvailableSettings()
        {
            return _itemsToFilter;
        }

        /// <summary>
        /// Handles the Click event of the buttonAdvCancel control.
        /// closes the form without activating the filter
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// invoke the filter event of the FilterBoxAdvanced
        /// and closes the form
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonAdvOk_Click(object sender, EventArgs e)
        {
            InvokeFilter();
            this.Close();
        }

        /// <summary>
        /// Clear all filters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAdvClear_Click(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();
        }    
    }

    /// <summary>
    /// contains a list of FilterBoxAdvancedFilter 
    /// </summary>
    public class FilterBoxAdvancedEventArgs : EventArgs    
    {
        //private IList<FilterAdvancedTupleItem> _filterRules;
        private IList<FilterBoxAdvancedFilter> _filterRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortBoxEventArgs"/> class.
        /// add the list of filter rules - list[0] is the primary and so on
        /// </summary>
        /// <param name="sortRules">The sort rules.</param>
        public FilterBoxAdvancedEventArgs(IList<FilterBoxAdvancedFilter> filterRules)
        {
            this._filterRules = filterRules;
        }

        public IList<FilterBoxAdvancedFilter> FilterRules
        {
            get { return _filterRules; }
        }
    }
}
