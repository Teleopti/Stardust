using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[TestFixture]
	[InfrastructureTest]
	[Category("BucketB")]
	public class Bug42845
	{
		public IScenarioRepository ScenarioRepository;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;
		public IPersonRepository PersonRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public IContractRepository ContractRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractScheduleRepository ContractScheduleRepository;

		[Test, Ignore("Really strange nhibernate/domain bug for Kratzenhytten to look into.")]
		public void ShouldNotCreateDuplicatesOfPersonSkill()
		{
			var activity = new Activity("act");
			var skillType = SkillTypeFactory.CreateSkillType();
			var skill = new Skill("skill", "skill", Color.Blue, 15, skillType)
			{
				Activity = activity,
				TimeZone = TimeZoneInfo.Utc
			};

			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");


			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent()));
			person.AddPersonPeriod(personPeriod);

			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SkillTypeRepository.Add(skillType);
				ActivityRepository.Add(activity);
				SkillRepository.Add(skill);
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
				PersonRepository.Add(person);
				setup.PersistAll();
			}

			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var persistedPerson = PersonRepository.FindPeople(new []{ person.Id.Value }).FirstOrDefault();
				var pp = persistedPerson.PersonPeriodCollection.First();
				var persistedSkill = SkillRepository.FindAllWithoutMultisiteSkills().FirstOrDefault();

				persistedPerson.ResetPersonSkills(pp);
				persistedPerson.AddSkill(new PersonSkill(persistedSkill, new Percent(1)) {Active = true}, pp);
				setup.PersistAll();
			}

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				PersonRepository.Get(person.Id.Value)
								.PersonPeriodCollection.First()
								.PersonSkillCollection.Count()
								.Should()
								.Be.EqualTo(1);
			}
		}
	}
}
