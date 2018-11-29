using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;


#pragma warning disable 618

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture]
	public class SaveBudgetDaysTest
	{
		private IBudgetGroupDataService target;
		private MockRepository mocks;
		private IBudgetDayRepository budgetDayRepository;
		private IBudgetDayProvider budgetDayProvider;
		private IBudgetPermissionService budgetPermissionService;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			var mainModel = new BudgetGroupMainModel(null) { Period = new DateOnlyPeriod(2010, 10, 1, 2010, 10, 31) };
			budgetDayRepository = mocks.StrictMock<IBudgetDayRepository>();
			budgetDayProvider = mocks.StrictMock<IBudgetDayProvider>();
			budgetPermissionService = mocks.StrictMock<IBudgetPermissionService>();
			target = new BudgetGroupDataService(mainModel, budgetDayRepository, null, budgetPermissionService);
		}

		[Test]
		public void ShouldSaveNewBudgetDays()
		{
			IBudgetDay budgetDay = new BudgetDay(null, null, new DateOnly(2010, 10, 13));
			var budgetGroupDayDetailModel = mocks.StrictMock<IBudgetGroupDayDetailModel>();
			using (mocks.Record())
			{
				budgetDayRepository.Add(budgetDay);
				Expect.Call(budgetGroupDayDetailModel.UpdateBudgetDay);
				Expect.Call(budgetGroupDayDetailModel.BudgetDay).Return(budgetDay).Repeat.AtLeastOnce();
				Expect.Call(budgetDayProvider.VisibleDayModels()).Return(new List<IBudgetGroupDayDetailModel>
																			 {budgetGroupDayDetailModel}).Repeat.AtLeastOnce();
			}
			using (mocks.Playback())
			{
				target.Save(budgetDayProvider);
			}
		}

		[Test]
		public void ShouldSaveUpdatedBudgetDays()
		{
			IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
			IBudgetDay budgetDay = new BudgetDay(null, null, new DateOnly(2010, 10, 13));
			IBudgetDay updatedBudgetDay = new BudgetDay(null, null, new DateOnly(2010, 10, 13));
			var budgetGroupDayDetailModel = mocks.StrictMock<IBudgetGroupDayDetailModel>();
			budgetDay.SetId(Guid.NewGuid());
			updatedBudgetDay.SetId(Guid.NewGuid());
			using (mocks.Record())
			{
				Expect.Call(budgetDayRepository.UnitOfWork).Return(unitOfWork);
				Expect.Call(unitOfWork.Merge(budgetDay)).Return(updatedBudgetDay);
				Expect.Call(budgetGroupDayDetailModel.UpdateBudgetDay);
				Expect.Call(budgetGroupDayDetailModel.BudgetDay).Return(budgetDay).Repeat.AtLeastOnce();
				Expect.Call(budgetGroupDayDetailModel.BudgetDay = updatedBudgetDay);
				Expect.Call(budgetDayProvider.VisibleDayModels()).Return(new List<IBudgetGroupDayDetailModel> { budgetGroupDayDetailModel }).Repeat.AtLeastOnce();
			}
			using (mocks.Playback())
			{
				target.Save(budgetDayProvider);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldBudgetDayUpdatedDueToParametersChanges()
		{
			IBudgetDay budgetDay = new BudgetDay(null, null, new DateOnly(2010, 10, 13));
			var budgetGroupDayDetailModel = new BudgetGroupDayDetailModel(budgetDay);
			budgetGroupDayDetailModel.AttritionRate = new Percent(0.2d);
			budgetGroupDayDetailModel.FulltimeEquivalentHours = 50d;
			budgetGroupDayDetailModel.StaffEmployed = 101d;
			budgetGroupDayDetailModel.Recruitment = 1d;
			budgetGroupDayDetailModel.Contractors = 10d;
			budgetGroupDayDetailModel.DaysOffPerWeek = 2d;
			budgetGroupDayDetailModel.OvertimeHours = 10;
			budgetGroupDayDetailModel.StudentsHours = 20;
			budgetGroupDayDetailModel.ForecastedHours = 80;
			budgetGroupDayDetailModel.IsClosed = true;
			budgetGroupDayDetailModel.AbsenceExtra = 2d;
			budgetGroupDayDetailModel.AbsenceOverride = 10d;
			budgetGroupDayDetailModel.AbsenceThreshold = new Percent(0.8);
			budgetGroupDayDetailModel.FullAllowance = 10d;
			budgetGroupDayDetailModel.ShrinkedAllowance = 8d;

			budgetGroupDayDetailModel.UpdateBudgetDay();
			Assert.AreEqual(new Percent(0.2d), budgetDay.AttritionRate);
			Assert.AreEqual(50d, budgetDay.FulltimeEquivalentHours);
			Assert.AreEqual(101d, budgetDay.StaffEmployed);
			Assert.AreEqual(1d, budgetDay.Recruitment);
			Assert.AreEqual(10d, budgetDay.Contractors);
			Assert.AreEqual(2d, budgetDay.DaysOffPerWeek);
			Assert.AreEqual(10, budgetDay.OvertimeHours);
			Assert.AreEqual(20, budgetDay.StudentHours);
			Assert.AreEqual(80, budgetDay.ForecastedHours);
			Assert.IsTrue(budgetDay.IsClosed);
			Assert.AreEqual(2d, budgetDay.AbsenceExtra);
			Assert.AreEqual(10d, budgetDay.AbsenceOverride);
			Assert.AreEqual(new Percent(0.8), budgetDay.AbsenceThreshold);
			Assert.AreEqual(10d, budgetDay.FullAllowance);
			Assert.AreEqual(8d, budgetDay.ShrinkedAllowance);
		}
	}
}