﻿using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Rows
{
    public class IntervalHeaderGridRow:IGridRow
    {
        private readonly IList<IntervalDefinition> _intervals;
        private DateTime _baseDate;

        public IntervalHeaderGridRow(IList<IntervalDefinition> intervals)
        {
            _intervals = intervals;
        }

        public DateTime BaseDate
        {
            get { return _baseDate; }
            set { _baseDate = value; }
        }

	    public TimeSpan? Time(CellInfo cellInfo)
	    {
			if (cellInfo.ColIndex < cellInfo.RowHeaderCount)
				return null;

			return _intervals[cellInfo.ColIndex - cellInfo.RowHeaderCount].TimeSpan;
		}

        #region IGridRow Members

        public void QueryCellInfo(CellInfo cellInfo)
        {
            if (cellInfo.ColIndex < cellInfo.RowHeaderCount) return;
            if (_intervals.Count < 1) return;
            if ((cellInfo.ColIndex - cellInfo.RowHeaderCount) >= _intervals.Count) return;
            cellInfo.Style.BaseStyle = "Header";
        	cellInfo.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
        	cellInfo.Style.CellValue = _baseDate.Add(Time(cellInfo).Value).ToString("t", TeleoptiPrincipal.CurrentPrincipal.Regional.Culture);
        }

        public void SaveCellInfo(CellInfo cellInfo)
        {
        }

        public void OnSelectionChanged(GridSelectionChangedEventArgs e, int rowHeaders)
        {
        }

        #endregion
    }
}