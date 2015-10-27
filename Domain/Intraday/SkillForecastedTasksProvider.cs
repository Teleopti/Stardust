using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillForecastedTasksProvider : ISkillForecastedTasksProvider
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;

		public SkillForecastedTasksProvider(ISkillRepository skillRepository, ISkillDayRepository skillDayRepository, IScenarioRepository scenarioRepository)
		{
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			//can this be different?
		}

		public IList<SkillTaskDetails> GetForecastedTasks()
		{
			var allSkills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			var skillToTaskData =  _skillDayRepository.GetSkillsTasksDetails(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), allSkills.ToList(),
				_scenarioRepository.LoadDefaultScenario()).ToList();
			var result = new List<SkillTaskDetails>();
			foreach (var item in skillToTaskData)
			{
				if (!result.Contains(new SkillTaskDetails(){SkillId = item.SkillId}))
				{
					result.Add(new SkillTaskDetails()
					{
						SkillId = item.SkillId,
						SkillName = item.Name,
						IntervalTasks =
							skillToTaskData.Where(x => x.SkillId == item.SkillId)
								.Select(x => new IntervalTasks()
								{
									//should use timezone helper
									Interval =
										new DateTimePeriod( new DateTime(x.Minimum.Year,x.Minimum.Month,x.Minimum.Day,x.Minimum.Hour,x.Minimum.Minute,x.Minimum.Second,DateTimeKind.Utc),
											new DateTime(x.Maximum.Year, x.Maximum.Month, x.Maximum.Day, x.Maximum.Hour, x.Maximum.Minute, x.Maximum.Second, DateTimeKind.Utc)),
									Task = x.TotalTasks
								}).ToList()
					});
				}
			}
			
			return result;
		}
	}

	public interface ISkillForecastedTasksProvider
	{
		IList<SkillTaskDetails> GetForecastedTasks();
	}

	public class SkillTaskDetails : IEquatable<SkillTaskDetails>
	{
		public Guid SkillId { get; set; }
		public string SkillName { get; set; }
		public List<IntervalTasks> IntervalTasks { get; set; }

		public bool Equals(SkillTaskDetails other)
		{
			return SkillId==other.SkillId;
		}

	}

	public class IntervalTasks
	{
		public DateTimePeriod Interval { get; set; }
		public double Task { get; set; }
	}
}