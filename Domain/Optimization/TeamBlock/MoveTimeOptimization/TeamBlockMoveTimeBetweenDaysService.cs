using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeBetweenDaysService
	{
		void Execute();
		event EventHandler<ResourceOptimizerProgressEventArgs> PerformMoveTime;
	}

	public class TeamBlockMoveTimeBetweenDaysService : ITeamBlockMoveTimeBetweenDaysService
	{
		public event EventHandler<ResourceOptimizerProgressEventArgs> PerformMoveTime;
		private bool _cancelMe;

		public void Execute()
		{
			var someCondition = true;
			while (someCondition)
			{
				var dateOnlyList = getCandidatesDatesToAnalyze();
				if (dateOnlyList.Count == 0) break;

				var candidatesTeamBlock = contractTeamBlockOnDates(dateOnlyList);

				deleteDaysAmongTeamBlockBasedOnOptions(candidatesTeamBlock);

				resceduleTeamBlock(candidatesTeamBlock);

				validateIfMoveTimeIsOk(candidatesTeamBlock);
			}
		}

		protected virtual void OnDayScheduled(ResourceOptimizerProgressEventArgs resourceOptimizerProgressEventArgs)
		{
			EventHandler<ResourceOptimizerProgressEventArgs> temp = PerformMoveTime;
			if (temp != null)
			{
				temp(this, resourceOptimizerProgressEventArgs);
			}
			_cancelMe = resourceOptimizerProgressEventArgs.Cancel;
		}

		private void validateIfMoveTimeIsOk(IList<TeamBlockInfo> candidatesTeamBlock)
		{
			throw new NotImplementedException();
		}

		private void resceduleTeamBlock(IList<TeamBlockInfo> candidatesTeamBlock)
		{
			throw new NotImplementedException();
		}

		private void deleteDaysAmongTeamBlockBasedOnOptions(IList<TeamBlockInfo> candidatesTeamBlock)
		{
			throw new NotImplementedException();
		}

		private IList<TeamBlockInfo> contractTeamBlockOnDates(IList<DateOnly> dateOnlyList)
		{
			throw new NotImplementedException();
		}

		private IList<DateOnly> getCandidatesDatesToAnalyze()
		{
			throw new NotImplementedException();
		}
	}
}
