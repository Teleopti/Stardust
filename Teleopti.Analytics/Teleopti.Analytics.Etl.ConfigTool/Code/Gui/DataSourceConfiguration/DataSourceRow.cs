using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Code.Gui.DataSourceConfiguration
{
    public class DataSourceRow
    {
        private DataSourceRow() { }

        public DataSourceRow(IDataSourceEtl dataSourceEtl, int rowIndex)
            : this()
        {
            Id = dataSourceEtl.DataSourceId;
            Name = dataSourceEtl.DataSourceName;
            TimeZoneId = dataSourceEtl.TimeZoneId.ToString(CultureInfo.InvariantCulture);
            TimeZoneCode = dataSourceEtl.TimeZoneCode;
            IntervalLengthText = dataSourceEtl.IntervalLength.ToString(CultureInfo.InvariantCulture);
            Inactive = dataSourceEtl.Inactive;
            RowIndex = rowIndex;
            setRowEnabled();
        }

        public int Id { get; private set; }
        public bool Inactive { get; set; }
        public string IntervalLengthText { get; set; }
        public string TimeZoneCode { get; set; }
        public string TimeZoneId { get; set; }
        public string Name { get; set; }
        public bool IsRowEnabled { get; set; }
        public int RowIndex { get; set; }
        public DataSourceState RowState { get; set; }
        public string RowStateToolTip { get; set; }

        private void setRowEnabled()
        {
            if (TimeZoneId == "-1")
            {
                IsRowEnabled = true;
            }
        }
    }
}