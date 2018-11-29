using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportAgent
{
	[TestFixture]
	[DomainTest]
	public class AgentPersisterTest :IIsolateSystem, IExtendSystem
	{
		public AgentPersister Target;

		public FakePersonRepository PersonRepository;
		public PersistPersonInfoFake TenantUserRepository;
		public FakeCurrentDatasource CurrentDatasource;
		public FakeTenants FindTenantByName;
		public ILoggedOnUser User;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AgentPersister>();
		}
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PersistPersonInfoFake>().For<IPersistPersonInfo>();
			isolate.UseTestDouble<TenantUserPersister>().For<ITenantUserPersister>();
			isolate.UseTestDouble<PersonInfoHelper>().For<IPersonInfoHelper>();
			isolate.UseTestDouble<FakeCurrentDatasource>().For<ICurrentDataSource>();
			isolate.UseTestDouble<CheckPasswordStrengthFake>().For<ICheckPasswordStrength>();
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

			Target.Persist(new List<AgentExtractionResult> { new AgentExtractionResult { Agent = agentData } }, TimeZoneInfo.Utc);

			var persistedUser = PersonRepository.LoadAll().Last();
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
			Target.Persist(new List<AgentExtractionResult> { holder }, TimeZoneInfo.Utc);
			PersonRepository.LoadAll()
				.Where(x => x != User.CurrentUser())
				.Should().Be.Empty();
			holder.Feedback.ErrorMessages.Single().Should().Be(Resources.DuplicatedApplicationLogonErrorMsgSemicolon);
		}

		[Test]
		public void ShouldSkipDataWithErrorAtTheStart()
		{
			var agentData = getAgentDataModel();
			var holder = new AgentExtractionResult { Agent = agentData};
			holder.Feedback.ErrorMessages.Add("existing data error");
			Target.Persist(new List<AgentExtractionResult> { holder }, TimeZoneInfo.Utc);
			PersonRepository.LoadAll()
				.Where(x => x != User.CurrentUser())
				.Should().Be.Empty();
		}


		[Test]
		public void ShouldRemovePersistedPersonAndTenantAfterRollback()
		{
			var agentData = getAgentDataModel();
			var holder = new AgentExtractionResult { Agent = agentData };
			var persisted = Target.Persist(new List<AgentExtractionResult> { holder }, TimeZoneInfo.Utc);
			PersonRepository.LoadAll()
				.Where(x => x != User.CurrentUser())
				.Count().Should().Be(1);
			Target.Rollback(persisted);

			PersonRepository.LoadAll()
				.Where(x => x != User.CurrentUser())
				.Should().Be.Empty();
			TenantUserRepository.RollBacked.Should().Be.True();

		}

		private AgentDataModel getAgentDataModel()
		{
			CurrentDatasource.FakeName("test");
			FindTenantByName.Has(new Tenant("test"));
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
