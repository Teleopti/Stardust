using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeGamificationSettingRepository : IGamificationSettingRepository
	{
		private readonly IList<IGamificationSetting> _gamificationSettings;

		public FakeGamificationSettingRepository()
		{
			_gamificationSettings = new List<IGamificationSetting>();
		}

		public void Add(IGamificationSetting gamificationSetting)
		{
			gamificationSetting.SetId(Guid.NewGuid());
			_gamificationSettings.Add(gamificationSetting);
		}

		public void Remove(IGamificationSetting gamificationSetting)
		{
			_gamificationSettings.Remove(_gamificationSettings.First(x => x.Id == gamificationSetting.Id));
		}

		public IGamificationSetting Get(Guid id)
		{
			return _gamificationSettings.First(x => x.Id == id);
		}

		public IGamificationSetting Load(Guid id)
		{
			return Get(id);
		}

		public IList<IGamificationSetting> LoadAll()
		{
			return _gamificationSettings;
		}

		public IEnumerable<IGamificationSetting> FindAllGamificationSettingsSortedByDescription()
		{
			throw new NotImplementedException();
		}
	}
}
