using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamBlockContractTimeValidator
	{
		bool ValidateContractTime(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2);
	}

	public class TeamBlockContractTimeValidator : ITeamBlockContractTimeValidator
	{
		public bool ValidateContractTime(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2)
		{
			var scheduleDayPros1 = extractScheduleDayPros(teamBlockInfo1.MatrixesForGroupAndBlock());
			var scheduleDayPros2 = extractScheduleDayPros(teamBlockInfo2.MatrixesForGroupAndBlock());

			var contractTime1 = calculateContractTime(scheduleDayPros1);
			var contractTime2 = calculateContractTime(scheduleDayPros2);

			return contractTime1.Equals(contractTime2);
		}

		private IEnumerable<IScheduleDayPro> extractScheduleDayPros(IEnumerable<IScheduleMatrixPro> scheduleMatrixPros)
		{
			var scheduleDayPros = new List<IScheduleDayPro>();

			foreach (var scheduleMatrixPro in scheduleMatrixPros)
			{
				scheduleDayPros.AddRange(scheduleMatrixPro.EffectivePeriodDays);
			}

			return scheduleDayPros;
		}

		private TimeSpan calculateContractTime(IEnumerable<IScheduleDayPro> scheduleDayPros)
		{
			var contractTime = TimeSpan.Zero;

			foreach (var scheduleDayPro in scheduleDayPros)
			{
				var scheduleDay = scheduleDayPro.DaySchedulePart();
				if (scheduleDay != null && scheduleDay.HasProjection())
				{
					contractTime += scheduleDay.ProjectionService().CreateProjection().ContractTime();
				}
			}

			return contractTime;
		}
	}
}
