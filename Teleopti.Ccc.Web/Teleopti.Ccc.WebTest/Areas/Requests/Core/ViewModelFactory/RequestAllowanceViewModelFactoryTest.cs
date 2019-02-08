using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.ViewModelFactory
{
	[SetCulture("en-US")]
	[DomainTest] 
	[WebTest] 
	[RequestsTest] 
	public class RequestAllowanceViewModelFactoryTest : IIsolateSystem
	{
		public IBudgetGroupRepository BudgetGroupRepository;
		public IRequestAllowanceViewModelFactory RequestAllowanceViewModelFactory;
		public ICurrentScenario CurrentScenario;
		public IBudgetDayRepository BudgetDayRepository;
		public IScheduleProjectionReadOnlyPersister ScheduleProjectionReadOnlyPersister;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeBudgetDayRepository>().For<IBudgetDayRepository>();
			isolate.UseTestDouble<FakeBudgetGroupRepository>().For<IBudgetGroupRepository>();
			isolate.UseTestDouble<FakeScheduleProjectionReadOnlyPersister>().For<IScheduleProjectionReadOnlyPersister>();
			isolate.UseTestDouble<RequestAllowanceProvider>().For<IRequestAllowanceProvider>();
		}

		[Test]
		public void ShouldGetAllBudgetGroups()
		{
			var budgetGroup1 = createBudgetGroup("bg1");
			var budgetGroup2 = createBudgetGroup("bg2");
			var budgetGroups = RequestAllowanceViewModelFactory.CreateBudgetGroupViewModels();
			budgetGroups.Count.Should().Be(2);
			budgetGroups.Any(b => b.Id.Equals(budgetGroup1.Id)).Should().Be(true);
			budgetGroups.Any(b => b.Id.Equals(budgetGroup2.Id)).Should().Be(true);
		}

		[Test]
		public void ShouldGetAllBudgetGroupsOrderByName()
		{
			var budgetGroup1 = createBudgetGroup("bg2");
			var budgetGroup2 = createBudgetGroup("bg1");
			var budgetGroups = RequestAllowanceViewModelFactory.CreateBudgetGroupViewModels();
			budgetGroups.Count.Should().Be(2);
			budgetGroups[0].Name.Should(budgetGroup2.Name);
			budgetGroups[1].Name.Should(budgetGroup1.Name);
		}

		[Test]
		public void ShouldGetAllowanceDetailsInWeek()
		{
			var allowanceDetailViewModels = RequestAllowanceViewModelFactory.CreateBudgetAbsenceAllowanceDetailViewModels(new DateOnly(2016, 12, 28), null);
			allowanceDetailViewModels.Count.Should().Be(7);
			allowanceDetailViewModels.First().Date.Should().Be(new DateOnly(2016, 12, 25));
			allowanceDetailViewModels.Last().Date.Should().Be(new DateOnly(2016, 12, 31));
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldGetWeekendDateByCulture()
		{
			var allowanceDetailViewModels = RequestAllowanceViewModelFactory.CreateBudgetAbsenceAllowanceDetailViewModels(new DateOnly(2016, 12, 28), null);
			allowanceDetailViewModels.Count.Should().Be(7);
			allowanceDetailViewModels[5].IsWeekend.Should().Be(true);
			allowanceDetailViewModels[5].Date.Should().Be(new DateOnly(2016, 12, 31));
			allowanceDetailViewModels[6].IsWeekend.Should().Be(true);
			allowanceDetailViewModels[6].Date.Should().Be(new DateOnly(2017, 1, 1));
		}

		[Test]
		public void ShouldGetAllowanceDetails()
		{
			var budgetGroup1 = createBudgetGroup("bg1");
			createBudgetDay(budgetGroup1, new DateOnly(2016, 12, 28), 1, 2);

			var allowanceDetailViewModels = RequestAllowanceViewModelFactory.CreateBudgetAbsenceAllowanceDetailViewModels(new DateOnly(2016, 12, 27), null);
			allowanceDetailViewModels[0].ShrinkedAllowance.Should().Be(0);
			allowanceDetailViewModels[1].FullAllowance.Should().Be(0);
			allowanceDetailViewModels[3].ShrinkedAllowance.Should().Be(1);
			allowanceDetailViewModels[3].FullAllowance.Should().Be(2);
		}

		[Test]
		public void ShouldGetAllowanceDetailsInTheFirstBudgetGroup()
		{
			var budgetGroup1 = createBudgetGroup("bg1");
			var budgetGroup2 = createBudgetGroup("bg2");
			createBudgetDay(budgetGroup1, new DateOnly(2016, 12, 28), 1, 2);
			createBudgetDay(budgetGroup2, new DateOnly(2016, 12, 28), 3, 4);

			var allowanceDetailViewModels =
				RequestAllowanceViewModelFactory.CreateBudgetAbsenceAllowanceDetailViewModels(new DateOnly(2016, 12, 27), null);
			allowanceDetailViewModels[3].ShrinkedAllowance.Should().Be(1);
			allowanceDetailViewModels[3].FullAllowance.Should().Be(2);
			allowanceDetailViewModels.Any(a => Math.Abs(a.ShrinkedAllowance - 3) < 0.0000001).Should().Be(false);
		}

		[Test]
		public void ShouldGetAllowanceDetailsInTheSelectedBudgetGroup()
		{
			var budgetGroup1 = createBudgetGroup("bg1");
			var budgetGroup2 = createBudgetGroup("bg2");
			createBudgetDay(budgetGroup1, new DateOnly(2016, 12, 28), 1, 2);
			createBudgetDay(budgetGroup2, new DateOnly(2016, 12, 28), 3, 4);

			var allowanceDetailViewModels =
				RequestAllowanceViewModelFactory.CreateBudgetAbsenceAllowanceDetailViewModels(new DateOnly(2016, 12, 27), budgetGroup2.Id);
			allowanceDetailViewModels[3].ShrinkedAllowance.Should().Be(3);
			allowanceDetailViewModels[3].FullAllowance.Should().Be(4);
			allowanceDetailViewModels.Any(a => Math.Abs(a.ShrinkedAllowance - 1) < 0.0000001).Should().Be(false);
		}

		[Test]
		public void ShouldGetAllowanceDetailsWithAbsencesInBudgetGroup()
		{
			var budgetGroup1 = createBudgetGroup("bg1");
			var customShrinkage = new CustomShrinkage("test", true);
			var absence = AbsenceFactory.CreateAbsence("holiday").WithId();
			customShrinkage.AddAbsence(absence);
			budgetGroup1.AddCustomShrinkage(customShrinkage);

			var allowanceDetailViewModels =
				RequestAllowanceViewModelFactory.CreateBudgetAbsenceAllowanceDetailViewModels(new DateOnly(2016, 12, 27), budgetGroup1.Id);

			allowanceDetailViewModels[0].UsedAbsencesDictionary.Count.Should().Be(1);
			allowanceDetailViewModels[0].UsedAbsencesDictionary.ContainsKey(absence.Name).Should().Be(true);
		}

		[Test]
		public void ShouldGetAllowanceDetailsWithDuplicatedAbsenceNames()
		{
			var budgetGroup1 = createBudgetGroup("bg1");
			var customShrinkage = new CustomShrinkage("test", true);
			var name = "holiday";
			customShrinkage.AddAbsence(AbsenceFactory.CreateAbsence(name).WithId());
			customShrinkage.AddAbsence(AbsenceFactory.CreateAbsence(name).WithId());
			budgetGroup1.AddCustomShrinkage(customShrinkage);

			var allowanceDetailViewModels =
				RequestAllowanceViewModelFactory.CreateBudgetAbsenceAllowanceDetailViewModels(new DateOnly(2016, 12, 27), budgetGroup1.Id);

			allowanceDetailViewModels[0].UsedAbsencesDictionary.Count.Should().Be(1);
			allowanceDetailViewModels[0].UsedAbsencesDictionary.ContainsKey(name).Should().Be(true);
		}

		private IBudgetGroup createBudgetGroup(string name)
		{
			var budgetGroup = new BudgetGroup { Name = name }.WithId();
			BudgetGroupRepository.Add(budgetGroup);
			return budgetGroup;
		}

		private void createBudgetDay(IBudgetGroup budgetGroup, DateOnly date, double shrinkedAllowance, double fullAllowance)
		{
			var budgetDay = new BudgetDay(budgetGroup, CurrentScenario.Current(), date)
			{
				ShrinkedAllowance = shrinkedAllowance,
				FullAllowance = fullAllowance
			};
			BudgetDayRepository.Add(budgetDay);
		}
	}
}