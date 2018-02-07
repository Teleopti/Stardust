using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Scheduling
{
	[DatabaseTest]
	[UseIocForFatClient]
	public class NoGroupPagePersistDesktopTest
	{
		public IGroupPageRepository GroupPageRepository;
		public IPersonRepository PersonRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public IContractRepository ContractRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;


		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public DesktopScheduling Target;

		[Test]
		[Ignore("48053 - to be fixed")]
		public void ShouldNotRemoveAgentFromUserDefinedGroupPage()
		{
			var date = new DateOnly(2016, 10, 24);
			var period = new DateOnlyPeriod(date, date);
			var contract = new Contract("_");
			var site = new Site("_");
			var team = new Team(){Site = site};
			team.SetDescription(new Description("_"));
			var partTimePercentage = new PartTimePercentage("_");
			var contractSchedule = new ContractSchedule("_");
			var skillType = new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony);
			var activity = new Activity("_");
			var skill1 = new Skill("_") {SkillType = skillType, Activity = activity}.InTimeZone(TimeZoneInfo.Utc);
			var skill2 = new Skill("_"){SkillType = skillType, Activity = activity}.InTimeZone(TimeZoneInfo.Utc);
			var personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
			var personPeriod1 = new PersonPeriod(date, personContract, team);
			var personPeriod2 = new PersonPeriod(date, personContract, team);
			personPeriod1.AddPersonSkill(new PersonSkill(skill1, new Percent()));
			personPeriod2.AddPersonSkill(new PersonSkill(skill2, new Percent()));
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent1.AddPersonPeriod(personPeriod1);
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPersonPeriod(personPeriod2);
			var rootPersonGroup1 = new RootPersonGroup("_");
			var rootPersonGroup2 = new RootPersonGroup("_");
			rootPersonGroup1.AddPerson(agent1);
			rootPersonGroup2.AddPerson(agent2);
			var groupPage1 = new GroupPage("_"); 
			var groupPage2 = new GroupPage("_");
			groupPage1.AddRootPersonGroup(rootPersonGroup1);
			groupPage2.AddRootPersonGroup(rootPersonGroup2);

			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skillType);
				SkillRepository.Add(skill1);
				SkillRepository.Add(skill2);
				ContractRepository.Add(contract);
				ContractScheduleRepository.Add(contractSchedule);
				PartTimePercentageRepository.Add(partTimePercentage);
				SiteRepository.Add(site);
				TeamRepository.Add(team);
				PersonRepository.Add(agent1);
				PersonRepository.Add(agent2);
				GroupPageRepository.Add(groupPage1);
				GroupPageRepository.Add(groupPage2);
				uow.PersistAll();
			}

			var schedulingOptions = new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.UserDefined, groupPage1.IdOrDescriptionKey),
				UseTeam = true
			};
			SchedulerStateHolderFrom.Fill(new Scenario(), period, new[] {agent1, agent2}, Enumerable.Empty<IPersonAssignment>(), Enumerable.Empty<ISkillDay>());

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent1}, period);

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				GroupPageRepository.GetGroupPagesForPerson(agent1.Id.Value).Should().Not.Be.Empty();
				GroupPageRepository.GetGroupPagesForPerson(agent2.Id.Value).Should().Not.Be.Empty();
			}
		}
	}
}
