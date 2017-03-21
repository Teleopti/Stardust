using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.People.Core;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture]
	[DomainTest]
	public class AgentPersisterTest :ISetup
	{
		public AgentPersister Target;

		public FakePersonRepository PersonRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public PersistPersonInfoFake TenantUserRepository;

		public void Setup(ISystem system,IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<PersonInfoMapperFake>().For<IPersonInfoMapper>();
			system.UseTestDouble<PersistPersonInfoFake>().For<IPersistPersonInfo>();
			system.UseTestDouble<TenantUserPersister>().For<ITenantUserPersister>();
			system.AddService<AgentPersister>();
		}

		[Test]
		public void CanResolveTarget()
		{
			Target.Should().Not.Be.Null();
		}
	
		[Test]
		public void ShouldPersistValidPersonDataAndTenantData()
		{
			var agentData = getAgentDataModel();

			Target.Persist(new List<AgentExtractionResult> { new AgentExtractionResult { Agent = agentData } });

			var persistedUser = PersonRepository.LoadAll().Single();
			var persistedTenant = TenantUserRepository.LastPersist;

			persistedUser.Name.FirstName.Should().Be(agentData.Firstname);
			persistedUser.Name.LastName.Should().Be(agentData.Lastname);

			persistedTenant.Identity.Should().Be(agentData.WindowsUser);
			persistedTenant.ApplicationLogonInfo.LogonName.Should().Be.EqualTo(agentData.ApplicationUserId);
			persistedTenant.ApplicationLogonInfo.LogonPassword.Should().Not.Be.NullOrEmpty();

			var persistedPersonPeriod = persistedUser.PersonPeriodCollection.Single();

			persistedPersonPeriod.PersonSkillCollection.Single().Skill.Should().Be.EqualTo(agentData.Skills.Single());
			persistedPersonPeriod.ExternalLogOnCollection.Single().Should().Be(agentData.ExternalLogons.Single());
			persistedPersonPeriod.PersonContract.Contract.Should().Be(agentData.Contract);
			persistedPersonPeriod.PersonContract.ContractSchedule.Should().Be(agentData.ContractSchedule);
			persistedPersonPeriod.PersonContract.PartTimePercentage.Should().Be(agentData.PartTimePercentage);
			persistedPersonPeriod.RuleSetBag.Should().Be(agentData.RuleSetBag);

			var persistedSchedulePeriod = persistedUser.PersonSchedulePeriodCollection.Single();

			persistedSchedulePeriod.PeriodType.Should().Be(agentData.SchedulePeriodType);
			persistedSchedulePeriod.Number.Should().Be.EqualTo(agentData.SchedulePeriodLength);
		}

		[Test]
		public void ShouldNotPersitUserAndTenantWithDuplicateLogon()
		{
			var agentData = getAgentDataModel();
			
			agentData.ApplicationUserId = "existingId@teleopti.com";
			var holder = new AgentExtractionResult {Agent = agentData};
			Target.Persist(new List<AgentExtractionResult> { holder });
			PersonRepository.LoadAll().Should().Be.Empty();
			holder.Feedback.ErrorMessages.Single().Should().Be(Resources.DuplicatedApplicationLogonErrorMsgSemicolon);
		}

		[Test]
		public void ShouldSkipDataWithErrorAtTheStart()
		{
			var agentData = getAgentDataModel();
			var holder = new AgentExtractionResult { Agent = agentData};
			holder.Feedback.ErrorMessages.Add("existing data error");
			Target.Persist(new List<AgentExtractionResult> { holder });
			PersonRepository.LoadAll().Should().Be.Empty();
		}

		private AgentDataModel getAgentDataModel()
		{
			var agent = new AgentDataModel
			{
				Firstname = "a",
				Lastname = "b",
				WindowsUser = "abc",
				ApplicationUserId = "abc",
				Password = "password",
				StartDate = new DateOnly(2017,1,1),
				Team = TeamFactory.CreateSimpleTeam(),
				Contract = ContractFactory.CreateContract("contract"),
				ContractSchedule = ContractScheduleFactory.CreateContractSchedule("contractSchedule"),
				PartTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("parttime percentage"),
				RuleSetBag = new RuleSetBag(),
				SchedulePeriodType = SchedulePeriodType.Week,
				SchedulePeriodLength = 4
			};

			agent.Roles.Add(ApplicationRoleFactory.CreateRole("role", "a role"));
			agent.Skills.Add(SkillFactory.CreateSkill("skill"));
			agent.ExternalLogons.Add(ExternalLogOnFactory.CreateExternalLogOn());
			return agent;
		}

	}
}
