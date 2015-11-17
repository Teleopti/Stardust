using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillActualTasksProvider : ISkillActualTasksProvider
	{
		private readonly IStatisticRepository _statisticRepository;
		private readonly ISkillRepository _skillRepository;

		public SkillActualTasksProvider(IStatisticRepository statisticRepository, ISkillRepository skillRepository)
		{
			_statisticRepository = statisticRepository;
			_skillRepository = skillRepository;
		}


		public IList<SkillTaskDetails> GetActualTasks(Dictionary<Guid, TimeZoneInfo> skillsTimezone)
		{
			var result = new  List<SkillTaskDetails>();
			var intradayStatistics = _statisticRepository.LoadSkillStatisticForSpecificDates(DateOnly.Today);
			foreach (var item in intradayStatistics)
			{

				if (!result.Contains(new SkillTaskDetails() {SkillId = item.SkillId}))
					{
						result.Add(new SkillTaskDetails()
						{
							SkillId = item.SkillId,
							SkillName = item.SkillName,
							IntervalTasks =
								intradayStatistics.Where(x => x.SkillId == item.SkillId)
									.Select(
										x =>
											new IntervalTasks()
											{
												IntervalStart = TimeZoneHelper.ConvertToUtc(x.Interval, skillsTimezone[item.SkillId]),
												Task = x.StatOfferedTasks
											}).ToList()
						});
					}
			}
			return result;
		}
	}

	public interface ISkillActualTasksProvider
	{
		IList<SkillTaskDetails> GetActualTasks(Dictionary<Guid, TimeZoneInfo> skillsTimezone);
	}
}
