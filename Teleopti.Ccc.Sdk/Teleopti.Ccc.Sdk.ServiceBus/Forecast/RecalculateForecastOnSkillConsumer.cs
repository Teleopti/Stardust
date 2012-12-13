using System;
using System.Collections.Generic;
using Rhino.ServiceBus;
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
		//private readonly static ILog Logger = LogManager.GetLogger(typeof(RecalculateForecastOnSkillConsumer));

		public RecalculateForecastOnSkillConsumer(IScenarioRepository scenarioRepository, ISkillDayRepository skillDayRepository, ISkillRepository skillRepository, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayRepository = skillDayRepository;
			_skillRepository = skillRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(RecalculateForecastOnSkillMessage message)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var scenario = _scenarioRepository.Get(message.ScenarioId);
				if (!scenario.DefaultScenario) return;

				var skill = _skillRepository.Get(message.SkillId);
				var period = new DateOnlyPeriod(new DateOnly(DateTime.Today), new DateOnly(DateTime.Today));

				var skillDays = new List<ISkillDay>(_skillDayRepository.FindRange(period, skill, scenario));
				foreach (var skillDay in skillDays)
				{
					foreach (var workloadDay in skillDay.WorkloadDayCollection)
					{
						workloadDay.Tasks = workloadDay.Tasks * 1.1;
					}
				}
				unitOfWork.PersistAll();
			}
		}
	}
}