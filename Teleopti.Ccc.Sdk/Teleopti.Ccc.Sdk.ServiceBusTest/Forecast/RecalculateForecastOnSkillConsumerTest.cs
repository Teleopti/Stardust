using System;
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
		private MockRepository _mocks;
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
			_mocks = new MockRepository();
			_scenarioRepository = _mocks.DynamicMock<IScenarioRepository>();
			_skillDayRepository = _mocks.DynamicMock<ISkillDayRepository>();
			_skillRepository = _mocks.DynamicMock<ISkillRepository>();
			_currentunitOfWorkFactory = _mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			_unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
			_statisticLoader = _mocks.DynamicMock<IStatisticLoader>();
			_reforecastPercentCalculator = _mocks.DynamicMock<IReforecastPercentCalculator>();
			_target = new RecalculateForecastOnSkillConsumer(_scenarioRepository, _skillDayRepository, _skillRepository,
															 _currentunitOfWorkFactory, _statisticLoader, _reforecastPercentCalculator);
			_uow = _mocks.DynamicMock<IUnitOfWork>();
		}

		[Test]
		public void ShouldOnlyDoDefaultScenario()
		{
			var scenarioId = Guid.NewGuid();
			_scenario = _mocks.StrictMock<IScenario>();
			var message = new RecalculateForecastOnSkillMessageCollection {ScenarioId = scenarioId};
			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
			Expect.Call(_scenario.DefaultScenario).Return(false);
			_mocks.ReplayAll();
			_target.Consume(message);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSkipIfWrongSkillId()
		{
			var scenarioId = Guid.NewGuid();
			_scenario = _mocks.StrictMock<IScenario>();
			var skillMessage = new RecalculateForecastOnSkillMessage();
			var message = new RecalculateForecastOnSkillMessageCollection
			              	{
			              		ScenarioId = scenarioId,
			              		MessageCollection = new Collection<RecalculateForecastOnSkillMessage> {skillMessage}
			              	};
			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
			Expect.Call(_scenario.DefaultScenario).Return(true);
			Expect.Call(_skillRepository.Get(Guid.Empty)).Return(null);
			_mocks.ReplayAll();
			_target.Consume(message);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldLoadSkillDaysOnSkill()
		{
			var scenarioId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var workloadId = Guid.NewGuid();
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
			_scenario = _mocks.StrictMock<IScenario>();
			var skill = _mocks.DynamicMock<ISkill>();
			var skillMessage = new RecalculateForecastOnSkillMessage { SkillId = skillId, WorkloadIds = new Collection<Guid> { workloadId } };
			var message = new RecalculateForecastOnSkillMessageCollection
			{
				ScenarioId = scenarioId,
				MessageCollection = new Collection<RecalculateForecastOnSkillMessage> { skillMessage }
			};
			var skillDay = _mocks.DynamicMock<ISkillDay>();
			var workloadDay = _mocks.DynamicMock<IWorkloadDay>();
			var workload = _mocks.DynamicMock<IWorkload>();
			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
			Expect.Call(_scenario.DefaultScenario).Return(true);
			Expect.Call(_skillRepository.Get(skillId)).Return(skill);
			Expect.Call(skill.TimeZone).Return(timezone);
			Expect.Call(_skillDayRepository.FindRange(new DateOnlyPeriod(), skill, _scenario)).IgnoreArguments().Return(
				new Collection<ISkillDay>{skillDay});
			Expect.Call(skillDay.WorkloadDayCollection).Return(new ReadOnlyCollection<IWorkloadDay>(new List<IWorkloadDay> { workloadDay }));
			Expect.Call(workloadDay.Workload).Return(workload);
			Expect.Call(workload.Id).Return(Guid.NewGuid());
			_mocks.ReplayAll();
			_target.Consume(message);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldRecalculateWorkloadDay()
		{
			var scenarioId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var workloadId = Guid.NewGuid();
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
			_scenario = _mocks.StrictMock<IScenario>();
			var skill = _mocks.DynamicMock<ISkill>();
			var skillMessage = new RecalculateForecastOnSkillMessage { SkillId = skillId, WorkloadIds = new Collection<Guid> { workloadId } };
			var message = new RecalculateForecastOnSkillMessageCollection
			{
				ScenarioId = scenarioId,
				MessageCollection = new Collection<RecalculateForecastOnSkillMessage> { skillMessage }
			};
			var skillDay = _mocks.DynamicMock<ISkillDay>();
			var workloadDay = _mocks.DynamicMock<IWorkloadDay>();
			var workload = _mocks.DynamicMock<IWorkload>();
			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_scenarioRepository.Get(scenarioId)).Return(_scenario);
			Expect.Call(_scenario.DefaultScenario).Return(true);
			Expect.Call(_skillRepository.Get(skillId)).Return(skill);
			Expect.Call(skill.TimeZone).Return(timezone);
			Expect.Call(_skillDayRepository.FindRange(new DateOnlyPeriod(), skill, _scenario)).IgnoreArguments().Return(
				new Collection<ISkillDay> { skillDay });
			Expect.Call(skillDay.WorkloadDayCollection).Return(new ReadOnlyCollection<IWorkloadDay>(new List<IWorkloadDay> { workloadDay }));
			Expect.Call(workloadDay.Workload).Return(workload);
			Expect.Call(workload.Id).Return(workloadId);
			Expect.Call(_statisticLoader.Execute(new DateTimePeriod(), workloadDay, null)).IgnoreArguments().Return(
				new DateTime(2012, 12, 18));
			Expect.Call(_reforecastPercentCalculator.Calculate(workloadDay, new DateTime(2012, 12, 18))).Return(1.1);
			_mocks.ReplayAll();
			_target.Consume(message);
			_mocks.VerifyAll();
		}
	}


}