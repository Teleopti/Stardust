using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface IShiftCategoryPointInfoExtractor
	{
		IDictionary<ITeamBlockInfo, IShiftCategoryPointInfo> ExtractShiftCategoryInfos(IList<ITeamBlockInfo> teamBlockInfos);
	}


	public class ShiftCategoryPointInfoExtractor : IShiftCategoryPointInfoExtractor
	{
		private readonly IShiftCategoryPointExtractor _shiftCategoryPointExtractor;

		public ShiftCategoryPointInfoExtractor(IShiftCategoryPointExtractor shiftCategoryPointExtractor)
		{
			_shiftCategoryPointExtractor = shiftCategoryPointExtractor;
		}

		public IDictionary<ITeamBlockInfo, IShiftCategoryPointInfo> ExtractShiftCategoryInfos(IList<ITeamBlockInfo> teamBlockInfos)
		{
			var shiftCategoryPoints = _shiftCategoryPointExtractor.ExtractShiftCategoryPoints();
			var result = new Dictionary<ITeamBlockInfo, IShiftCategoryPointInfo>();

			foreach (var teamBlockInfo in teamBlockInfos)
			{
				var points = 0;
				var period = teamBlockInfo.BlockInfo.BlockPeriod;
				var scheduleMatrixes = teamBlockInfo.TeamInfo.MatrixesForGroupAndPeriod(period).ToList();

				
				foreach (var dateOnly in period.DayCollection())
				{
					foreach (var scheduleMatrixPro in scheduleMatrixes)
					{
						var scheduleDayPro = scheduleMatrixPro.GetScheduleDayByKey(dateOnly);
						if (scheduleDayPro == null) continue;
						
						var scheduleDay = scheduleDayPro.DaySchedulePart();
						if (scheduleDay == null) continue;
						
						var personAssignment = scheduleDay.PersonAssignment();
						if (personAssignment == null) continue;

						var shiftCategory = personAssignment.ShiftCategory;
						if (shiftCategory == null) continue;
						
						points += shiftCategoryPoints[shiftCategory];			
					}	
				}

				var shiftCategoryPointInfo = new ShiftCategoryPointInfo(teamBlockInfo, points);
				result.Add(teamBlockInfo, shiftCategoryPointInfo);
			}

			return result;
		}
	}
}
