

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ITeamBlockDayOffOptimizerService
	{
	}

	public class TeamBlockDayOffOptimizerService : ITeamBlockDayOffOptimizerService
	{
		// find a random selected TeamInfo
 		// find days off to move within the common matrix period
		// execute do moves
		// ev back to legal state?
		// clear involved teamblocks
		// reschedule involved teamblocks
		// rollback id failed or not good
		// remember not to break anything in shifts or restrictions
	}
}