using System;
using System.Collections.Generic;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public class RecalculateForecastOnSkillConsumer:  ConsumerOf<RecalculateForecastOnSkillMessage>
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IStatisticLoader _statisticLoader;
		private readonly IReforecastPercentCalculator _reforecastPercentCalculator;
		//private readonly static ILog Logger = LogManager.GetLogger(typeof(RecalculateForecastOnSkillConsumer));

		public RecalculateForecastOnSkillConsumer(IScenarioRepository scenarioRepository, ISkillDayRepository skillDayRepository, 
			ISkillRepository skillRepository, IUnitOfWorkFactory unitOfWorkFactory, IStatisticLoader statisticLoader, IReforecastPercentCalculator reforecastPercentCalculator)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayRepository = skillDayRepository;
			_skillRepository = skillRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_statisticLoader = statisticLoader;
			_reforecastPercentCalculator = reforecastPercentCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(RecalculateForecastOnSkillMessage message)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var scenario = _scenarioRepository.Get(message.ScenarioId);
				if (!scenario.DefaultScenario) return;
				var dateOnly = DateOnly.Today;
				var skill = _skillRepository.Get(message.SkillId);
				var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
				var period = new DateOnlyPeriod(dateOnly, dateOnly).ToDateTimePeriod(skill.TimeZone);
				period = period.ChangeEndTime(skill.MidnightBreakOffset.Add(TimeSpan.FromHours(1)));

				var skillDays = new List<ISkillDay>(_skillDayRepository.FindRange(dateOnlyPeriod, skill, scenario));
				foreach (var skillDay in skillDays)
				{
					foreach (var workloadDay in skillDay.WorkloadDayCollection)
					{
						var lastInterval =_statisticLoader.Execute(period, workloadDay, skillDay.SkillStaffPeriodCollection);
						var perc = _reforecastPercentCalculator.Calculate(workloadDay, lastInterval);
						workloadDay.Tasks = workloadDay.Tasks * perc;
					}
				}
				unitOfWork.PersistAll();
			}
		}
	}
}