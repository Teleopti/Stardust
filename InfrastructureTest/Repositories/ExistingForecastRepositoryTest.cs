using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


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
		private void concreteSetup()
		{
			_skill = SkillFactory.CreateSkill("dummy", _skillType, 15);

			_skill.Activity = _activity;

			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			_skill.TimeZone = timeZoneInfo;
			PersistAndRemoveFromUnitOfWork(_skill);

			_workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(_skill);
			PersistAndRemoveFromUnitOfWork(_workload);

			_date = timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2015, 5, 8, 2, 0, 0));
		}

		private void createBasicStuff()
		{
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			PersistAndRemoveFromUnitOfWork(_scenario);
			_activity = new Activity("dummyActivity");
			_skillType = SkillTypeFactory.CreateSkillTypePhone();
			PersistAndRemoveFromUnitOfWork(_activity);
			PersistAndRemoveFromUnitOfWork(_skillType);
		}

		/// <summary>
		/// Creates an aggreagte using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		private ISkillDay createSkillDay(IList<IWorkloadDay> workloads)
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

			var skillDay = new SkillDay(new DateOnly(_date), _skill, _scenario, workloads, skillDataPeriods);

			_date = _date.AddDays(1);

			return skillDay;
		}

		private IWorkloadDay createWorkload()
		{
			var workloadDay = new WorkloadDay();
			workloadDay.Create(new DateOnly(_date), _workload, new List<TimePeriod> { new TimePeriod("12:30-17:30") });
			workloadDay.Tasks = 7 * 20;
			workloadDay.AverageTaskTime = TimeSpan.FromSeconds(22);
			workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(233);
			return workloadDay;
		}

		[Test]
		public void ShouldReturnExistingForecast()
		{
			createBasicStuff();
			concreteSetup();
			var day1 = createSkillDay(new[] { createWorkload() });
			var day2 = createSkillDay(new[] { createWorkload() });

			PersistAndRemoveFromUnitOfWork(day1);
			PersistAndRemoveFromUnitOfWork(day2);

			var target = new ExistingForecastRepository(new ThisUnitOfWork(UnitOfWork));

			var range = new DateOnlyPeriod(2015, 05, 01, 2015, 05, 31);
			var result = target.ExistingForecastForAllSkills(range, _scenario).ToList();
			result.Count.Should().Be(1);
			result.First().Periods.Count().Should().Be(1);
			result.First().Periods.First().Should().Be(new DateOnlyPeriod(2015, 05, 08, 2015, 05, 09));
		}

		[Test]
		public void ShouldReturnExistingForecastOnTwoSkills()
		{
			createBasicStuff();
			concreteSetup();
			var day1 = createSkillDay(new[] { createWorkload() });
			concreteSetup();
			var day2 = createSkillDay(new[] { createWorkload() });

			PersistAndRemoveFromUnitOfWork(day1);
			PersistAndRemoveFromUnitOfWork(day2);

			var target = new ExistingForecastRepository(new ThisUnitOfWork(UnitOfWork));

			var range = new DateOnlyPeriod(2015, 05, 01, 2015, 05, 31);
			var result = target.ExistingForecastForAllSkills(range, _scenario).ToList();
			result.Count.Should().Be(2);
			result.First().Periods.Count().Should().Be(1);
			result.First().Periods.First().Should().Be(new DateOnlyPeriod(2015, 05, 08, 2015, 05, 08));

			result.Last().Periods.Count().Should().Be(1);
			result.Last().Periods.First().Should().Be(new DateOnlyPeriod(2015, 05, 08, 2015, 05, 08));
		}

		[Test]
		public void ShouldReturnExistingForecastOnTwoSkillsAndDifferentLengths()
		{
			createBasicStuff();
			concreteSetup();
			var day1 = createSkillDay(new[] { createWorkload() });
			concreteSetup();
			var day2 = createSkillDay(new[] { createWorkload() });
			var day3 = createSkillDay(new[] { createWorkload() });

			PersistAndRemoveFromUnitOfWork(day1);
			PersistAndRemoveFromUnitOfWork(day2);
			PersistAndRemoveFromUnitOfWork(day3);

			var target = new ExistingForecastRepository(new ThisUnitOfWork(UnitOfWork));

			var range = new DateOnlyPeriod(2015, 05, 01, 2015, 05, 31);
			var result = target.ExistingForecastForAllSkills(range, _scenario).ToList();
			result.Count.Should().Be(2);
			result.First(x => x.SkillId == day1.Skill.Id).Periods.Count().Should().Be(1);
			result.First(x => x.SkillId == day1.Skill.Id).Periods.First().Should().Be(new DateOnlyPeriod(2015, 05, 08, 2015, 05, 08));

			result.Last(x => x.SkillId == day2.Skill.Id).Periods.Count().Should().Be(1);
			result.Last(x => x.SkillId == day2.Skill.Id).Periods.First().Should().Be(new DateOnlyPeriod(2015, 05, 08, 2015, 05, 09));
		}

		[Test]
		public void ShouldReturnExistingForecastForMultipleWorkloads()
		{
			createBasicStuff();
			concreteSetup();
			var day1 = createSkillDay(new[] { createWorkload(), createWorkload() });
			var day2 = createSkillDay(new[] { createWorkload() });

			PersistAndRemoveFromUnitOfWork(day1);
			PersistAndRemoveFromUnitOfWork(day2);

			var target = new ExistingForecastRepository(new ThisUnitOfWork(UnitOfWork));

			var range = new DateOnlyPeriod(2015, 05, 01, 2015, 05, 31);
			var result = target.ExistingForecastForAllSkills(range, _scenario).ToList();
			result.Count.Should().Be(1);
			result.First().Periods.Count().Should().Be(1);
			result.First().Periods.First().Should().Be(new DateOnlyPeriod(2015, 05, 08, 2015, 05, 09));
		}

		[Test]
		public void ShouldReturnSkillWithNoForecast()
		{
			createBasicStuff();
			concreteSetup();
			var day1 = createSkillDay(new[] { createWorkload() });

			PersistAndRemoveFromUnitOfWork(day1);

			var target = new ExistingForecastRepository(new ThisUnitOfWork(UnitOfWork));

			var range = new DateOnlyPeriod(2015, 06, 01, 2015, 06, 30);
			var result = target.ExistingForecastForAllSkills(range, _scenario).ToList();
			result.Count.Should().Be(1);
			result.First().Periods.Should().Be.Empty();
		}
	}
}
