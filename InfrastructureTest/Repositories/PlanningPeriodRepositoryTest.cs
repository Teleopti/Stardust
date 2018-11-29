using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	public class PlanningPeriodRepositoryTest : RepositoryTest<PlanningPeriod>
	{
		private static readonly DateOnly startDate = new DateOnly(2001, 1, 1);
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

		protected override PlanningPeriod CreateAggregateWithCorrectBusinessUnit()
		{
			var planningGroup = new PlanningGroup();
			PersistAndRemoveFromUnitOfWork(planningGroup);
			var planningPeriod = new PlanningPeriod(new DateOnly(2015,4,1),SchedulePeriodType.Week, 1, planningGroup);
			planningPeriod.JobResults.Add(jobResult);
			return planningPeriod;
		}

		protected override void VerifyAggregateGraphProperties(PlanningPeriod loadedAggregateFromDatabase)
		{
			var aggregateWithCorrectBusinessUnit = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.Range.Should().Be.EqualTo(aggregateWithCorrectBusinessUnit.Range);
			loadedAggregateFromDatabase.JobResults.Single().JobCategory.Should().Be.EqualTo(aggregateWithCorrectBusinessUnit.JobResults.Single().JobCategory);
			loadedAggregateFromDatabase.JobResults.Single().Owner.Should().Be.EqualTo(aggregateWithCorrectBusinessUnit.JobResults.Single().Owner);
		}

		protected override Repository<PlanningPeriod> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PlanningPeriodRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldNotAllowNullAsPlanningGroup()
		{
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriod = new PlanningPeriod(DateOnly.Today, SchedulePeriodType.Day, 1, null);

			Assert.Throws<DataSourceException>(() =>
			{
				repository.Add(planningPeriod);
				Session.Flush();
			});
		}

		[Test]
		public void ShouldGetPlanningPeriodsForPlanningGroup()
		{
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningGroup = new PlanningGroup();
			PersistAndRemoveFromUnitOfWork(planningGroup);
			PersistAndRemoveFromUnitOfWork(new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()), planningGroup));

			var planningPeriods = repository.LoadForPlanningGroup(planningGroup);

			planningPeriods.SingleOrDefault().PlanningGroup.Name.Should().Be.EqualTo(planningGroup.Name);
		}

		[Test]
		public void ShouldGetMostCommonlyUsedSchedulePeriods()
		{
			SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[] {() => new SchedulePeriod(startDate, SchedulePeriodType.Week, 1)});
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
			SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[] { () => new SchedulePeriod(startDate, SchedulePeriodType.Week, 1) });
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)));

			var suggestedPeriod = planningPeriodSuggestions.SuggestedPeriods(new DateOnlyPeriod(new DateOnly(2015, 4, 6),new DateOnly(2015, 4,12)));
			suggestedPeriod.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldMergeSuggestionsWithSameResultingRangeAndPeriodDetails()
		{
			SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[]
			{
				() => new SchedulePeriod(startDate, SchedulePeriodType.Week, 1),
				() => new SchedulePeriod(startDate.AddDays(7), SchedulePeriodType.Week, 1)
			});
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)));

			var suggestedPeriod = planningPeriodSuggestions.SuggestedPeriods(new DateOnlyPeriod(new DateOnly(2015, 4, 6), new DateOnly(2015, 4, 12)));
			suggestedPeriod.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotMergeSuggestionsWithSamePeriodDetailsWhenRangeDiffers()
		{
			SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[]
			{
				() => new SchedulePeriod(startDate, SchedulePeriodType.Week, 1),
				() => new SchedulePeriod(startDate.AddDays(5), SchedulePeriodType.Week, 1)
			});
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)));

			var suggestedPeriod = planningPeriodSuggestions.SuggestedPeriods(new DateOnlyPeriod(new DateOnly(2015, 4, 6), new DateOnly(2015, 4, 12)));
			suggestedPeriod.Count().Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldReturnPlanningPeriodSuggestions()
		{
			SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[]
			{
				() => new SchedulePeriod(startDate, SchedulePeriodType.Week, 1),
				() => new SchedulePeriod(startDate.AddDays(5), SchedulePeriodType.Week, 1)
			});

			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)));

			var suggestedPeriod = planningPeriodSuggestions.SuggestedPeriods(new DateOnlyPeriod(new DateOnly(2015, 4, 6), new DateOnly(2015, 4, 12)));
			suggestedPeriod.Count().Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldReturnSuggestionsForAgentsWithFirstSchedulePeriodStartingLaterThanToday()
		{
			var addedPeople = SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[]
			{
				() => new SchedulePeriod(new DateOnly(2017,06,12), SchedulePeriodType.Week, 1),
			});
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<Guid> { addedPeople.First().Id.GetValueOrDefault() });

			var suggestedPeriod = planningPeriodSuggestions.Default();
			suggestedPeriod.Number.Should().Be.EqualTo(1);
			suggestedPeriod.PeriodType.Should().Be.EqualTo(SchedulePeriodType.Week);
		}

		[Test]
		public void ShouldReturnSuggestionsOnlyForSpecificPeople()
		{
			var addedPeople = SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[]
			{
				() => new SchedulePeriod(startDate, SchedulePeriodType.Week, 1),
				() => new SchedulePeriod(startDate, SchedulePeriodType.Month, 1)
			});
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<Guid>{ addedPeople.First().Id.GetValueOrDefault()});

			var suggestedPeriod = planningPeriodSuggestions.Default();
			suggestedPeriod.Number.Should().Be.EqualTo(1);
			suggestedPeriod.PeriodType.Should().Be.EqualTo(SchedulePeriodType.Week);
		}

		[Test]
		public void ShouldNotShowDaySuggestions()
		{
			var addedPeople = SetupPersonsInOrganizationWithContract(new Func<SchedulePeriod>[]
			{
				() => new SchedulePeriod(new DateOnly(2017,06,12), SchedulePeriodType.Day, 1),
			});
			var repository = new PlanningPeriodRepository(CurrUnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<Guid> { addedPeople.First().Id.GetValueOrDefault() });

			var suggestedPeriod = planningPeriodSuggestions.Default();
			suggestedPeriod.PeriodType.Should().Not.Be.EqualTo(SchedulePeriodType.Day);
		}


		private IList<IPerson> SetupPersonsInOrganizationWithContract(IEnumerable<Func<SchedulePeriod>> schedulePeriodTypes)
		{
			ITeam team = TeamFactory.CreateSimpleTeam("hola");
			ISite site = SiteFactory.CreateSimpleSite();
			site.AddTeam(team);
			IActivity act = new Activity("for test");
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			ISkill skill = new Skill("for test", "sdf", Color.Blue, 3, skType)
			{
				Activity = act,
				TimeZone = TimeZoneInfo.Local
			};
			var okPeriod = new PersonPeriod(startDate, createPersonContract2(), team);
			okPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			
			PersistAndRemoveFromUnitOfWork(act);
			PersistAndRemoveFromUnitOfWork(skType);
			PersistAndRemoveFromUnitOfWork(skill);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);

			var people = createPersonAndSchedulePeriod(okPeriod, schedulePeriodTypes);

			forceReadModelUpdate();
			return people;
		}

		private void forceReadModelUpdate()
		{
			var groupingReadModel = new GroupingReadOnlyRepository(CurrUnitOfWork);
			groupingReadModel.UpdateGroupingReadModel(new[] {Guid.Empty});
		}

		private IList<IPerson> createPersonAndSchedulePeriod(PersonPeriod okPeriod, IEnumerable<Func<SchedulePeriod>> schedulePeriodTypes)
		{
			return schedulePeriodTypes.Select(schedulePeriodCreator =>
			{
				var schedulePeriod2 = schedulePeriodCreator();
				var sCategory = new ShiftCategory("for test");
				schedulePeriod2.AddShiftCategoryLimitation(new ShiftCategoryLimitation(sCategory));

				IPerson okPerson = PersonFactory.CreatePerson("hejhej");
				okPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
				okPerson.AddPersonPeriod(okPeriod.NoneEntityClone());
				okPerson.AddSchedulePeriod(schedulePeriod2);

				PersistAndRemoveFromUnitOfWork(sCategory);
				PersistAndRemoveFromUnitOfWork(okPerson);
				return okPerson;
			}).ToList();
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
