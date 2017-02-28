namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ITeamGamificationSetting : IAggregateRoot, IBelongsToBusinessUnit, ICloneableEntity<ITeamGamificationSetting>
	{
		ITeam Team { get; set; }
		IGamificationSetting GamificationSetting { get; set; }
	}
}
