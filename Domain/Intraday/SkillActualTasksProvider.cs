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

		public SkillActualTasksProvider(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public IList<SkillTaskDetails> GetActualTasks()
		{
			var result = new  List<SkillTaskDetails>();
			var intradayStatistics = _statisticRepository.LoadSkillStatisticForSpecificDates(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow));
			foreach (var item in intradayStatistics)
			{
				if (!result.Contains(new SkillTaskDetails() { SkillId = item.SkillId }))
				{
					result.Add(new SkillTaskDetails()
					{
						SkillId = item.SkillId,
						SkillName = item.SkillName,
						IntervalTasks =
							intradayStatistics.Where(x => x.SkillId == item.SkillId)
								.Select(x => new IntervalTasks() { Interval = new DateTimePeriod((x.Interval), x.Interval), Task = x.StatOfferedTasks }).ToList()
					});
				}
			}
			return result;
		}
	}

	public interface ISkillActualTasksProvider
	{
		IList<SkillTaskDetails> GetActualTasks();
	}
}
