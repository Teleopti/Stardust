using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public class RecalculateForecastOnSkillEventHandler : IHandleEvent<RecalculateForecastOnSkillCollectionEvent>, IRunOnServiceBus
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IStatisticLoader _statisticLoader;
		private readonly IReforecastPercentCalculator _reforecastPercentCalculator;
		
		public RecalculateForecastOnSkillEventHandler(IScenarioRepository scenarioRepository, ISkillDayRepository skillDayRepository,
			ISkillRepository skillRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IStatisticLoader statisticLoader, IReforecastPercentCalculator reforecastPercentCalculator)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayRepository = skillDayRepository;
			_skillRepository = skillRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_statisticLoader = statisticLoader;
			_reforecastPercentCalculator = reforecastPercentCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(RecalculateForecastOnSkillCollectionEvent message)
		{
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var scenario = _scenarioRepository.Get(message.ScenarioId);
				if (!scenario.DefaultScenario) return;

				foreach (var skillMessage in message.SkillCollection)
				{
					var dateOnly = DateOnly.Today;
					var skill = _skillRepository.Get(skillMessage.SkillId);
					if (skill == null)
						continue;
					var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
					var period = new DateOnlyPeriod(dateOnly, dateOnly).ToDateTimePeriod(skill.TimeZone);
					period = period.ChangeEndTime(skill.MidnightBreakOffset.Add(TimeSpan.FromHours(1)));

					var skillDays = new List<ISkillDay>(_skillDayRepository.FindRange(dateOnlyPeriod, skill, scenario));
					foreach (var skillDay in skillDays)
					{
						foreach (var workloadDay in skillDay.WorkloadDayCollection)
						{
							if (!skillMessage.WorkloadIds.Contains(workloadDay.Workload.Id.GetValueOrDefault()))
								continue;
							var lastInterval = _statisticLoader.Execute(period, workloadDay, skillDay.SkillStaffPeriodCollection);
							var perc = _reforecastPercentCalculator.Calculate(workloadDay, lastInterval);
							workloadDay.CampaignTasks = new Percent(0);
							workloadDay.Tasks = workloadDay.Tasks * perc;
						}
					}
				}
				unitOfWork.PersistAll();
			}
		}
	}
}