using System;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping
{
    public interface IFindPersonsView
    {
        DateTime FromDate { get; set; }
        DateTime ToDate { get; set; }
        string FindText { get; set; }
        TreeNodeAdvCollection Result { get; }
        void SetErrorOnEndDate(string errorValue);
        void SetErrorOnStartDate(string errorValue);
        void ClearDateErrors();
        bool TextBoxFindEnabled { get; set; }
    }
}