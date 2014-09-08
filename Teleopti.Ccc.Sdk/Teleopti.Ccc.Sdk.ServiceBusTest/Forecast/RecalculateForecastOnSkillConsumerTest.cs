﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class RecalculateForecastOnSkillConsumerTest
	{
		private RecalculateForecastOnSkillConsumer _target;
		private IScenarioRepository _scenarioRepository;
		private ISkillDayRepository _skillDayRepository;
		private ISkillRepository _skillRepository;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private ICurrentUnitOfWorkFactory _currentunitOfWorkFactory;
		private IStatisticLoader _statisticLoader;
		private IReforecastPercentCalculator _reforecastPercentCalculator;
		private IUnitOfWork _uow;
		private IScenario _scenario;

		[SetUp]
		public void Setup()
		{
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_currentunitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_statisticLoader = MockRepository.GenerateMock<IStatisticLoader>();
			_reforecastPercentCalculator = MockRepository.GenerateMock<IReforecastPercentCalculator>();
			_target = new RecalculateForecastOnSkillConsumer(_scenarioRepository, _skillDayRepository, _skillRepository,
															 _currentunitOfWorkFactory, _statisticLoader, _reforecastPercentCalculator);
			_uow = MockRepository.GenerateMock<IUnitOfWork>();
		}

		[Test]
		public void ShouldOnlyDoDefaultScenario()
		{
			var scenarioId = Guid.NewGuid();
			_scenario = MockRepository.GenerateStrictMock<IScenario>();
			var message = new RecalculateForecastOnSkillMessageCollection {ScenarioId = scenarioId};
			_currentunitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_uow);
			_scenarioRepository.Stub(x => x.Get(scenarioId)).Return(_scenario);
			_scenario.Stub(x => x.DefaultScenario).Return(false);
			_target.Consume(message);
		}

		[Test]
		public void ShouldSkipIfWrongSkillId()
		{
			var scenarioId = Guid.NewGuid();
			_scenario = MockRepository.GenerateStrictMock<IScenario>();
			var skillMessage = new RecalculateForecastOnSkillMessage();
			var message = new RecalculateForecastOnSkillMessageCollection
			              	{
			              		ScenarioId = scenarioId,
			              		MessageCollection = new Collection<RecalculateForecastOnSkillMessage> {skillMessage}
			              	};
			_currentunitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_uow);
			_scenarioRepository.Stub(x => x.Get(scenarioId)).Return(_scenario);
			_scenario.Stub(x => x.DefaultScenario).Return(true);
			_skillRepository.Stub(x => x.Get(Guid.Empty)).Return(null);
			
			_target.Consume(message);
			
		}

		[Test]
		public void ShouldLoadSkillDaysOnSkill()
		{
			var scenarioId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var workloadId = Guid.NewGuid();
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
			_scenario = MockRepository.GenerateStrictMock<IScenario>();
			var skill = MockRepository.GenerateMock<ISkill>();
			var skillMessage = new RecalculateForecastOnSkillMessage { SkillId = skillId, WorkloadIds = new Collection<Guid> { workloadId } };
			var message = new RecalculateForecastOnSkillMessageCollection
			{
				ScenarioId = scenarioId,
				MessageCollection = new Collection<RecalculateForecastOnSkillMessage> { skillMessage }
			};
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var workloadDay = MockRepository.GenerateMock<IWorkloadDay>();
			var workload = MockRepository.GenerateMock<IWorkload>();
			_currentunitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_uow);
			_scenarioRepository.Stub(x => x.Get(scenarioId)).Return(_scenario);
			_scenario.Stub(x => x.DefaultScenario).Return(true);
			_skillRepository.Stub(x => x.Get(skillId)).Return(skill);
			skill.Stub(x => x.TimeZone).Return(timezone);
			_skillDayRepository.Stub(x => x.FindRange(new DateOnlyPeriod(), skill, _scenario)).IgnoreArguments().Return(
				new Collection<ISkillDay>{skillDay});
			skillDay.Stub(x => x.WorkloadDayCollection).Return(new ReadOnlyCollection<IWorkloadDay>(new List<IWorkloadDay> { workloadDay }));
			workloadDay.Stub(x => x.Workload).Return(workload);
			workload.Stub(x => x.Id).Return(Guid.NewGuid());
			_target.Consume(message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldRecalculateWorkloadDay()
		{
			var scenarioId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var workloadId = Guid.NewGuid();
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
			_scenario = MockRepository.GenerateStrictMock<IScenario>();
			var skill = MockRepository.GenerateMock<ISkill>();
			var skillMessage = new RecalculateForecastOnSkillMessage { SkillId = skillId, WorkloadIds = new Collection<Guid> { workloadId } };
			var message = new RecalculateForecastOnSkillMessageCollection
			{
				ScenarioId = scenarioId,
				MessageCollection = new Collection<RecalculateForecastOnSkillMessage> { skillMessage }
			};
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var workloadDay = MockRepository.GenerateStrictMock<IWorkloadDay>();
			var workload = MockRepository.GenerateMock<IWorkload>();
			_currentunitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_uow);
			_scenarioRepository.Stub(x => x.Get(scenarioId)).Return(_scenario);
			_scenario.Stub(x => x.DefaultScenario).Return(true);
			_skillRepository.Stub(x => x.Get(skillId)).Return(skill);
			skill.Stub(x => x.TimeZone).Return(timezone);
			_skillDayRepository.Stub(x => x.FindRange(new DateOnlyPeriod(), skill, _scenario)).IgnoreArguments().Return(
				new Collection<ISkillDay> { skillDay });
			skillDay.Stub(x => x.WorkloadDayCollection).Return(new ReadOnlyCollection<IWorkloadDay>(new List<IWorkloadDay> { workloadDay }));
			workloadDay.Stub(x => x.Workload).Return(workload);
			workload.Stub(x => x.Id).Return(workloadId);
			_statisticLoader.Stub(x => x.Execute(new DateTimePeriod(), workloadDay, null)).IgnoreArguments().Return(
				new DateTime(2012, 12, 18));
			_reforecastPercentCalculator.Stub(x => x.Calculate(workloadDay, new DateTime(2012, 12, 18))).Return(1.1);
			workloadDay.Stub(x => x.Tasks).Return(100);
			workloadDay.Stub(x => x.Tasks).SetPropertyWithArgument(100 * 1.1);
			workloadDay.Stub(x => x.CampaignTasks).SetPropertyWithArgument(new Percent(0));
			_target.Consume(message);
		}
	}


}