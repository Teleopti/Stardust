using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public interface IIsTeamGamificationSettingsAvailable
	{
		bool Satisfy();
	}

	public class IsTeamGamificationSettingsAvailable : IIsTeamGamificationSettingsAvailable
	{
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepository;

		public IsTeamGamificationSettingsAvailable(ITeamGamificationSettingRepository teamGamificationSettingRepository)
		{
			_teamGamificationSettingRepository = teamGamificationSettingRepository;
		}

		public bool Satisfy()
		{
			var settings = _teamGamificationSettingRepository.FindAllTeamGamificationSettingsSortedByTeam();
			if (settings == null || !settings.Any())
			{
				return false;
			}
			return true;
		}
	}
}