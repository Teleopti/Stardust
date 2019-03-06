using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Intraday;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.InfrastructureTest.Intraday
{
	[TestFixture]
	[UnitOfWorkTest]
	public class PersonProviderForOvertimeTest : IIsolateSystem
	{
		public IPersonRepository PersonRepository;
		public ISkillRepository SkillRepository;
		public IPersonScheduleDayReadModelPersister PersonScheduleDayReadModelPersister;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IPersonForOvertimeProvider Target;
		public ISkillTypeRepository SkillTypeRepository;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IActivityRepository ActivityRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		private BusinessUnit _businessUnit;
		private SkillType _skillType;
		private Activity _activity;
		private Team _team;
		private IMultiplicatorDefinitionSet _multi;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PersonForOvertimeProvider>().For<IPersonForOvertimeProvider>();
		}

		[Test]
		public void ShouldReturnPersonHavingProvidedSkillAndPeriod()
		{
			var date = new DateOnly(2017, 02, 13);
			setUpBaselineData();
			var skill = setupSkill("direct");
			var personPeriod = setupPersonPeriod(date, new ISkill[] { skill});

			var person = PersonFactory.CreatePerson("asad");
			PersonRepository.Add(person);
			person.AddPersonPeriod(personPeriod );
			person.PersonPeriodCollection.First().Team = _team;

			CurrentUnitOfWork.Current().PersistAll();
			var model = new PersonScheduleDayReadModel
			{
				Date = date.Date,
				PersonId = person.Id.GetValueOrDefault(),
				Start = date.Date.AddHours(10),
				End = date.Date.AddHours(18),
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = DateTime.UtcNow,
				Version = 1
			};

			PersonScheduleDayReadModelPersister.UpdateReadModels(new DateOnlyPeriod(date, date), person.Id.GetValueOrDefault(), skill.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault(),
				new[] { model }, false);

			var persons = Target.Persons(new List<Guid>() { skill.Id.GetValueOrDefault() }, date.Date.AddHours(15), date.Date.AddHours(20), _multi.Id.GetValueOrDefault(), 1000);
			persons.Count.Should().Be.EqualTo(1);
			persons.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			persons.First().End.Should().Be.EqualTo(date.Date.AddHours(18));
			persons.First().TimeToAdd.Should().Be.EqualTo(120);

		}

		[Test]
		public void ShouldNotReturnIfOutsidePeriod()
		{
			var date = new DateOnly(2017, 02, 13);
			setUpBaselineData();
			var skill = setupSkill("direct");
			var personPeriod = setupPersonPeriod(date, new ISkill[] { skill });

			var person = PersonFactory.CreatePerson("asad");
			PersonRepository.Add(person);
			person.AddPersonPeriod(personPeriod);
			person.PersonPeriodCollection.First().Team = _team;

			CurrentUnitOfWork.Current().PersistAll();
			var model = new PersonScheduleDayReadModel
			{
				Date = date.Date,
				PersonId = person.Id.GetValueOrDefault(),
				Start = date.Date.AddHours(10),
				End = date.Date.AddHours(16),
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = DateTime.UtcNow,
				Version = 1
			};

			PersonScheduleDayReadModelPersister.UpdateReadModels(new DateOnlyPeriod(date, date), person.Id.GetValueOrDefault(), skill.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault(),
				new[] { model }, false);

			var persons = Target.Persons(new List<Guid>() { skill.Id.GetValueOrDefault() }, date.Date.AddHours(17), date.Date.AddHours(20), Guid.NewGuid(), 1000);

			persons.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotReturnIfSkillNotFound()
		{
			var date = new DateOnly(2017, 02, 13);
			setUpBaselineData();
			var skill = setupSkill("direct");
			var skillMissing = setupSkill("missingDirect");
			var personPeriod = setupPersonPeriod(date, new ISkill[] { skill });

			var person = PersonFactory.CreatePerson("asad");
			PersonRepository.Add(person);
			person.AddPersonPeriod(personPeriod);
			person.PersonPeriodCollection.First().Team = _team;

			CurrentUnitOfWork.Current().PersistAll();
			var model = new PersonScheduleDayReadModel
			{
				Date = date.Date,
				PersonId = person.Id.GetValueOrDefault(),
				Start = date.Date.AddHours(10),
				End = date.Date.AddHours(18),
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = DateTime.UtcNow,
				Version = 1
			};

			PersonScheduleDayReadModelPersister.UpdateReadModels(new DateOnlyPeriod(date, date), person.Id.GetValueOrDefault(), skill.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault(),
				new[] { model }, false);

			var persons = Target.Persons(new List<Guid>() { skillMissing.Id.GetValueOrDefault() }, date.Date.AddHours(15), date.Date.AddHours(20), Guid.NewGuid(), 1000);

			persons.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnIfPersonHasAtleastOneSkill()
		{
			var date = new DateOnly(2017, 02, 13);
			setUpBaselineData();
			var skill = setupSkill("direct");
			var skill2 = setupSkill("skill2");
			var personPeriod = setupPersonPeriod(date, new ISkill[] { skill,skill2 });

			var person = PersonFactory.CreatePerson("asad");
			PersonRepository.Add(person);
			person.AddPersonPeriod(personPeriod);
			person.PersonPeriodCollection.First().Team = _team;

			CurrentUnitOfWork.Current().PersistAll();
			var model = new PersonScheduleDayReadModel
			{
				Date = date.Date,
				PersonId = person.Id.GetValueOrDefault(),
				Start = date.Date.AddHours(10),
				End = date.Date.AddHours(18),
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = DateTime.UtcNow,
				Version = 1
			};

			PersonScheduleDayReadModelPersister.UpdateReadModels(new DateOnlyPeriod(date, date), person.Id.GetValueOrDefault(), skill.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault(),
				new[] { model }, false);

			var persons = Target.Persons(new List<Guid>() { skill2.Id.GetValueOrDefault() }, date.Date.AddHours(15), date.Date.AddHours(20),_multi.Id.GetValueOrDefault(), 1000);

			persons.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotReturnIfNoPersonPeriodFound()
		{
			var date = new DateOnly(2017, 02, 13);
			setUpBaselineData();
			var skill = setupSkill("direct");
			var personPeriod = setupPersonPeriod(date.AddDays(2), new ISkill[] { skill });

			var person = PersonFactory.CreatePerson("asad");
			PersonRepository.Add(person);
			person.AddPersonPeriod(personPeriod);
			person.PersonPeriodCollection.First().Team = _team;

			CurrentUnitOfWork.Current().PersistAll();
			var model = new PersonScheduleDayReadModel
			{
				Date = date.Date,
				PersonId = person.Id.GetValueOrDefault(),
				Start = date.Date.AddHours(10),
				End = date.Date.AddHours(18),
				Model = "{shift: blablabla}",
				ScheduleLoadTimestamp = DateTime.UtcNow,
				Version = 1
			};

			PersonScheduleDayReadModelPersister.UpdateReadModels(new DateOnlyPeriod(date, date), person.Id.GetValueOrDefault(), skill.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault(),
				new[] { model }, false);

			var persons = Target.Persons(new List<Guid>() { skill.Id.GetValueOrDefault() }, date.Date.AddHours(15), date.Date.AddHours(20), Guid.NewGuid(), 1000);

			persons.Count.Should().Be.EqualTo(0);
		}

		private void setUpBaselineData()
		{
			_businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("1");
			BusinessUnitRepository.Add(_businessUnit);

			_skillType = SkillTypeFactory.CreateSkillTypePhone();
			SkillTypeRepository.Add(_skillType);

			_activity = new Activity("activty");
			_activity.SetBusinessUnit(_businessUnit);
			ActivityRepository.Add(_activity);

			var site = SiteFactory.CreateSimpleSite("site");
			site.SetBusinessUnit(_businessUnit);
			SiteRepository.Add(site);

			_team = TeamFactory.CreateTeam("team", "site");
			_team.Site = site;
			TeamRepository.Add(_team);

			_multi =
				MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("Overdue", MultiplicatorType.Overtime);
			MultiplicatorDefinitionSetRepository.Add(_multi);
		}

		private IPersonPeriod setupPersonPeriod(DateOnly date, ISkill[] skills)
		{
			var personPeriod = PersonPeriodFactory.CreatePersonPeriodWithSkills(date.AddDays(-1), skills);
			personPeriod.PersonContract.PartTimePercentage.SetBusinessUnit(_businessUnit);
			PartTimePercentageRepository.Add(personPeriod.PersonContract.PartTimePercentage);
			personPeriod.PersonContract.Contract.SetBusinessUnit(_businessUnit);
			ContractRepository.Add(personPeriod.PersonContract.Contract);
			personPeriod.PersonContract.ContractSchedule.SetBusinessUnit(_businessUnit);
			personPeriod.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(_multi);
			ContractScheduleRepository.Add(personPeriod.PersonContract.ContractSchedule);
			return personPeriod;
		}

		private Skill setupSkill(string name)
		{
			var skill = new Skill(name, "asdf", Color.AliceBlue, 15, _skillType);
			skill.Activity = _activity;
			skill.SetBusinessUnit(_businessUnit);
			skill.TimeZone = TimeZoneInfo.Utc;
			SkillRepository.Add(skill);
			return skill;
		}
	}

	
}
