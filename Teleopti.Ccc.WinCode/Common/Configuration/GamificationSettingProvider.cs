using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class GamificationSettingProvider : IGamificationSettingProvider
    {
        private readonly IGamificationSettingRepository _gamificationSettingRepository;
		private IList<IGamificationSetting> _gamificationSettingCollection;
		public static readonly IGamificationSetting NullGamificationSetting = new GamificationSetting("NullGameSetting");

		public GamificationSettingProvider(IGamificationSettingRepository gamificationSettingRepository)
        {
	        _gamificationSettingRepository = gamificationSettingRepository;
        }

		public IEnumerable<IGamificationSetting> GetGamificationSettingsEmptyNotIncluded()
        {
			EnsureInitialized(IsEmptySettingIncluded:false);
			return _gamificationSettingCollection;
        }

		public IEnumerable<IGamificationSetting> GetGamificationSettingsEmptyIncluded()
        {
			EnsureInitialized(IsEmptySettingIncluded: true);
			return _gamificationSettingCollection;
        }

		private void EnsureInitialized(bool IsEmptySettingIncluded)
        {
			if (_gamificationSettingCollection == null)
            {
                var result = _gamificationSettingRepository.LoadAll();
				if (IsEmptySettingIncluded)
                    result.Add(NullGamificationSetting);
                _gamificationSettingCollection = result;
            }
        }
    }
}