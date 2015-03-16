using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastingAccuracy
	{
		public Guid WorkloadId { get; set; }
		public double Accuracy { get; set; }
		public bool CanForecast { get; set; }
	}

	public class QuickForecastCreator : IQuickForecastCreator
	{
		private readonly IQuickForecaster _quickForecaster;
		private readonly ISkillRepository _skillRepository;
		private readonly INow _now;

		public QuickForecastCreator(IQuickForecaster quickForecaster, ISkillRepository skillRepository, INow now)
		{
			_quickForecaster = quickForecaster;
			_skillRepository = skillRepository;
			_now = now;
		}

		public ForecastingAccuracy CreateForecastForAllSkills(DateOnlyPeriod futurePeriod)
		{
			var historicalPeriod = getHistoricalPeriod();
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			return new ForecastingAccuracy
			{
				Accuracy = Math.Round(skills.Average(skill => _quickForecaster.ForecastForSkill(skill, futurePeriod, historicalPeriod)), 1)
			};
		}

		public ForecastingAccuracy[] CreateForecastForWorkloads(DateOnlyPeriod futurePeriod, Guid[] workloadIds)
		{
			var historicalPeriod = getHistoricalPeriod();
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			var workloads = workloadIds.Select(workloadId => (
				from skill in skills 
				from workload in skill.WorkloadCollection 
				where workloadId == workload.Id 
				select workload).Single());
			return workloads.Select(workload => new ForecastingAccuracy
			{
				Accuracy = Math.Round(_quickForecaster.ForecastForWorkload(workload, futurePeriod, historicalPeriod), 1),
				WorkloadId = workload.Id.Value
			}).ToArray();
		}

		private DateOnlyPeriod getHistoricalPeriod()
		{
			var nowDate = _now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-2)), nowDate);
			return historicalPeriod;
		}
	}
}