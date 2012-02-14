using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.WinCode.Common.Rows;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public interface ITaskOwnerGrid
    {
        bool HasColumns { get; }
        void RefreshGrid();
        AbstractDetailView Owner { get; set;}
        void GoToDate(DateTime theDate);
        DateTime GetLocalCurrentDate(int column);
        IDictionary<int,GridRow> EnabledChartGridRows{ get;}
        ReadOnlyCollection<GridRow> AllGridRows { get; }
        int MainHeaderRow{ get;}
        IList<GridRow> EnabledChartGridRowsMicke65();
        void SetRowVisibility(string key, bool enabled);

    }
}
