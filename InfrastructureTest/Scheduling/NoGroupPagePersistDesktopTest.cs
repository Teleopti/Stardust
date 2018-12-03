using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
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
		public void ShouldNotRemoveAgentFromUserDefinedGroupPage()
		{
			var date = new DateOnly(2016, 10, 24);
			var period = new DateOnlyPeriod(date, date);
			var skillType = new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony);
			var activity = new Activity("_");
			var skill1 = new Skill("_") {SkillType = skillType, Activity = activity}.InTimeZone(TimeZoneInfo.Utc);
			var skill2 = new Skill("_"){SkillType = skillType, Activity = activity}.InTimeZone(TimeZoneInfo.Utc);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill1);
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill2);
			var rootPersonGroup1 = new RootPersonGroup("_");
			var rootPersonGroup2 = new RootPersonGroup("_");
			rootPersonGroup1.AddPerson(agent1);
			rootPersonGroup2.AddPerson(agent2);
			var groupPage1 = new GroupPage("_"); 
			var groupPage2 = new GroupPage("_");
			groupPage1.AddRootPersonGroup(rootPersonGroup1);
			groupPage2.AddRootPersonGroup(rootPersonGroup2);
			var agents = new[] {agent1, agent2};
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skillType);
				SkillRepository.Add(skill1);
				SkillRepository.Add(skill2);
				foreach (var agent in agents)
				{
					var personPeriod = agent.Period(date);
					ContractRepository.Add(personPeriod.PersonContract.Contract);
					ContractScheduleRepository.Add(personPeriod.PersonContract.ContractSchedule);
					PartTimePercentageRepository.Add(personPeriod.PersonContract.PartTimePercentage);
					SiteRepository.Add(personPeriod.Team.Site);
					TeamRepository.Add(personPeriod.Team);
					PersonRepository.Add(agent);
				}
				GroupPageRepository.Add(groupPage1);
				GroupPageRepository.Add(groupPage2);
				uow.PersistAll();
			}
			var schedulingOptions = new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.UserDefined, groupPage1.IdOrDescriptionKey),
				UseTeam = true
			};
			SchedulerStateHolderFrom.Fill(new Scenario(), period, agents, Enumerable.Empty<IPersonAssignment>(), Enumerable.Empty<ISkillDay>());

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent1}, period);

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				GroupPageRepository.GetGroupPagesForPerson(agent1.Id.Value).Should().Not.Be.Empty();
				GroupPageRepository.GetGroupPagesForPerson(agent2.Id.Value).Should().Not.Be.Empty();
			}
		}
	}
}
