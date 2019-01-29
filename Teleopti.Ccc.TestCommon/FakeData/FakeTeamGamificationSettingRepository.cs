using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeTeamGamificationSettingRepository : ITeamGamificationSettingRepository
	{
		private readonly IList<ITeamGamificationSetting> _teamGamificationSettings;
		public FakeTeamGamificationSettingRepository()
		{
			_teamGamificationSettings = new List<ITeamGamificationSetting>();
		}

		public void Add(ITeamGamificationSetting teamGamificationSettings)
		{
			teamGamificationSettings.SetId(Guid.NewGuid());
			_teamGamificationSettings.Add(teamGamificationSettings);
		}

		public void Remove(ITeamGamificationSetting teamGamificationSettings)
		{
			_teamGamificationSettings.Remove(_teamGamificationSettings.First(x => x.Id == teamGamificationSettings.Id));
		}

		public ITeamGamificationSetting Get(Guid id)
		{
			return _teamGamificationSettings.FirstOrDefault(x => x.Id == id);
		}

		public ITeamGamificationSetting Load(Guid id)
		{
			return Get(id);
		}

		public IEnumerable<ITeamGamificationSetting> LoadAll()
		{
			return _teamGamificationSettings;
		}

		public IEnumerable<ITeamGamificationSetting> FindAllTeamGamificationSettingsSortedByTeam()
		{
			return _teamGamificationSettings.OrderByDescending(x => x.Team.Description.Name);
		}

		public ITeamGamificationSetting FindTeamGamificationSettingsByTeam(ITeam team)
		{
			return _teamGamificationSettings.FirstOrDefault(x => x.Team.Id == team.Id);
		}

		public IEnumerable<ITeamGamificationSetting> FetchTeamGamificationSettings(Guid gamificationId)
		{
			return _teamGamificationSettings.Where(ts => gamificationId == ts.GamificationSetting.Id.GetValueOrDefault()).ToList();
		}
	}
}
