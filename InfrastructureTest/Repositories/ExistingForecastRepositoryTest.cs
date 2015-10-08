using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class ExistingForecastRepositoryTest : DatabaseTest
	{
		private IScenario _scenario;
		private ISkillType _skillType;
		private IActivity _activity;
		private ISkill _skill;
		private IWorkload _workload;
		private DateTime _date;

		/// <summary>
		/// Runs every test. Implemented by repository's concrete implementation.
		/// </summary>
		private void ConcreteSetup()
		{
			_skill = SkillFactory.CreateSkill("dummy", _skillType, 15);
			
			_skill.Activity = _activity;

			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			_skill.TimeZone = timeZoneInfo;
			PersistAndRemoveFromUnitOfWork(_skill);

			_workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(_skill);
			PersistAndRemoveFromUnitOfWork(_workload);

			_date = timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2015, 5, 8, 2, 0, 0));
		}

		private void CreateBasicStuff()
		{
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			PersistAndRemoveFromUnitOfWork(_scenario);
			_activity = new Activity("dummyActivity");
			_skillType = SkillTypeFactory.CreateSkillType();
			PersistAndRemoveFromUnitOfWork(_activity);
			PersistAndRemoveFromUnitOfWork(_skillType);
		}

		/// <summary>
		/// Creates an aggreagte using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		private ISkillDay CreateSkillDay(IList<IWorkloadDay> workloads)
		{
			IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();
			skillDataPeriods.Add(
				 new SkillDataPeriod(
					  ServiceAgreement.DefaultValues(),
					  new SkillPersonData(2, 5),
					  new DateTimePeriod(_date, _date.Add(TimeSpan.FromHours(12)))));
			skillDataPeriods.Add(
				 new SkillDataPeriod(
					  skillDataPeriods[0].ServiceAgreement,
					  skillDataPeriods[0].SkillPersonData,
					  skillDataPeriods[0].Period.MovePeriod(TimeSpan.FromHours(12))));

			SkillDay skillDay = new SkillDay(new DateOnly(_date), _skill, _scenario,
				 workloads, skillDataPeriods);

			_date = _date.AddDays(1);

			return skillDay;
		}

		private WorkloadDay CreateWorkload()
		{
			IList<TimePeriod> openHourPeriods = new List<TimePeriod>();
			openHourPeriods.Add(new TimePeriod("12:30-17:30"));

			WorkloadDay workloadDay = new WorkloadDay();
			workloadDay.Create(new DateOnly(_date), _workload, openHourPeriods);
			workloadDay.Tasks = 7 * 20;
			workloadDay.AverageTaskTime = TimeSpan.FromSeconds(22);
			workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(233);
			return workloadDay;
		}
	

		[Test]
		public void ShouldReturnExistingForecast()
		{
			CreateBasicStuff();
			ConcreteSetup();
			var day1 = CreateSkillDay(new[] { CreateWorkload ()});
			var day2 = CreateSkillDay(new[] { CreateWorkload() });

			PersistAndRemoveFromUnitOfWork(day1);
			PersistAndRemoveFromUnitOfWork(day2);

			var target = new ExistingForecastRepository(new ThisUnitOfWork(UnitOfWork));

			var range = new DateOnlyPeriod(2015, 05, 01, 2015, 05, 31);
			var result =  target.ExistingForecastForAllSkills(range, _scenario );
			result.Count().Should().Be(1);
			result.First().Item2.Count().Should().Be(1);
			result.First().Item2.First().Should().Be(new DateOnlyPeriod(2015, 05, 8, 2015, 05, 9));
		}

		[Test]
		public void ShouldReturnExistingForecastOnTwoSkills()
		{
			CreateBasicStuff();
			ConcreteSetup();
			var day1 = CreateSkillDay(new[] { CreateWorkload() });
			ConcreteSetup();
			var day2 = CreateSkillDay(new[] { CreateWorkload() });

			PersistAndRemoveFromUnitOfWork(day1);
			PersistAndRemoveFromUnitOfWork(day2);

			var target = new ExistingForecastRepository(new ThisUnitOfWork(UnitOfWork));

			var range = new DateOnlyPeriod(2015, 05, 01, 2015, 05, 31);
			var result = target.ExistingForecastForAllSkills(range, _scenario);
			result.Count().Should().Be(2);
			result.First().Item2.Count().Should().Be(1);
			result.First().Item2.First().Should().Be(new DateOnlyPeriod(2015, 05, 8, 2015, 05, 8));

			result.Last().Item2.Count().Should().Be(1);
			result.Last().Item2.First().Should().Be(new DateOnlyPeriod(2015, 05, 8, 2015, 05, 8));
		}

		[Test]
		public void ShouldReturnExistingForecastForMultipleWorkloads()
		{
			CreateBasicStuff();
			ConcreteSetup();
			var day1 = CreateSkillDay(new[] { CreateWorkload(), CreateWorkload() });
			var day2 = CreateSkillDay(new[] { CreateWorkload() });

			PersistAndRemoveFromUnitOfWork(day1);
			PersistAndRemoveFromUnitOfWork(day2);

			var target = new ExistingForecastRepository(new ThisUnitOfWork(UnitOfWork));

			var range = new DateOnlyPeriod(2015, 05, 01, 2015, 05, 31);
			var result = target.ExistingForecastForAllSkills(range, _scenario);
			result.Count().Should().Be(1);
			result.First().Item2.Count().Should().Be(1);
			result.First().Item2.First().Should().Be(new DateOnlyPeriod(2015, 05, 8, 2015, 05, 9));
		}

		[Test]
		public void ShouldReturnSkillWithNoForecast()
		{
			CreateBasicStuff();
			ConcreteSetup();
			var day1 = CreateSkillDay(new[] { CreateWorkload() });

			PersistAndRemoveFromUnitOfWork(day1);

			var target = new ExistingForecastRepository(new ThisUnitOfWork(UnitOfWork));

			var range = new DateOnlyPeriod(2015, 06, 01, 2015, 06, 30);
			var result = target.ExistingForecastForAllSkills(range, _scenario);
			result.Count().Should().Be(1);
			result.First().Item2.Should().Be.Empty();
		}
	}
}
