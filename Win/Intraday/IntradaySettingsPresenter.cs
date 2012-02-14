using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Intraday
{
    [Serializable]
    public class IntradaySettingsPresenter : SettingValue
    {

        private IList<IntradaySetting> _intradaySettings = new List<IntradaySetting>();
        private IntradaySetting _intradaySetting;

        public IList<IntradaySetting> IntradaySettings
        {
            get { return _intradaySettings; }
        }

        public IntradaySetting CurrentIntradaySetting
        {
            get { return _intradaySetting; }
        }

        public void Load()
        {
            _intradaySettings = new List<IntradaySetting>();
        }

        public void RemoveIntradaySetting(IntradaySetting intradaySetting)
        {
            _intradaySettings.Remove(intradaySetting);
        }

        public void SetIntradaySetting(string intradaySettingName)
        {
            _intradaySetting = GetIntradaySetting(intradaySettingName);
        }

        public IntradaySetting GetIntradaySetting(string intradaySettingName)
        {
            IntradaySetting intradaySetting = _intradaySettings.FirstOrDefault(s => s.Name == intradaySettingName);
            if (intradaySetting == null)
            {
                intradaySetting = new IntradaySetting(intradaySettingName);
                _intradaySettings.Add(intradaySetting);
            }

            return intradaySetting;
        }

        public void SaveIntradaySetting(IntradaySetting intradaySetting)
        {
            if(!_intradaySettings.Contains(intradaySetting))
            {
                _intradaySettings.Add(intradaySetting);
                if (((ISettingValue)this).BelongsTo == null)
                    return;

                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    new PersonalSettingDataRepository(uow).PersistSettingValue(this);
                    uow.PersistAll();
                }
            }
        }
    }
}