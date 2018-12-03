using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	public class RecalculateForecastOnSkillHandlerTest
	{
		private RecalculateForecastOnSkillEventHandler _target;
		private IScenarioRepository _scenarioRepository;
		private ISkillDayRepository _skillDayRepository;
		private ISkillRepository _skillRepository;
		private IStatisticLoader _statisticLoader;
		private IReforecastPercentCalculator _reforecastPercentCalculator;
		private IScenario _scenario;

		[SetUp]
		public void Setup()
		{
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_statisticLoader = MockRepository.GenerateMock<IStatisticLoader>();
			_reforecastPercentCalculator = MockRepository.GenerateMock<IReforecastPercentCalculator>();
			_target = new RecalculateForecastOnSkillEventHandler(_scenarioRepository, _skillDayRepository, _skillRepository,
				 _statisticLoader, _reforecastPercentCalculator);
		}

		[Test]
		public void ShouldOnlyDoDefaultScenario()
		{
			var scenarioId = Guid.NewGuid();
			_scenario = MockRepository.GenerateStrictMock<IScenario>();
			var message = new RecalculateForecastOnSkillCollectionEvent {ScenarioId = scenarioId};
			_scenarioRepository.Stub(x => x.Get(scenarioId)).Return(_scenario);
			_scenario.Stub(x => x.DefaultScenario).Return(false);
			_target.Handle(message);
		}

		[Test]
		public void ShouldSkipIfWrongSkillId()
		{
			var scenarioId = Guid.NewGuid();
			_scenario = MockRepository.GenerateStrictMock<IScenario>();
			var skillMessage = new RecalculateForecastOnSkill();
			var message = new RecalculateForecastOnSkillCollectionEvent
			{
				ScenarioId = scenarioId,
				SkillCollection = new Collection<RecalculateForecastOnSkill> {skillMessage}
			};
			_scenarioRepository.Stub(x => x.Get(scenarioId)).Return(_scenario);
			_scenario.Stub(x => x.DefaultScenario).Return(true);
			_skillRepository.Stub(x => x.Get(Guid.Empty)).Return(null);

			_target.Handle(message);

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
			var skillMessage = new RecalculateForecastOnSkill
			{
				SkillId = skillId,
				WorkloadIds = new Collection<Guid> {workloadId}
			};
			var message = new RecalculateForecastOnSkillCollectionEvent
			{
				ScenarioId = scenarioId,
				SkillCollection = new Collection<RecalculateForecastOnSkill> {skillMessage}
			};
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var workloadDay = MockRepository.GenerateMock<IWorkloadDay>();
			var workload = MockRepository.GenerateMock<IWorkload>();
			_scenarioRepository.Stub(x => x.Get(scenarioId)).Return(_scenario);
			_scenario.Stub(x => x.DefaultScenario).Return(true);
			_skillRepository.Stub(x => x.Get(skillId)).Return(skill);
			skill.Stub(x => x.TimeZone).Return(timezone);
			_skillDayRepository.Stub(x => x.FindRange(new DateOnlyPeriod(), skill, _scenario)).IgnoreArguments().Return(
				new Collection<ISkillDay> {skillDay});
			skillDay.Stub(x => x.WorkloadDayCollection)
				.Return(new ReadOnlyCollection<IWorkloadDay>(new List<IWorkloadDay> {workloadDay}));
			workloadDay.Stub(x => x.Workload).Return(workload);
			workload.Stub(x => x.Id).Return(Guid.NewGuid());
			_target.Handle(message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void ShouldRecalculateWorkloadDay()
		{
			var scenarioId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var workloadId = Guid.NewGuid();
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
			_scenario = MockRepository.GenerateStrictMock<IScenario>();
			var skill = MockRepository.GenerateMock<ISkill>();
			var skillMessage = new RecalculateForecastOnSkill
			{
				SkillId = skillId,
				WorkloadIds = new Collection<Guid> {workloadId}
			};
			var message = new RecalculateForecastOnSkillCollectionEvent
			{
				ScenarioId = scenarioId,
				SkillCollection = new Collection<RecalculateForecastOnSkill> {skillMessage}
			};
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var workloadDay = MockRepository.GenerateStrictMock<IWorkloadDay>();
			var workload = MockRepository.GenerateMock<IWorkload>();
			_scenarioRepository.Stub(x => x.Get(scenarioId)).Return(_scenario);
			_scenario.Stub(x => x.DefaultScenario).Return(true);
			_skillRepository.Stub(x => x.Get(skillId)).Return(skill);
			skill.Stub(x => x.TimeZone).Return(timezone);
			_skillDayRepository.Stub(x => x.FindRange(new DateOnlyPeriod(), skill, _scenario)).IgnoreArguments().Return(
				new Collection<ISkillDay> {skillDay});
			skillDay.Stub(x => x.WorkloadDayCollection)
				.Return(new ReadOnlyCollection<IWorkloadDay>(new List<IWorkloadDay> {workloadDay}));
			workloadDay.Stub(x => x.Workload).Return(workload);
			workload.Stub(x => x.Id).Return(workloadId);
			_statisticLoader.Stub(x => x.Execute(new DateTimePeriod(), workloadDay, null)).IgnoreArguments().Return(
				new DateTime(2012, 12, 18));
			_reforecastPercentCalculator.Stub(x => x.Calculate(workloadDay, new DateTime(2012, 12, 18))).Return(1.1);
			workloadDay.Stub(x => x.Tasks).Return(100);
			workloadDay.Stub(x => x.Tasks).SetPropertyWithArgument(100*1.1);
			workloadDay.Stub(x => x.CampaignTasks).SetPropertyWithArgument(new Percent(0));
			_target.Handle(message);
		}
	}
}