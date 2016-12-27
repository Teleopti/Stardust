using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.ViewModelFactory
{
	[TestFixture, RequestsTest]
	public class RequestAllowanceViewModelFactoryTest : ISetup
	{
		public IBudgetGroupRepository BudgetGroupRepository;
		public IRequestAllowanceViewModelFactory RequestAllowanceViewModelFactory;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeBudgetDayRepository>().For<IBudgetDayRepository>();
			system.UseTestDouble<FakeBudgetGroupRepository>().For<IBudgetGroupRepository>();
			system.UseTestDouble<FakeScheduleProjectionReadOnlyPersister>().For<IScheduleProjectionReadOnlyPersister>();
			system.UseTestDouble<RequestAllowanceProvider>().For<IRequestAllowanceProvider>();
		}

		[Test]
		public void ShouldGetAllBudgetGroups()
		{
			var budgetGroup1 = new BudgetGroup {Name = "bg2"}.WithId();
			var budgetGroup2 = new BudgetGroup {Name = "bg1"}.WithId();
			BudgetGroupRepository.Add(budgetGroup1);
			BudgetGroupRepository.Add(budgetGroup2);
			var budgetGroups = RequestAllowanceViewModelFactory.CreateBudgetGroupViewModels();
			budgetGroups.Count.Should().Be(2);
			budgetGroups.Any(b => b.Id.Equals(budgetGroup1.Id)).Should().Be(true);
			budgetGroups.Any(b => b.Id.Equals(budgetGroup2.Id)).Should().Be(true);
		}

		[Test]
		public void ShouldGetAllBudgetGroupsOrderByName()
		{
			var budgetGroup1 = new BudgetGroup { Name = "bg2" }.WithId();
			var budgetGroup2 = new BudgetGroup { Name = "bg1" }.WithId();
			BudgetGroupRepository.Add(budgetGroup1);
			BudgetGroupRepository.Add(budgetGroup2);
			var budgetGroups = RequestAllowanceViewModelFactory.CreateBudgetGroupViewModels();
			budgetGroups.Count.Should().Be(2);
			budgetGroups[0].Name.Should(budgetGroup2.Name);
			budgetGroups[1].Name.Should(budgetGroup1.Name);
		}
	}
}
