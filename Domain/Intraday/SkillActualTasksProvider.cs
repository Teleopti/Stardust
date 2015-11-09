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
		private ISkillRepository _skillRepository;

		public SkillActualTasksProvider(IStatisticRepository statisticRepository, ISkillRepository skillRepository)
		{
			_statisticRepository = statisticRepository;
			_skillRepository = skillRepository;
		}

		//check if the date and time is stored in the skill timezone 
		//and convert it in the query

		public IList<SkillTaskDetails> GetActualTasks()
		{
			var result = new  List<SkillTaskDetails>();
			var intradayStatistics = _statisticRepository.LoadSkillStatisticForSpecificDates(DateOnly.Today);
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			foreach (var item in intradayStatistics)
			{
				var concernedSkill = skills.Where(x => x.Id.Value == item.SkillId);
				if (concernedSkill.Any())
				{
					if (!result.Contains(new SkillTaskDetails() { SkillId = item.SkillId }))
					{
						result.Add(new SkillTaskDetails()
						{
							SkillId = item.SkillId,
							SkillName = item.SkillName,
							IntervalTasks =
								intradayStatistics.Where(x => x.SkillId == item.SkillId)
									.Select(x => new IntervalTasks() { IntervalStart = TimeZoneHelper.ConvertToUtc(x.Interval,concernedSkill.First().TimeZone), Task = x.StatOfferedTasks }).ToList()
						});
					}
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
