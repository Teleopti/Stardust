using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamSteadyState
	{
		bool SteadyState(IVirtualSchedulePeriod virtualSchedulePeriod, IScheduleMatrixPro scheduleMatrixPro, IPersonPeriod personPeriod);
	}

	public class TeamSteadyState : ITeamSteadyState
	{
		private readonly ITeamSteadyStatePersonPeriod _teamSteadyStatePersonPeriod;
		private readonly ITeamSteadyStateSchedulePeriod _teamSteadyStateSchedulePeriod;

		public TeamSteadyState(ITeamSteadyStatePersonPeriod teamSteadyStatePersonPeriod	, ITeamSteadyStateSchedulePeriod teamSteadyStateSchedulePeriod)
		{
			_teamSteadyStatePersonPeriod = teamSteadyStatePersonPeriod;
			_teamSteadyStateSchedulePeriod = teamSteadyStateSchedulePeriod;
		}

		public bool SteadyState(IVirtualSchedulePeriod virtualSchedulePeriod, IScheduleMatrixPro scheduleMatrixPro, IPersonPeriod personPeriod)	
		{
			if (!_teamSteadyStateSchedulePeriod.SchedulePeriodEquals(virtualSchedulePeriod, scheduleMatrixPro)) 
				return false;
			if (!_teamSteadyStatePersonPeriod.PersonPeriodEquals(personPeriod)) 
				return false;

			return true;
		}
	}
}
