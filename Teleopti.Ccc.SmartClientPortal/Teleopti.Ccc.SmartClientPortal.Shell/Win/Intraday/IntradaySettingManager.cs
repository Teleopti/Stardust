using System.Collections;
using System.IO;
using System.Text;
using Syncfusion.Runtime.Serialization;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.Win.Intraday;
using Teleopti.Ccc.WinCode.Common.Chart;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday
{
    public class IntradaySettingManager
    {
        private IntradaySettingsPresenter _intradaySettingsPresenter;
        private DockingManager _dockingManager;
        private bool _hasUnsavedLayout;
        private string _defaultSetting;
        private const string _DEFAULT_VIEW = "xxDefaultIntradaySetting";

        public void Initialize(DockingManager dockingManager)
        {
            _dockingManager = dockingManager;
            _dockingManager.ShowCaption = true;
            _defaultSetting = streamDockingState();
            IntradaySettingsPresenter defaultIntradaySettingsPresenter = createDefaultSettingsPresenter();

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _intradaySettingsPresenter = PersonalSettingDataRepository.DONT_USE_CTOR(uow).FindValueByKey("IntradaySettings", defaultIntradaySettingsPresenter);
            }
        }
        public IEnumerable IntradaySettings
        {
            get { return _intradaySettingsPresenter.IntradaySettings; }
        }

        public IntradaySetting CurrentIntradaySetting
        {
            get { return _intradaySettingsPresenter.CurrentIntradaySetting; }
        }

        public bool HasUnsavedLayout
        {
            get { return _hasUnsavedLayout; }
            set { _hasUnsavedLayout = value; }
        }

        public ChartSettings ChartSetting
        {
            get { return _intradaySettingsPresenter.CurrentIntradaySetting.ChartSetting; }
        }

        public void Remove(string view)
        {
            var intradaySetting = _intradaySettingsPresenter.GetIntradaySetting(view);
            _intradaySettingsPresenter.RemoveIntradaySetting(intradaySetting);
        }

        private void PrepareNewIntradaySetting(IntradaySetting setting)
        {
            setting.ChartSetting.SelectedRows.Add("FStaff");
            setting.ChartSetting.SelectedRows.Add("CalculatedResource");
            setting.ChartSetting.SelectedRows.Add("CalculatedLoggedOn");
            setting.ChartSetting.DefinedSetting("FStaff",new ChartSettingsManager().ChartSettingsDefault);
            setting.ChartSetting.DefinedSetting("CalculatedResource", new ChartSettingsManager().ChartSettingsDefault);
            setting.ChartSetting.DefinedSetting("CalculatedLoggedOn", new ChartSettingsManager().ChartSettingsDefault);
        }

        private string streamDockingState()
        {
            using (var stream = new MemoryStream())
            {
                var apop = new AppStateSerializer(SerializeMode.XMLFmtStream, stream);
                _dockingManager.SaveDockState(apop);
                _hasUnsavedLayout = false;
                apop.PersistNow();
                var encoding = new ASCIIEncoding();
                return encoding.GetString(stream.GetBuffer());
            }
        }

        public void ResetDockingState()
        {
            _dockingManager.LockDockPanelsUpdate();
            _dockingManager.LoadDesignerDockState();
            _dockingManager.UnlockDockPanelsUpdate();
        }

        public void ResetDefaultSetting()
        {
            var setting = _intradaySettingsPresenter.GetIntradaySetting(_DEFAULT_VIEW);
            LoadDockingState(setting.Name);
            setting.DockingState = streamDockingState();
            setting.ChartSetting.SelectedRows.Clear();
            PrepareNewIntradaySetting(setting);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void LoadDockingState(string value)
        {
            var intradaySetting = _intradaySettingsPresenter.GetIntradaySetting(value);

            _dockingManager.LockDockPanelsUpdate();
            try
            {
                loadNewDockingState(intradaySetting.DockingState);
            }
            catch
            {
                _dockingManager.LoadDesignerDockState();
            }
            finally
            {
                _dockingManager.UnlockDockPanelsUpdate();
            }
        }

        private void loadNewDockingState(string value)
        {
            //_flag = false;
            string setting = value ?? _defaultSetting;

            var encoding = new ASCIIEncoding();
            using (var stream = new MemoryStream(encoding.GetBytes(setting)))
            {
                _dockingManager.LoadDockState(new AppStateSerializer(SerializeMode.XMLFmtStream, stream));
            }
        }

        public void Persist(string view)
        {
            var intradaySetting = _intradaySettingsPresenter.GetIntradaySetting(view);
            if (intradaySetting != null)
            {
                intradaySetting.DockingState = streamDockingState();
                ResetDefaultSetting();
            }

            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                PersonalSettingDataRepository.DONT_USE_CTOR(uow).PersistSettingValue(_intradaySettingsPresenter);
                uow.PersistAll();
            }
        }

        public IntradaySetting DefaultSetting()
        {
            return _intradaySettingsPresenter.GetIntradaySetting(_DEFAULT_VIEW);
        }

        public void UpdatePreviousDockingState()
        {
            if (!_hasUnsavedLayout) return;
          
            var previousIntradaySetting = _intradaySettingsPresenter.GetIntradaySetting(_intradaySettingsPresenter.CurrentIntradaySetting.Name);
            previousIntradaySetting.DockingState = streamDockingState();
        }

        private IntradaySettingsPresenter createDefaultSettingsPresenter()
        {
            var ret = new IntradaySettingsPresenter();
            var setting = createDefaultSetting();
            ret.IntradaySettings.Add(setting);
            ret.SetIntradaySetting(_DEFAULT_VIEW);
            return ret;
        }

        private IntradaySetting createDefaultSetting()
        {
            var setting = new IntradaySetting(_DEFAULT_VIEW);
            PrepareNewIntradaySetting(setting);
            return setting;
        }

        public void SetChartView(string chartView)
        {
            _intradaySettingsPresenter.SetIntradaySetting(chartView);
        }

        public void LoadCurrentDockingState()
        {
            LoadDockingState(CurrentIntradaySetting.Name);
        }
    }
}