using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class PlanningPeriodRepositoryTest : RepositoryTest<IPlanningPeriod>
	{
		protected override IPlanningPeriod CreateAggregateWithCorrectBusinessUnit()
		{
			return new PlanningPeriod(new TestableNow(new DateTime(2015,4,1)));
		}

		protected override void VerifyAggregateGraphProperties(IPlanningPeriod loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Range.Should().Be.EqualTo(CreateAggregateWithCorrectBusinessUnit().Range);
		}

		protected override Repository<IPlanningPeriod> TestRepository(IUnitOfWork unitOfWork)
		{
			return new PlanningPeriodRepository(unitOfWork);
		}

		[Test]
		public void ShouldGetMostCommonlyUsedSchedulePeriods()
		{
			SetupPersonsInOrganizationWithContract(new []{SchedulePeriodType.Week});
			var repository = new PlanningPeriodRepository(UnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new TestableNow(new DateTime(2015, 4, 1)));
			var range = planningPeriodSuggestions.Default(true);
			range.StartDate.Should().Be.EqualTo(new DateOnly(2015, 04, 6));
			range.EndDate.Should().Be.EqualTo(new DateOnly(2015, 04, 12));
		}

		[Test]
		public void ShouldGetMonthAsDefaultWhenNoSchedulePeriods()
		{
			var repository = new PlanningPeriodRepository(UnitOfWork);
			var planningPeriodSuggestions = repository.Suggestions(new TestableNow(new DateTime(2015, 4, 1)));

			var defaultRange = planningPeriodSuggestions.Default(true);
			defaultRange.StartDate.Should().Be.EqualTo(new DateOnly(2015, 5, 1));
			defaultRange.EndDate.Should().Be.EqualTo(new DateOnly(2015, 5, 31));
		}

		private void SetupPersonsInOrganizationWithContract(IEnumerable<SchedulePeriodType> schedulePeriodTypes )
		{
			ITeam team = TeamFactory.CreateSimpleTeam("hola");
			ISite site = SiteFactory.CreateSimpleSite();
			site.AddTeam(team);
			IActivity act = new Activity("for test");
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			ISkill skill = new Skill("for test", "sdf", Color.Blue, 3, skType);
			ISkill skill2 = new Skill("for test2", "sdf", Color.Blue, 3, skType);
			skill.Activity = act;
			skill2.Activity = act;
			skill.TimeZone = (TimeZoneInfo.Local);
			skill2.TimeZone = (TimeZoneInfo.Local);

			PersonPeriod okPeriod = new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract2(), team);
			okPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			okPeriod.AddPersonSkill(new PersonSkill(skill2, new Percent(1)));
			PersonPeriod okPeriod2 = new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract2(), team);
			okPeriod2.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			okPeriod2.AddPersonSkill(new PersonSkill(skill2, new Percent(1)));
			PersonPeriod noPeriod2 = new PersonPeriod(new DateOnly(2002, 1, 1), createPersonContract2(), team);
			noPeriod2.AddPersonSkill(new PersonSkill(skill, new Percent(1)));

			PersistAndRemoveFromUnitOfWork(act);
			PersistAndRemoveFromUnitOfWork(skType);
			PersistAndRemoveFromUnitOfWork(skill);
			PersistAndRemoveFromUnitOfWork(skill2);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);

			createPersonAndSchedulePeriod(okPeriod, team, schedulePeriodTypes);
		}

		private void createPersonAndSchedulePeriod(PersonPeriod okPeriod, ITeam team, IEnumerable<SchedulePeriodType> schedulePeriodTypes)
		{
			var personIdToUpdate = new List<Guid>();
			schedulePeriodTypes.ForEach(x =>
			{
				SchedulePeriod schedulePeriod1 = new SchedulePeriod(okPeriod.StartDate, SchedulePeriodType.Month, 1); //2000-01-01
				SchedulePeriod schedulePeriod2 = new SchedulePeriod(new DateOnly(okPeriod.StartDate.Date.AddYears(1)),
					x, 1); //2001-01-01
				var sCategory = new ShiftCategory("for test");
				schedulePeriod1.AddShiftCategoryLimitation(new ShiftCategoryLimitation(sCategory));
				schedulePeriod2.AddShiftCategoryLimitation(new ShiftCategoryLimitation(sCategory));

				IPerson okPerson = PersonFactory.CreatePerson("hejhej");
				okPerson.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
				okPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), createPersonContract2(), team));
				okPerson.AddPersonPeriod(okPeriod);
				okPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(2001, 1, 1), createPersonContract2(), team));
				okPerson.AddSchedulePeriod(schedulePeriod1);
				okPerson.AddSchedulePeriod(schedulePeriod2);

				PersistAndRemoveFromUnitOfWork(sCategory);
				PersistAndRemoveFromUnitOfWork(okPerson);
				personIdToUpdate.Add(okPerson.Id.Value);
			});
			

			var groupingReadModel = new GroupingReadOnlyRepository(new FixedCurrentUnitOfWork(UnitOfWork));
			groupingReadModel.UpdateGroupingReadModel(personIdToUpdate);
		}

		private IPersonContract createPersonContract2(IBusinessUnit otherBusinessUnit = null)
		{
			var pContract = PersonContractFactory.CreatePersonContract();
			if (otherBusinessUnit != null)
			{
				pContract.Contract.SetBusinessUnit(otherBusinessUnit);
				pContract.ContractSchedule.SetBusinessUnit(otherBusinessUnit);
				pContract.PartTimePercentage.SetBusinessUnit(otherBusinessUnit);
			}
			PersistAndRemoveFromUnitOfWork(pContract.Contract);
			PersistAndRemoveFromUnitOfWork(pContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(pContract.PartTimePercentage);
			return pContract;
		}

		//private void SetupPersonsInOrganizationWithContract()
		//{
		//	IPerson okPerson = PersonFactory.CreatePerson("hejhej");
		//	okPerson.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
		//	ITeam team = TeamFactory.CreateSimpleTeam("hola");
		//	ISite site = SiteFactory.CreateSimpleSite();
		//	site.AddTeam(team);

		//	PersonPeriod okPeriod = new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract(), team);


		//	SchedulePeriod schedulePeriod1 = new SchedulePeriod(okPeriod.StartDate, SchedulePeriodType.Month, 1);
		//	SchedulePeriod schedulePeriod2 = new SchedulePeriod(new DateOnly(okPeriod.StartDate.Date.AddYears(1)), SchedulePeriodType.Week, 1);

		//	okPerson.AddPersonPeriod(okPeriod);
		//	okPerson.AddSchedulePeriod(schedulePeriod1);
		//	okPerson.AddSchedulePeriod(schedulePeriod2);

		//	PersistAndRemoveFromUnitOfWork(site);
		//	PersistAndRemoveFromUnitOfWork(team);
		//	PersistAndRemoveFromUnitOfWork(okPerson);
		//	var groupingReadModel = new GroupingReadOnlyRepository(new FixedCurrentUnitOfWork(UnitOfWork));
		//	groupingReadModel.UpdateGroupingReadModel(new List<Guid>{okPeriod.Id.Value });
		//}

		//private void SetupPersonsInOrganizationWithContract2()
		//{
		//	IPerson okPerson = PersonFactory.CreatePerson("hejhej");
		//	okPerson.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
		//	IPerson okPerson2 = PersonFactory.CreatePerson("hejhej2");
		//	IPerson noPerson1 = PersonFactory.CreatePerson("bajbaj");
		//	IPerson noPerson2 = PersonFactory.CreatePerson("bajbaj");
		//	IPerson noPerson3 = PersonFactory.CreatePersonWithBasicPermissionInfo("dra på ", "trissor");
		//	ITeam team = TeamFactory.CreateSimpleTeam("hola");
		//	ISite site = SiteFactory.CreateSimpleSite();
		//	site.AddTeam(team);
		//	IActivity act = new Activity("for test");
		//	ISkillType skType = SkillTypeFactory.CreateSkillType();
		//	ISkill skill = new Skill("for test", "sdf", Color.Blue, 3, skType);
		//	ISkill skill2 = new Skill("for test2", "sdf", Color.Blue, 3, skType);
		//	skill.Activity = act;
		//	skill2.Activity = act;
		//	skill.TimeZone = (TimeZoneInfo.Local);
		//	skill2.TimeZone = (TimeZoneInfo.Local);

		//	PersonPeriod okPeriod = new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract2(), team);
		//	okPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
		//	okPeriod.AddPersonSkill(new PersonSkill(skill2, new Percent(1)));
		//	PersonPeriod okPeriod2 = new PersonPeriod(new DateOnly(2000, 1, 1), createPersonContract2(), team);
		//	okPeriod2.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
		//	okPeriod2.AddPersonSkill(new PersonSkill(skill2, new Percent(1)));
		//	PersonPeriod noPeriod2 = new PersonPeriod(new DateOnly(2002, 1, 1), createPersonContract2(), team);
		//	noPeriod2.AddPersonSkill(new PersonSkill(skill, new Percent(1)));

		//	SchedulePeriod schedulePeriod1 = new SchedulePeriod(okPeriod.StartDate, SchedulePeriodType.Month, 1);
		//	SchedulePeriod schedulePeriod2 = new SchedulePeriod(new DateOnly(okPeriod.StartDate.Date.AddYears(1)), SchedulePeriodType.Month, 1);
		//	var sCategory = new ShiftCategory("for test");
		//	schedulePeriod1.AddShiftCategoryLimitation(new ShiftCategoryLimitation(sCategory));
		//	schedulePeriod2.AddShiftCategoryLimitation(new ShiftCategoryLimitation(sCategory));

		//	okPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), createPersonContract2(), team));
		//	okPerson.AddPersonPeriod(okPeriod);
		//	okPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(2001, 1, 1), createPersonContract2(), team));
		//	okPerson.AddSchedulePeriod(schedulePeriod1);
		//	okPerson.AddSchedulePeriod(schedulePeriod2);
		//	okPerson2.AddPersonPeriod(okPeriod2);

		//	noPerson2.AddPersonPeriod(noPeriod2);

		//	PersistAndRemoveFromUnitOfWork(sCategory);
		//	PersistAndRemoveFromUnitOfWork(act);
		//	PersistAndRemoveFromUnitOfWork(skType);
		//	PersistAndRemoveFromUnitOfWork(skill);
		//	PersistAndRemoveFromUnitOfWork(skill2);
		//	PersistAndRemoveFromUnitOfWork(site);
		//	PersistAndRemoveFromUnitOfWork(team);
		//	PersistAndRemoveFromUnitOfWork(noPerson3);
		//	PersistAndRemoveFromUnitOfWork(noPerson1);
		//	PersistAndRemoveFromUnitOfWork(noPerson2);
		//	PersistAndRemoveFromUnitOfWork(okPerson);
		//	PersistAndRemoveFromUnitOfWork(okPerson2);

		//	var groupingReadModel = new GroupingReadOnlyRepository(new FixedCurrentUnitOfWork(UnitOfWork));
		//	groupingReadModel.UpdateGroupingReadModel(new List<Guid> { okPerson.Id.Value, okPerson2.Id.Value, noPerson1.Id.Value, noPerson2.Id.Value, noPerson3.Id.Value });
		//	groupingReadModel.UpdateGroupingReadModelData(new List<Guid> { okPerson.Id.Value, okPerson2.Id.Value, noPerson1.Id.Value, noPerson2.Id.Value, noPerson3.Id.Value });
		//}

		//private IPersonContract createPersonContract()
		//{
		//	var pContract = PersonContractFactory.CreatePersonContract();
		//	PersistAndRemoveFromUnitOfWork(pContract.Contract);
		//	PersistAndRemoveFromUnitOfWork(pContract.ContractSchedule);
		//	PersistAndRemoveFromUnitOfWork(pContract.PartTimePercentage);
		//	return pContract;
		//}
	}
}
