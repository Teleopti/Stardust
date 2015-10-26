using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillActualTasksProvider : ISkillActualTasksProvider
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IStatisticRepository _statisticRepository;

		public SkillActualTasksProvider(ISkillRepository skillRepository, IStatisticRepository statisticRepository)
		{
			_skillRepository = skillRepository;
			_statisticRepository = statisticRepository;
		}

		public Dictionary<ISkill,IList<SkillTaskDetails>> GetActualTasks()
		{
			var result = new Dictionary<ISkill, IList<SkillTaskDetails>>();
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			var queueSourceCollection = new List<IQueueSource>();
			foreach (var skill in skills)
			{
				foreach (var workload in skill.WorkloadCollection)
				{
					queueSourceCollection.AddRange(workload.QueueSourceCollection);
					//need to refactor this
					result.Add(skill, new List<SkillTaskDetails>());
					var statisticTasks = _statisticRepository.LoadSpecificDates(queueSourceCollection, new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow));
					//foreach (var statisticTask in statisticTasks)
					//{
					//	//check the resolution
					//	result[skill].Add(new SkillTaskDetails()
					//	{
					//		Interval = new DateTimePeriod(statisticTask.Interval, statisticTask.Interval.AddMinutes(skill.DefaultResolution))
					//	});
					//}
				}
				
			}
			return result;
		}
		
		//public Dictionary<ISkill,IList<SkillTaskDetails>> GetActualTasks()
		//{
		//	var result = new Dictionary<ISkill, IList<SkillTaskDetails>>();
		//	var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
		//	var queueSourceCollection = new List<IQueueSource>();
		//	const string timeZoneId = "dummy timezone as this will be refactored and removed";
		//	var intradayStatistic = _statisticRepository.LoadSkillStatisticForSpecificDates(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow), timeZoneId, TimeSpan.Zero);
		//	foreach (var item in intradayStatistic)
		//	{
		//		if(result.ContainsKey(item.))
		//	}
		//	return result;
		//}
	}

	public interface ISkillActualTasksProvider
	{
		Dictionary<ISkill,IList<SkillTaskDetails>> GetActualTasks();
	}
}