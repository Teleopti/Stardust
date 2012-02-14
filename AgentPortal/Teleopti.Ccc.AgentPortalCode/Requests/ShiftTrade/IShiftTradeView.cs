using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade
{
    public interface IShiftTradeView
    {
        //PersonRequestActionType Action { get; set; }

        string Message { set; get; }
        string Subject { set; get; }
        ICollection<DateTime> SelectedDates { get; }
        void SetStatus(string value);
        void SetPersonName(string value);
        void SetLabelName(string value);
        void SetPanelAcceptDenyVisible(bool visible);
        void SetAcceptButtonEnabled(bool value);
        void SetDenyButtonEnabled(bool value);
        void SetDeleteButtonEnabled(bool value);
        void SetResponseTabEnabled(bool value);
        void SetOkButtonVisible(bool value);
        void SetDialogResult(DialogResult value);
        void SetReasonMessageVisibility(bool value);
        void Close();
        void DisableMessage();
        void SetInitialDate(DateTime initialDate);
        void RefreshVisualView();
        void ShowErrorMessage(string text, string caption);
        /// <summary>
        /// Gets or sets the reason text.
        /// </summary>
        /// <value>The reason text.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2010-06-02
        /// </remarks>
        string ReasonMessage { set; get; }
    }
}