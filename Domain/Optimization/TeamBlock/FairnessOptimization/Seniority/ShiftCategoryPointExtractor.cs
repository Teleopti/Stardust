using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IShiftCategoryPointExtractor
	{
        IDictionary<ITeamBlockInfo, ITeamBlockPoints> ExtractShiftCategoryInfos(IList<ITeamBlockInfo> teamBlockInfos, IList<IShiftCategory> shiftCategories);
	}


	public class ShiftCategoryPointExtractor : IShiftCategoryPointExtractor
	{
		private readonly IShiftCategoryPoints _shiftCategoryPointExtractor;

        public ShiftCategoryPointExtractor(IShiftCategoryPoints shiftCategoryPoints)
		{
			_shiftCategoryPointExtractor = shiftCategoryPoints;
		}

        public IDictionary<ITeamBlockInfo, ITeamBlockPoints> ExtractShiftCategoryInfos(IList<ITeamBlockInfo> teamBlockInfos, IList<IShiftCategory> shiftCategories)
		{
			var shiftCategoryPoints = _shiftCategoryPointExtractor.ExtractShiftCategoryPoints(shiftCategories);
			var result = new Dictionary<ITeamBlockInfo, ITeamBlockPoints>();

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

				var shiftCategoryPointInfo = new TeamBlockPoints(teamBlockInfo, points);
				result.Add(teamBlockInfo, shiftCategoryPointInfo);
			}

			return result;
		}
	}
}
