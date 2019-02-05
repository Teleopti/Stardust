namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ITeamGamificationSetting : IAggregateRoot, IFilterOnBusinessUnit, ICloneableEntity<ITeamGamificationSetting>
	{
		ITeam Team { get; set; }
		IGamificationSetting GamificationSetting { get; set; }
	}
}
