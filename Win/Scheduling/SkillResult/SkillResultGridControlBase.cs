using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Common.Chart;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.SkillResult
{
	public class SkillResultGridControlBase : TeleoptiGridControl, IHelpContext
	{
		private ChartSettings _chartSettings;
		private readonly ChartSettings _defaultChartSettings = new ChartSettings();

		public void InitializeBase(string settingName)
		{
			setupChartDefault();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_chartSettings = new PersonalSettingDataRepository(uow).FindValueByKey(settingName, _defaultChartSettings);
			}
		}

		public bool HasHelp
		{
			get
			{
				return true;
			}
		}

		public string HelpId
		{
			get
			{
				return Name;
			}
		}

		public ChartSettings ChartSettings
		{
			get { return _chartSettings; }
		}

		public void SaveSetting()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonalSettingDataRepository(uow).PersistSettingValue(_chartSettings);
				uow.PersistAll();
			}
		}

		public IChartSeriesSetting ConfigureSetting(string key)
		{
			var ret = ChartSettings.DefinedSetting(key, new ChartSettingsManager().ChartSettingsDefault);
			ret.Enabled = ChartSettings.SelectedRows.Contains(key);
			return ret;
		}

		private void setupChartDefault()
		{
			_defaultChartSettings.SelectedRows.Add("ForecastedHours");
			_defaultChartSettings.SelectedRows.Add("ScheduledHours");
			_defaultChartSettings.SelectedRows.Add("RelativeDifference");
		}
	}
}