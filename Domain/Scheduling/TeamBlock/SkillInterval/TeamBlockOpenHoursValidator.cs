using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ITeamBlockOpenHoursValidator
	{
		bool Validate(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class TeamBlockOpenHoursValidator : ITeamBlockOpenHoursValidator
	{
		private readonly ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private readonly ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;

		public TeamBlockOpenHoursValidator(ICreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity, ISkillIntervalDataOpenHour skillIntervalDataOpenHour)
		{
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
			_skillIntervalDataOpenHour = skillIntervalDataOpenHour;
		}

		public bool Validate(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var dayIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, schedulingResultStateHolder);
			var firstDate = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().First();
			var firstDateActivities = dayIntervalDataPerDateAndActivity[firstDate].Keys.ToList();
			

			foreach (var activity in firstDateActivities)
			{
				var dictionary = dayIntervalDataPerDateAndActivity[firstDate];
				var firstDateOpenPeriod = _skillIntervalDataOpenHour.GetOpenHours(dictionary[activity], firstDate);

				foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
				{
					if(dateOnly.Equals(firstDate)) continue;

					dictionary = dayIntervalDataPerDateAndActivity[dateOnly];
					var openPeriod = _skillIntervalDataOpenHour.GetOpenHours(dictionary[activity], dateOnly);

					if (openPeriod != firstDateOpenPeriod) return false;
				}
			}

			return true;
		}
	}
}
