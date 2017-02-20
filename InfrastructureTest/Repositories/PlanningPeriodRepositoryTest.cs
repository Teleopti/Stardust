using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	public class PlanningPeriodRepositoryTest : RepositoryTest<IPlanningPeriod>
	{
		private static readonly DateOnly _startDate = new DateOnly(2001, 1, 1);
		private IPerson person;
		private DateOnlyPeriod period;
		private DateTime timestamp;
		private IJobResult jobResult;

		protected override void ConcreteSetup()
		{
			period = new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31);
			person = PersonFactory.CreatePerson();
			timestamp = DateTime.UtcNow;
			PersistAndRemoveFromUnitOfWork(person);
			jobResult = new JobResult(JobCategory.WebSchedule, period, person, timestamp);
			PersistAndRemoveFromUnitOfWork(jobResult);
		}

		protected override IPlanningPeriod CreateAggregateWithCorrectBusinessUnit()
		{
			var planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			planningPeriod.JobResults.Add(jobResult);
			return planningPeriod;
		}

		protected override void VerifyAggregateGraphProperties(IPlanningPeriod loadedAggregateFromDatabase)
		{
			var aggregateWithCorrectBusinessUnit = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.Range.Should().Be.EqualTo(aggregateWithCorrectBusinessUnit.Range);
			loadedAggregateFromDatabase.AgentGroup.Should().Be.EqualTo(aggregateWithCorrectBusinessUnit.Range);
			loadedAggregateFromDatabase.JobResults.Single().JobCategory.Should().Be.EqualTo(aggregateWithCorrectBusinessUnit.JobResults.Single().JobCategory);
			loadedAggregateFromDatabase.JobResults.Single().Owner.Should().Be.EqualTo(aggregateWithCorrectBusinessUnit.JobResults.Single().Owner);
		}

		protected override Repository<IPlanningPeriod> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PlanningPeriodRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldGetPlanningPeriodsForAgentGroup()
		{
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var agentGroup = new AgentGroup("test agent group");
			PersistAndRemoveFromUnitOfWork(agentGroup);
			PersistAndRemoveFromUnitOfWork(new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()), agentGroup));

			var planningPeriods = repository.LoadForAgentGroup(agentGroup);

			planningPeriods.SingleOrDefault().AgentGroup.Name.Should().Be.EqualTo(agentGroup.Name);
		}

		[Test]
		public void ShouldGetMostCommonlyUsedSchedulePeriods()
		{
			SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[] {() => new SchedulePeriod(_startDate, SchedulePeriodType.Week, 1)});
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)));
			var range = planningPeriodSuggestions.Default().Range;
			range.StartDate.Should().Be.EqualTo(new DateOnly(2015, 04, 6));
			range.EndDate.Should().Be.EqualTo(new DateOnly(2015, 04, 12));
		}

		[Test]
		public void ShouldGetMonthAsDefaultWhenNoSchedulePeriods()
		{
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)));

			var defaultRange = planningPeriodSuggestions.Default().Range;
			defaultRange.StartDate.Should().Be.EqualTo(new DateOnly(2015, 5, 1));
			defaultRange.EndDate.Should().Be.EqualTo(new DateOnly(2015, 5, 31));
		}

		[Test]
		public void ShouldGetSuggestions()
		{
			SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[] { () => new SchedulePeriod(_startDate, SchedulePeriodType.Week, 1) });
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)));

			var suggestedPeriod = planningPeriodSuggestions.SuggestedPeriods(new DateOnlyPeriod(new DateOnly(2015, 4, 6),new DateOnly(2015, 4,12)));
			suggestedPeriod.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldMergeSuggestionsWithSameResultingRangeAndPeriodDetails()
		{
			SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[]
			{
				() => new SchedulePeriod(_startDate, SchedulePeriodType.Week, 1),
				() => new SchedulePeriod(_startDate.AddDays(7), SchedulePeriodType.Week, 1)
			});
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)));

			var suggestedPeriod = planningPeriodSuggestions.SuggestedPeriods(new DateOnlyPeriod(new DateOnly(2015, 4, 6), new DateOnly(2015, 4, 12)));
			suggestedPeriod.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldNotMergeSuggestionsWithSamePeriodDetailsWhenRangeDiffers()
		{
			SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[]
			{
				() => new SchedulePeriod(_startDate, SchedulePeriodType.Week, 1),
				() => new SchedulePeriod(_startDate.AddDays(5), SchedulePeriodType.Week, 1)
			});
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)));

			var suggestedPeriod = planningPeriodSuggestions.SuggestedPeriods(new DateOnlyPeriod(new DateOnly(2015, 4, 6), new DateOnly(2015, 4, 12)));
			suggestedPeriod.Count().Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldReturnPlanningPeriodSuggestions()
		{
			SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[]
			{
				() => new SchedulePeriod(_startDate, SchedulePeriodType.Week, 1),
				() => new SchedulePeriod(_startDate.AddDays(5), SchedulePeriodType.Week, 1)
			});
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)));

			var suggestedPeriod = planningPeriodSuggestions.SuggestedPeriods(new DateOnlyPeriod(new DateOnly(2015, 4, 6), new DateOnly(2015, 4, 12)));
			suggestedPeriod.Count().Should().Be.EqualTo(7);
		}

	
		private void SetupPersonsInOrganizationWithContract(IEnumerable<Func<SchedulePeriod>> schedulePeriodTypes)
		{
			ITeam team = TeamFactory.CreateSimpleTeam("hola");
			ISite site = SiteFactory.CreateSimpleSite();
			site.AddTeam(team);
			IActivity act = new Activity("for test");
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			ISkill skill = new Skill("for test", "sdf", Color.Blue, 3, skType);
			skill.Activity = act;
			skill.TimeZone = TimeZoneInfo.Local;

			PersonPeriod okPeriod = new PersonPeriod(_startDate, createPersonContract2(), team);
			okPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			
			PersistAndRemoveFromUnitOfWork(act);
			PersistAndRemoveFromUnitOfWork(skType);
			PersistAndRemoveFromUnitOfWork(skill);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);

			createPersonAndSchedulePeriod(okPeriod, schedulePeriodTypes);

			forceReadModelUpdate();
		}

		private void forceReadModelUpdate()
		{
			var groupingReadModel = new GroupingReadOnlyRepository(new ThisUnitOfWork(UnitOfWork));
			groupingReadModel.UpdateGroupingReadModel(new[] {Guid.Empty});
		}

		private void createPersonAndSchedulePeriod(PersonPeriod okPeriod, IEnumerable<Func<SchedulePeriod>> schedulePeriodTypes)
		{
			schedulePeriodTypes.ForEach(x =>
			{
				SchedulePeriod schedulePeriod2 = x();
				var sCategory = new ShiftCategory("for test");
				schedulePeriod2.AddShiftCategoryLimitation(new ShiftCategoryLimitation(sCategory));

				IPerson okPerson = PersonFactory.CreatePerson("hejhej");
				okPerson.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
				okPerson.AddPersonPeriod(okPeriod.NoneEntityClone());
				okPerson.AddSchedulePeriod(schedulePeriod2);

				PersistAndRemoveFromUnitOfWork(sCategory);
				PersistAndRemoveFromUnitOfWork(okPerson);
			});
		}

		private IPersonContract createPersonContract2()
		{
			var pContract = PersonContractFactory.CreatePersonContract();
			PersistAndRemoveFromUnitOfWork(pContract.Contract);
			PersistAndRemoveFromUnitOfWork(pContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(pContract.PartTimePercentage);
			return pContract;
		}
	}
}
