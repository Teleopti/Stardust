using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public class TeamBlockSwap
	{
		private readonly ITeamBlockSwapDayValidator _teamBlockSwapDayValidator;
		private readonly ISwapServiceNew _swapServiceNew;
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly ITeamBlockSwapValidator _teamBlockSwapValidator;

		public TeamBlockSwap(ISwapServiceNew swapServiceNew, IScheduleDictionary scheduleDictionary, ITeamBlockSwapValidator teamBlockSwapValidator, ITeamBlockSwapDayValidator teamBlockSwapDayValidator)
		{
			_teamBlockSwapDayValidator = teamBlockSwapDayValidator;
			_swapServiceNew = swapServiceNew;
			_scheduleDictionary = scheduleDictionary;
			_teamBlockSwapValidator = teamBlockSwapValidator;
		}

		public bool Swap(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2)
		{
			if (!_teamBlockSwapValidator.ValidateCanSwap(teamBlockInfo1, teamBlockInfo2)) return false;
			var scheduleDayPros1 = extractScheduleDayPros(teamBlockInfo1.MatrixesForGroupAndBlock()).ToList();
			var scheduleDayPros2 = extractScheduleDayPros(teamBlockInfo2.MatrixesForGroupAndBlock()).ToList();
			var index = scheduleDayPros1.Count;
			var daysToSwap = new List<IScheduleDay>();
			var swappedDays = new List<IScheduleDay>();

			for (var i = 0; i < index; i++)
			{	
				daysToSwap.Clear();
				
				var day1 = scheduleDayPros1[i].DaySchedulePart();
				var day2 = scheduleDayPros2[i].DaySchedulePart();

				if (!_teamBlockSwapDayValidator.ValidateSwapDays(day1, day2)) return false;

				daysToSwap.Add(day1);
				daysToSwap.Add(day2);

				var result = _swapServiceNew.Swap(daysToSwap, _scheduleDictionary);
				swappedDays.AddRange(result);
			}

			return true;
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
	}
}
