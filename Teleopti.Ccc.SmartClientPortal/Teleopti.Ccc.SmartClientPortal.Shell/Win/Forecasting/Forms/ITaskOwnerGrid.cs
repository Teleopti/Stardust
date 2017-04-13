using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public interface ITaskOwnerGrid
    {
        bool HasColumns { get; }
        void RefreshGrid();
        AbstractDetailView Owner { get; set;}
        void GoToDate(DateOnly theDate);
        DateOnly GetLocalCurrentDate(int column);
        IDictionary<int,GridRow> EnabledChartGridRows{ get;}
        ReadOnlyCollection<GridRow> AllGridRows { get; }
        int MainHeaderRow{ get;}
        IList<GridRow> EnabledChartGridRowsMicke65();
        void SetRowVisibility(string key, bool enabled);

    }
}
