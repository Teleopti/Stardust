using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture]
	public class LoadForecastedHoursCommandTest
	{
		private MockRepository mocks;
		private IBudgetGroup budgetGroup;
		private IBudgetDay budgetDay2;
		private IBudgetDay budgetDay1;
		private BudgetGroupDayDetailModel dayDetail1;
		private BudgetGroupDayDetailModel dayDetail2;
		private BudgetGroupMainModel mainModel;
		private ISkill skill;
		private IScenario scenario;
		private IDictionary<ISkill, IEnumerable<ISkillDay>> skillDayDictionary;
		private ISkillStaffPeriod skillStaffPeriod1;
		private ISkillStaffPeriod skillStaffPeriod2;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();

			budgetGroup = new BudgetGroup();
			skill = SkillFactory.CreateSkill("Test");
			budgetGroup.AddSkill(skill);
			budgetGroup.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			scenario = ScenarioFactory.CreateScenarioAggregate();

			budgetDay1 = new BudgetDay(budgetGroup, scenario, new DateOnly(2010, 10, 20));
			budgetDay2 = new BudgetDay(budgetGroup, scenario, new DateOnly(2010, 10, 22));

			dayDetail1 = new BudgetGroupDayDetailModel(budgetDay1);
			dayDetail2 = new BudgetGroupDayDetailModel(budgetDay2);

			skillStaffPeriod1 = mocks.StrictMock<ISkillStaffPeriod>();
			skillStaffPeriod2 = mocks.StrictMock<ISkillStaffPeriod>();

			mainModel = new BudgetGroupMainModel(null)
			{
				BudgetGroup = budgetGroup,
				Period = new DateOnlyPeriod(2010, 10, 15, 2010, 10, 31),
				Scenario = scenario
			};
		}

		[Test]
		public void ShouldLoadForecastedHoursForTheGivenBudgetDays()
		{
			var budgetSkillStaffPeriodProvider = mocks.StrictMock<IBudgetSkillStaffPeriodProvider>();
			var budgetSkillStaffPeriodContainer = mocks.StrictMock<IBudgetSkillStaffPeriodContainer>();

			using (mocks.Record())
			{
				Expect.Call(budgetSkillStaffPeriodProvider.CreateContainer()).Return(budgetSkillStaffPeriodContainer);
				Expect.Call(budgetSkillStaffPeriodContainer.SelectedBudgetDays).Return(new List<IBudgetGroupDayDetailModel> { dayDetail1, dayDetail2 });
				Expect.Call(
					budgetSkillStaffPeriodContainer.ForPeriod(
						new DateOnlyPeriod(budgetDay1.Day, budgetDay1.Day).ToDateTimePeriod(budgetGroup.TimeZone))).Return(new List<ISkillStaffPeriod> { skillStaffPeriod1 });
				Expect.Call(
					budgetSkillStaffPeriodContainer.ForPeriod(
						new DateOnlyPeriod(budgetDay2.Day, budgetDay2.Day).ToDateTimePeriod(budgetGroup.TimeZone))).Return(new List<ISkillStaffPeriod> { skillStaffPeriod2 });

				Expect.Call(skillStaffPeriod1.ForecastedIncomingDemand()).Return(TimeSpan.FromHours(10));
				Expect.Call(skillStaffPeriod2.ForecastedIncomingDemand()).Return(TimeSpan.FromHours(20));
			}
			using (mocks.Playback())
			{
				var target = new LoadForecastedHoursCommand(budgetSkillStaffPeriodProvider, mainModel);
				target.Execute();

				Assert.AreEqual(10d, dayDetail1.ForecastedHours);
				Assert.AreEqual(20d, dayDetail2.ForecastedHours);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldCreateSkillStaffPeriodContainerFromTheProvider()
		{
			var selectedBudgetDays = mocks.StrictMock<ISelectedBudgetDays>();
			var skillDayLoader = mocks.StrictMock<ISkillDayLoadHelper>();
			var skillDay1 = mocks.StrictMock<ISkillDay>();
			var skillDay2 = mocks.StrictMock<ISkillDay>();

			prepareDictionaryForRegularSkill(skillDay1, skillDay2);

			using (mocks.Record())
			{
				Expect.Call(selectedBudgetDays.Find()).Return(new List<IBudgetGroupDayDetailModel> { dayDetail1, dayDetail2 });

				var expectedPeriodToLoad = new DateOnlyPeriod(2010, 10, 20, 2010, 10, 22);
				Expect.Call(skillDayLoader.LoadBudgetSkillDays(expectedPeriodToLoad, budgetGroup.SkillCollection, scenario)).Return(
																	skillDayDictionary);
				Expect.Call(skillDay1.SkillStaffPeriodCollection).Return(new [] { skillStaffPeriod1 }).Repeat.AtLeastOnce();
				Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(new [] { skillStaffPeriod2 }).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod1.Period).Return(
					DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 10, 19, 22, 0, 0, DateTimeKind.Utc), 0)).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod2.Period).Return(
					DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 10, 21, 22, 0, 0, DateTimeKind.Utc), 0)).Repeat.AtLeastOnce();

				Expect.Call(skillDay1.OpenForWork).Return(new OpenForWork(true,true));
				Expect.Call(skillDay1.CurrentDate).Return(new DateOnly(2010, 10, 20));
				Expect.Call(skillDay2.CurrentDate).Return(new DateOnly(2010, 10, 20));
			}
			using (mocks.Playback())
			{
				var target = new BudgetingSkillStaffPeriodProvider(mainModel, selectedBudgetDays, skillDayLoader);
				var result = target.CreateContainer();
				Assert.AreEqual(2, result.SelectedBudgetDays.Count());
				Assert.AreEqual(2, result.ForPeriod(new DateTimePeriod(2010, 1, 1, 2010, 12, 31)).Count());
			}
		}

		private void prepareDictionaryForRegularSkill(ISkillDay skillDay1, ISkillDay skillDay2)
		{
			skillDayDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			skillDayDictionary.Add(skill, new List<ISkillDay> { skillDay1, skillDay2 });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldOnlyHaveSelectedChildSkillInSkillStaffPeriodContainerFromTheProvider()
		{
			var selectedBudgetDays = mocks.StrictMock<ISelectedBudgetDays>();
			var skillDayLoader = mocks.StrictMock<ISkillDayLoadHelper>();
			var skillDay1 = mocks.StrictMock<ISkillDay>();
			var skillDay2 = mocks.StrictMock<ISkillDay>();

			prepareDictionaryForMultisite(skillDay1, skillDay2);

			var expectedPeriodToLoad = new DateOnlyPeriod(2010, 10, 20, 2010, 10, 22);

			using (mocks.Record())
			{
				Expect.Call(selectedBudgetDays.Find()).Return(new List<IBudgetGroupDayDetailModel> { dayDetail1, dayDetail2 });

				Expect.Call(skillDayLoader.LoadBudgetSkillDays(expectedPeriodToLoad, budgetGroup.SkillCollection, scenario)).Return(
																	skillDayDictionary);
				Expect.Call(skillDay2.SkillStaffPeriodCollection).Return(new [] { skillStaffPeriod2 }).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod2.Period).Return(
					DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 10, 21, 22, 0, 0, DateTimeKind.Utc), 0)).Repeat.AtLeastOnce();

                Expect.Call(skillDay1.OpenForWork).Return(new OpenForWork(true,true));
			    Expect.Call(skillDay1.CurrentDate).Return(new DateOnly(2010, 10, 20));
			    Expect.Call(skillDay2.CurrentDate).Return(new DateOnly(2010, 10, 20));
			}
			using (mocks.Playback())
			{
				var target = new BudgetingSkillStaffPeriodProvider(mainModel, selectedBudgetDays, skillDayLoader);
				var result = target.CreateContainer();
				Assert.AreEqual(2, result.SelectedBudgetDays.Count());
				Assert.AreEqual(1, result.ForPeriod(new DateTimePeriod(2010, 1, 1, 2010, 12, 31)).Count());
			}
		}

		private void prepareDictionaryForMultisite(ISkillDay skillDay1, ISkillDay skillDay2)
		{
			var multisiteSkill = SkillFactory.CreateMultisiteSkill("test");
			var childSkill = SkillFactory.CreateChildSkill("child", multisiteSkill);

			budgetGroup.RemoveAllSkills();
			budgetGroup.AddSkill(childSkill);

			skillDayDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			skillDayDictionary.Add(multisiteSkill, new List<ISkillDay> { skillDay1 });
			skillDayDictionary.Add(childSkill, new List<ISkillDay> { skillDay2 });
		}

		[Test]
		public void ShouldCreateSkillStaffPeriodContainerFromTheProviderWithEmptySelection()
		{
			var selectedBudgetDays = mocks.StrictMock<ISelectedBudgetDays>();
			var skillDayLoader = mocks.StrictMock<ISkillDayLoadHelper>();

			using (mocks.Record())
			{
				Expect.Call(selectedBudgetDays.Find()).Return(new List<IBudgetGroupDayDetailModel>());
			}
			using (mocks.Playback())
			{
				var target = new BudgetingSkillStaffPeriodProvider(mainModel, selectedBudgetDays, skillDayLoader);
				var result = target.CreateContainer();
				Assert.AreEqual(0, result.SelectedBudgetDays.Count());
				Assert.AreEqual(0, result.ForPeriod(new DateTimePeriod(2010, 1, 1, 2010, 12, 31)).Count());
			}
		}
	}
}
