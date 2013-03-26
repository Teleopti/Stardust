using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class BudgetStepDefinitions
	{
		[Given(@"there is a budgetgroup '(.*)'")]
		public void GivenThereIsABudgetgroup(string name)
		{
			var budgetConfigurable = new BudgetGroupConfigurable(name);
			UserFactory.User().Setup(budgetConfigurable);
		}

		[Given(@"there is a budgetday")]
		public void GivenThereIsAnBudgetday(Table table)
		{
			var budgetday = table.CreateInstance<BudgetdayConfigurable>();
			UserFactory.User().Setup(budgetday);
		}


		//henke hitta på ett bättre namn
		[Given(@"there is a \(readonly\) PersonScheduleDayModel")]
		public void GivenThereIsAReadonlyPersonScheduleDayModel(Table table)
		{
			var scheduleReadOnlyProjection = table.CreateInstance<ReadModelScheduleProjectionConfigurable>();
			UserFactory.User().Setup(scheduleReadOnlyProjection);
		}



		//henke
		[Then(@"I should see stuff for '(.*)' to '(.*)'")]
		public void ThenIShouldSeeStuffForTo(DateTime from, DateTime to)
		{

			//
		}

		[Given(@"I have stuff for '(.*)' to '(.*)'")]
		public void GivenIHaveStuffForTo(DateTime from, DateTime to)
		{
			var removeMe = new RemoveMe() {From = from, To = to};
			UserFactory.User().Setup(removeMe);
		}


	}

	public class RemoveMe : IPostSetup
	{
		public DateTime From { get; set; }
		public DateTime To { get; set; }

		public void Apply(IPerson user, IUnitOfWork uow)
		{
			var rogersFavvo = new ScheduleProjectionReadOnlyRepository(GlobalUnitOfWorkState.UnitOfWorkFactory);


			var scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;
			
			var budgetGroupRepo = new BudgetGroupRepository(uow);
			var firstBudgetGroup = budgetGroupRepo.LoadAll().First();
			//firstBudgetGroup.

			var customShrinkage = new CustomShrinkage("Semester");
			customShrinkage.IncludedInAllowance = true;
			

			var absenceRepo = new AbsenceRepository(uow);
			var absence = absenceRepo.LoadAll().First(a => a.Name == "Vacation");


			
				customShrinkage.AddAbsence(absence);
				
		

			firstBudgetGroup.AddCustomShrinkage(customShrinkage);

			//var budgetGroup = new BudgetGroup();
			var period = new DateOnlyPeriod(new DateOnly(From), new DateOnly(To));

			var result = rogersFavvo.AbsenceTimePerBudgetGroup(period, firstBudgetGroup, scenario);

			//tjoho

			Assert.That(result, Is.Not.Null);
		}
	}
}