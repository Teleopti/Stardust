using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Code.Gui.DataSourceConfiguration
{
    public interface IDataSourceConfigurationView
    {
        void Initialize();
        string ConnectionString { get; }
        bool ShowForm { get;}
        bool CloseApplication { get; set; }
        string DataSourceStatusColumnHeader { get; set; }
        string DataSourceNameColumnHeader { get; set; }
        string DataSourceTimeZoneColumnHeader { get; set; }
        string DataSourceIntervalLengthColumnHeader { get; set; }
        string DataSourceInactiveColumnHeader { get; set; }
        IList<DataSourceRow> DataSource { get; }
        IList<ITimeZoneDim> TimeZoneDataSource { get; }
        void SetDataSource(IList<DataSourceRow> dataSource);
        void SetTimeZoneDataSource(IList<ITimeZoneDim> dataSource);
        void SetOkButtonEnabled(bool isEnabled);
        void SetViewEnabled(bool isEnabled);
        void SetRowStateImage(DataSourceRow dataSourceRow);
        void SetRowReadOnly(DataSourceRow dataSourceRow);
        void EtlToolIsNowReady(IJob initialJob);
        bool IsEtlToolLoading { get; set; }
        void SetInitialJob(IJob job);
        void SetToolStripState(bool showSpinningProgress, string message);
        void RunInitialJob();
        void ShowErrorMessage(string message);
        void CloseView();
        bool IsSaved { get; set; }
        void SetTimeZoneSelected(DataSourceRow dataSourceRow);
        void SetTimeZoneComboState(int rowIndex, bool isEnabled);
        void DetachEvent();
    }
}