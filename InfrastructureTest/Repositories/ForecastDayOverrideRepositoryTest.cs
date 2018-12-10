using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class ForecastDayOverrideRepositoryTest: RepositoryTest<IForecastDayOverride>
	{
		private IScenario _scenario;
		private IWorkload _workload;

		protected override void ConcreteSetup()
		{
			base.ConcreteSetup();
			_scenario = ScenarioFactory.CreateScenario("test scenario", true, false);
			PersistAndRemoveFromUnitOfWork(_scenario);

			var activity = new Activity("test activity");
			PersistAndRemoveFromUnitOfWork(activity);

			var skill = SkillFactory.CreateSkill("test skill");
			skill.Activity = activity;
			PersistAndRemoveFromUnitOfWork(skill.SkillType);
			PersistAndRemoveFromUnitOfWork(skill);

			_workload = WorkloadFactory.CreateWorkload("test workload", skill);
			PersistAndRemoveFromUnitOfWork(_workload);
		}

		protected override IForecastDayOverride CreateAggregateWithCorrectBusinessUnit()
		{
			return new ForecastDayOverride(new DateOnly(2018, 1, 1), _workload, _scenario);
		}

		protected override void VerifyAggregateGraphProperties(IForecastDayOverride loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Date.Should().Be(new DateOnly(2018, 1, 1));
			loadedAggregateFromDatabase.Workload.Should().Be(_workload);
			loadedAggregateFromDatabase.Scenario.Should().Be(_scenario);
		}

		protected override Repository<IForecastDayOverride> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ForecastDayOverrideRepository(currentUnitOfWork);
		}
	}
}
