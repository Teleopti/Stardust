using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.Chart;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public class ForecasterChartSetting
    {
        private readonly TemplateTarget _templateTarget;
        private ChartSettings _chartSettings;
        private ChartSettings _defaultChartSettings;
        private const string SettingName = "{0}TaskOwner{1}GridGridAndChart";

        internal ForecasterChartSetting(TemplateTarget templateTarget)
        {
            _templateTarget = templateTarget;
        }

        internal void Initialize()
        {
            _defaultChartSettings = new ChartSettings();
            _defaultChartSettings.SelectedRows.Add("TotalTasks");
            _defaultChartSettings.SelectedRows.Add("TotalAverageTaskTime");
            _defaultChartSettings.SelectedRows.Add("TotalAverageAfterTaskTime");
        }

        internal ChartSettings GetChartSettings()
        {
            if (_chartSettings!=null)
            {
                return _chartSettings;
            }

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _chartSettings =
                    new PersonalSettingDataRepository(uow).FindValueByKey(
                        string.Format(CultureInfo.InvariantCulture, SettingName, _templateTarget, WorkingInterval.Custom),
                        _defaultChartSettings);
            }

            return _chartSettings;
        }

        internal void SaveSettings(PersonalSettingDataRepository repository)
        {
            if (_chartSettings != null)
                repository.PersistSettingValue(_chartSettings);
        }
    }
}
