using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeGamificationSettingRepository : IGamificationSettingRepository
	{
		private readonly Dictionary<Guid, IGamificationSetting> _gamificationSettings;

		public FakeGamificationSettingRepository()
		{
			_gamificationSettings = new Dictionary<Guid, IGamificationSetting>();
		}

		public void Add(IGamificationSetting gamificationSetting)
		{
			if (!gamificationSetting.Id.HasValue)
			{
				gamificationSetting.SetId(Guid.NewGuid());
			}

			_gamificationSettings[gamificationSetting.Id.Value] = gamificationSetting;
		}

		public void Remove(IGamificationSetting gamificationSetting)
		{
			_gamificationSettings.Remove(gamificationSetting.Id.Value);
		}

		public IGamificationSetting Get(Guid id)
		{
			return _gamificationSettings.ContainsKey(id) ? _gamificationSettings[id] : null;
		}

		public IGamificationSetting Load(Guid id)
		{
			return Get(id);
		}

		public IList<IGamificationSetting> LoadAll()
		{
			return _gamificationSettings.Values.ToList();
		}

		public IEnumerable<IGamificationSetting> FindAllGamificationSettingsSortedByDescription()
		{
			throw new NotImplementedException();
		}
	}
}
