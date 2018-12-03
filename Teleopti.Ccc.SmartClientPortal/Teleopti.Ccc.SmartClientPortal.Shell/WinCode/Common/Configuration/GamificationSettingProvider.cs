using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
    public class GamificationSettingProvider : IGamificationSettingProvider
    {
        private readonly IGamificationSettingRepository _gamificationSettingRepository;
		private IList<IGamificationSetting> _gamificationSettingCollection;
		public static readonly IGamificationSetting NullGamificationSetting = new GamificationSetting("No gamification setting");

		public GamificationSettingProvider(IGamificationSettingRepository gamificationSettingRepository)
        {
	        _gamificationSettingRepository = gamificationSettingRepository;
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
                var result = _gamificationSettingRepository.LoadAll().ToList();
				if (IsEmptySettingIncluded)
                    result.Add(NullGamificationSetting);
                _gamificationSettingCollection = result;
            }
        }
    }
}