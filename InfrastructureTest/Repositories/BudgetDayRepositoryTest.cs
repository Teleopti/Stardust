using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class BudgetDayRepositoryTest : RepositoryTest<IBudgetDay>
	{
		private string description;
		private TimeZoneInfo timeZone;
		private BudgetGroup budgetGroup;
		private IScenario scenario;
		private ICustomShrinkage customShrinkage;
		private ICustomEfficiencyShrinkage customEfficiencyShrinkage;
		private DateOnly budgetDayDate;

		protected override void ConcreteSetup()
		{
			description = "budget";
			timeZone = TimeZoneInfo.GetSystemTimeZones()[0];
			budgetGroup = new BudgetGroup { Name = description, TimeZone = timeZone };
			budgetGroup.TrySetDaysPerYear(365);
			customShrinkage = new CustomShrinkage("Vacation");
			budgetGroup.AddCustomShrinkage(customShrinkage);
			customEfficiencyShrinkage = new CustomEfficiencyShrinkage("Coffee");
			budgetGroup.AddCustomEfficiencyShrinkage(customEfficiencyShrinkage);
			PersistAndRemoveFromUnitOfWork(budgetGroup);

			scenario = ScenarioFactory.CreateScenarioAggregate();
			PersistAndRemoveFromUnitOfWork(scenario);
			budgetDayDate = new DateOnly(2010, 9, 30);
		}

		protected override IBudgetDay CreateAggregateWithCorrectBusinessUnit()
		{
			budgetDayDate = budgetDayDate.AddDays(1);
			IBudgetDay budgetDay = new BudgetDay(budgetGroup, scenario, budgetDayDate)
			{
				AttritionRate = new Percent(0.1),
				Contractors = 1.5d,
				DaysOffPerWeek = 2d,
				ForecastedHours = 3d,
				FulltimeEquivalentHours = 7.6d,
				OvertimeHours = 3,
				Recruitment = 3.25,
				StaffEmployed = 50d,
				StudentHours = 4,
				IsClosed = true,
				AbsenceThreshold = new Percent(0.8),
				AbsenceExtra = 2d,
				AbsenceOverride = 10d,
				FullAllowance = 10d,
				ShrinkedAllowance = 8d
			};
			budgetDay.CustomShrinkages.SetShrinkage(customShrinkage.Id.GetValueOrDefault(Guid.Empty), new Percent(0.07d));
			budgetDay.CustomEfficiencyShrinkages.SetEfficiencyShrinkage(
				customEfficiencyShrinkage.Id.GetValueOrDefault(Guid.Empty), new Percent(0.08d));
			return budgetDay;
		}

		protected override void VerifyAggregateGraphProperties(IBudgetDay loadedAggregateFromDatabase)
		{
			budgetDayDate = loadedAggregateFromDatabase.Day.AddDays(-1);
			var budgetDay = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(budgetDay.AttritionRate, loadedAggregateFromDatabase.AttritionRate);
			Assert.AreEqual(budgetDay.BudgetGroup, loadedAggregateFromDatabase.BudgetGroup);
			Assert.AreEqual(budgetDay.Contractors, loadedAggregateFromDatabase.Contractors);
			Assert.AreEqual(budgetDay.Day, loadedAggregateFromDatabase.Day);
			Assert.AreEqual(budgetDay.DaysOffPerWeek, loadedAggregateFromDatabase.DaysOffPerWeek);
			Assert.AreEqual(budgetDay.ForecastedHours, loadedAggregateFromDatabase.ForecastedHours);
			Assert.AreEqual(budgetDay.FulltimeEquivalentHours, loadedAggregateFromDatabase.FulltimeEquivalentHours);
			Assert.AreEqual(budgetDay.OvertimeHours, loadedAggregateFromDatabase.OvertimeHours);
			Assert.AreEqual(budgetDay.Recruitment, loadedAggregateFromDatabase.Recruitment);
			Assert.AreEqual(budgetDay.Scenario, loadedAggregateFromDatabase.Scenario);
			Assert.AreEqual(budgetDay.StaffEmployed, loadedAggregateFromDatabase.StaffEmployed);
			Assert.AreEqual(budgetDay.StudentHours, loadedAggregateFromDatabase.StudentHours);
			Assert.AreEqual(budgetDay.IsClosed, loadedAggregateFromDatabase.IsClosed);
			Assert.AreEqual(budgetDay.AbsenceThreshold, loadedAggregateFromDatabase.AbsenceThreshold);
			Assert.AreEqual(budgetDay.AbsenceExtra, loadedAggregateFromDatabase.AbsenceExtra);
			Assert.AreEqual(budgetDay.AbsenceOverride, loadedAggregateFromDatabase.AbsenceOverride);
			Assert.AreEqual(budgetDay.FullAllowance, loadedAggregateFromDatabase.FullAllowance);
			Assert.AreEqual(budgetDay.ShrinkedAllowance, loadedAggregateFromDatabase.ShrinkedAllowance);

			var customShrinkageId = customShrinkage.Id.GetValueOrDefault(Guid.Empty);
			Assert.AreEqual(budgetDay.CustomShrinkages.GetShrinkage(customShrinkageId),
							loadedAggregateFromDatabase.CustomShrinkages.GetShrinkage(customShrinkageId));

			var customEfficiencyShrinkageId = customEfficiencyShrinkage.Id.GetValueOrDefault(Guid.Empty);
			Assert.AreEqual(budgetDay.CustomEfficiencyShrinkages.GetEfficiencyShrinkage(customEfficiencyShrinkageId),
							loadedAggregateFromDatabase.CustomEfficiencyShrinkages.GetEfficiencyShrinkage(customEfficiencyShrinkageId));

			//  Assert.AreEqual(1, customShrinkage.AbsenceCollection.Count());
		}

		protected override Repository<IBudgetDay> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new BudgetDayRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldGetLastDateWithStaffEmployedSpecified()
		{
			IBudgetDay budgetDay = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(budgetDay);

			IBudgetDayRepository budgetDayRepository = new BudgetDayRepository(CurrentUnitOfWork.Make());
			DateOnly lastDayWithStaffEmployed = budgetDayRepository.FindLastDayWithStaffEmployed(scenario, budgetGroup, budgetDay.Day.AddDays(1));
			Assert.AreEqual(budgetDay.Day, lastDayWithStaffEmployed);
		}

		[Test]
		public void ShouldReturnLastDateToSearchWhenNoBudgetDayFound()
		{
			IBudgetDayRepository budgetDayRepository = new BudgetDayRepository(CurrentUnitOfWork.Make());
			DateOnly lastDayWithStaffEmployed = budgetDayRepository.FindLastDayWithStaffEmployed(scenario, budgetGroup, DateOnly.Today);
			Assert.AreEqual(DateOnly.Today, lastDayWithStaffEmployed);
		}

		[Test]
		public void ShouldGetSavedDayWithinSelectedPeriod()
		{
			IBudgetDay budgetDay = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(budgetDay);

			IBudgetDayRepository budgetDayRepository = new BudgetDayRepository(CurrentUnitOfWork.Make());
			DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 9, 10, 2010, 10, 30);
			IList<IBudgetDay> foundBudgetDays = budgetDayRepository.Find(scenario, budgetGroup, dateOnlyPeriod);
			Assert.That(1, Is.EqualTo(foundBudgetDays.Count));
		}

		[Test]
		public void ShouldCreateRepositoryWithUnitOfWorkFactory()
		{
			IBudgetDayRepository budgetDayRepository = new BudgetDayRepository(CurrentUnitOfWork.Make());
			Assert.IsNotNull(budgetDayRepository);
		}

		

	}
}