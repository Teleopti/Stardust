using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class AgentBadgeProviderTest
	{
		private IAgentBadgeRepository agentBadgeRepository;
		private IPersonRepository personRepository;
		private IBadgeSettingProvider settingProvider;
		private AgentBadgeProvider target;
		private IPermissionProvider permissionProvider;
		private IPersonNameProvider personNameProvider;

		[SetUp]
		public void SetUp()
		{
			agentBadgeRepository = MockRepository.GenerateMock<IAgentBadgeRepository>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			settingProvider = MockRepository.GenerateMock<IBadgeSettingProvider>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			target = new AgentBadgeProvider(agentBadgeRepository, permissionProvider, personRepository, settingProvider,
				personNameProvider);
		}



		[Test]
		public void ShouldQueryAllAgents()
		{
			personRepository.Stub(x => x.FindPeople(new Guid[] {Guid.NewGuid()}))
				.IgnoreArguments()
				.Return(new IPerson[] {new Person()});
			target.GetPermittedAgents(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard);

			agentBadgeRepository.AssertWasCalled(x => x.GetAllAgentBadges());
		}

		[Test]
		public void ShouldFilterPermittedAgentsWhenQueryingAll()
		{
			
			var agents = new IAgentBadge[] {new AgentBadge(), new AgentBadge()};
			var person0 = new Person();
			person0.Name = new Name("first1", "last1");
			var person1 = new Person();
			person1.Name = new Name("first2", "last2");
			var persons = new IPerson[] { person0, person1 };
			var personName0 = "first1 last1";
			var personName1 = "first2 last2";
			agentBadgeRepository.Stub(x => x.GetAllAgentBadges()).Return(agents);
			personRepository.Stub(x => x.FindPeople(agents.Select(item => item.Person).ToList())).IgnoreArguments().Return(persons);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today, person0)).Return(false);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today, person1)).Return(true);
			personNameProvider.Stub(x => x.BuildNameFromSetting(person0.Name)).Return(personName0);
			personNameProvider.Stub(x => x.BuildNameFromSetting(person1.Name)).Return(personName1);
			
			var result = target.GetPermittedAgents(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard);

			result.Single().AgentName.Should().Be(personName1);
		}

		[Test]
		public void ShouldReturnCorrectTotalBadgeNumber()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var persons= new IPerson[]{person};
			var agents = new IAgentBadge[]
			{
				new AgentBadge
				{
					BadgeType = BadgeType.Adherence,
					TotalAmount = 16,
					Person = (Guid) person.Id
				}, 

				new AgentBadge
				{
					BadgeType = BadgeType.AnsweredCalls,
					TotalAmount = 25,
					Person = (Guid) person.Id
				},

				new AgentBadge
				{
					BadgeType = BadgeType.AverageHandlingTime,
					TotalAmount = 32,
					Person = (Guid) person.Id
				}
			};
			var setting = new AgentBadgeSettings()
			{
				BadgeEnabled = true,
				GoldToSilverBadgeRate = 2,
				SilverToBronzeBadgeRate = 5
			};
			agentBadgeRepository.Stub(x=>x.GetAllAgentBadges()).Return(agents);
			personRepository.Stub(x => x.FindPeople(agents.Select(item => item.Person).ToList())).IgnoreArguments().Return(persons);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today, persons.ElementAt(0))).Return(true);
			settingProvider.Stub(x => x.GetBadgeSettings()).Return(setting);
			var result = target.GetPermittedAgents(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard).ToArray();
			result.Single().Gold.Should().Be(6);
			result.Single().Silver.Should().Be(2);
			result.Single().Bronze.Should().Be(3);

		}

	}
}