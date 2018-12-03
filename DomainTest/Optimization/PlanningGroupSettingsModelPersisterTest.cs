using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class PlanningGroupSettingsModelPersisterTest
	{
		public FakeContractRepository ContractRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;
		public IPlanningGroupSettingsModelPersister Target;

		[Test]
		public void ShouldUpdatePreferenceValue()
		{
			var planningGroup = new PlanningGroup();
			PlanningGroupRepository.Has(planningGroup);
			var defaultSetting = planningGroup.Settings.Single(x=>x.Default);
			
			var model = new PlanningGroupSettingsModel
			{
				Id = defaultSetting.Id.Value,
				PlanningGroupId = planningGroup.Id.Value,
				PreferencePercent = 24
			};
			
			Target.Persist(model);
			
			PlanningGroupRepository.Get(planningGroup.Id.Value).Settings.PreferenceValue.Should().Be.EqualTo(new Percent(0.24));
		}

		[Test]
		public void ShouldUpdate()
		{
			var existing = new PlanningGroupSettings().WithId();
			var planningGroup = new PlanningGroup().WithId();
			planningGroup.AddSetting(existing);
			PlanningGroupRepository.Add(planningGroup);

			var model = new PlanningGroupSettingsModel
			{
				Id = existing.Id.Value,
				MinDayOffsPerWeek = 2,
				MaxDayOffsPerWeek = 6,
				MinConsecutiveDayOffs = 1,
				MaxConsecutiveDayOffs = 1,
				MinConsecutiveWorkdays = 2,
				MaxConsecutiveWorkdays = 2,
				BlockFinderType = BlockFinderType.BetweenDayOff,
				BlockSameStartTime = true,
				BlockSameShift = true,
				BlockSameShiftCategory = true,
				MinFullWeekendsOff = 2,
				MaxFullWeekendsOff = 6,
				MinWeekendDaysOff = 2,
				MaxWeekendDaysOff = 15,
				PlanningGroupId = planningGroup.Id.Value
			};

			Target.Persist(model);

			var inDb = PlanningGroupRepository.Get(planningGroup.Id.Value).Settings.Single(x => x.Id == existing.Id);
			inDb.DayOffsPerWeek.Should().Be.EqualTo(new MinMax<int>(model.MinDayOffsPerWeek, model.MaxDayOffsPerWeek));
			inDb.ConsecutiveDayOffs.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveDayOffs, model.MaxConsecutiveDayOffs));
			inDb.ConsecutiveWorkdays.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveWorkdays, model.MaxConsecutiveWorkdays));
			inDb.Default.Should().Be.False();
			inDb.BlockFinderType.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
			inDb.BlockSameStartTime.Should().Be.True();
			inDb.BlockSameShift.Should().Be.True();
			inDb.BlockSameShiftCategory.Should().Be.True();
			inDb.FullWeekendsOff.Should().Be.EqualTo(new MinMax<int>(model.MinFullWeekendsOff, model.MaxFullWeekendsOff));
			inDb.WeekendDaysOff.Should().Be.EqualTo(new MinMax<int>(model.MinWeekendDaysOff, model.MaxWeekendDaysOff));
		}

		[Test]
		public void ShouldUpdateDefault()
		{
			var planningGroup = new PlanningGroup();
			PlanningGroupRepository.Has(planningGroup);
			var defaultSetting = planningGroup.Settings.Single(x=>x.Default);
			var model = new PlanningGroupSettingsModel
			{
				Id = defaultSetting.Id.Value,
				MinDayOffsPerWeek = 2,
				MaxDayOffsPerWeek = 3,
				MinConsecutiveDayOffs = 1,
				MaxConsecutiveDayOffs = 4,
				MinConsecutiveWorkdays = 1,
				MaxConsecutiveWorkdays = 1,
				Default = true,
				BlockFinderType = BlockFinderType.BetweenDayOff,
				BlockSameStartTime = true,
				BlockSameShift = true,
				BlockSameShiftCategory = true,
				MinFullWeekendsOff = 2,
				MaxFullWeekendsOff = 6,
				MinWeekendDaysOff = 2,
				MaxWeekendDaysOff = 15,
				PlanningGroupId = planningGroup.Id.Value
			};

			Target.Persist(model);

			var inDb = PlanningGroupRepository.Get(planningGroup.Id.Value).Settings.Single(x => x.Id == defaultSetting.Id);
			inDb.DayOffsPerWeek.Should().Be.EqualTo(new MinMax<int>(model.MinDayOffsPerWeek, model.MaxDayOffsPerWeek));
			inDb.ConsecutiveDayOffs.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveDayOffs, model.MaxConsecutiveDayOffs));
			inDb.ConsecutiveWorkdays.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveWorkdays, model.MaxConsecutiveWorkdays));
			inDb.Default.Should().Be.True();
			inDb.BlockFinderType.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
			inDb.BlockSameStartTime.Should().Be.True();
			inDb.BlockSameShift.Should().Be.True();
			inDb.BlockSameShiftCategory.Should().Be.True();
			inDb.FullWeekendsOff.Should().Be.EqualTo(new MinMax<int>(model.MinFullWeekendsOff, model.MaxFullWeekendsOff));
			inDb.WeekendDaysOff.Should().Be.EqualTo(new MinMax<int>(model.MinWeekendDaysOff, model.MaxWeekendDaysOff));
		}

		[Test]
		public void ShouldInsert()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);
			
			var model = new PlanningGroupSettingsModel
			{
				MinDayOffsPerWeek = 1,
				MaxDayOffsPerWeek = 2,
				MinConsecutiveDayOffs = 3,
				MaxConsecutiveDayOffs = 4,
				MinConsecutiveWorkdays = 5,
				MaxConsecutiveWorkdays = 6,
				BlockFinderType = BlockFinderType.BetweenDayOff,
				BlockSameStartTime = true,
				BlockSameShift = true,
				BlockSameShiftCategory = true,
				MinFullWeekendsOff = 2,
				MaxFullWeekendsOff = 6,
				MinWeekendDaysOff = 2,
				MaxWeekendDaysOff = 15,
				PlanningGroupId = planningGroup.Id.Value
			};

			Target.Persist(model);

			var inDb = PlanningGroupRepository.Get(planningGroup.Id.Value).Settings.Single(x => !x.Default);
			inDb.DayOffsPerWeek.Should().Be.EqualTo(new MinMax<int>(model.MinDayOffsPerWeek, model.MaxDayOffsPerWeek));
			inDb.ConsecutiveDayOffs.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveDayOffs, model.MaxConsecutiveDayOffs));
			inDb.ConsecutiveWorkdays.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveWorkdays, model.MaxConsecutiveWorkdays));
			inDb.Default.Should().Be.False();
			inDb.BlockFinderType.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
			inDb.BlockSameStartTime.Should().Be.True();
			inDb.BlockSameShift.Should().Be.True();
			inDb.BlockSameShiftCategory.Should().Be.True();
			inDb.FullWeekendsOff.Should().Be.EqualTo(new MinMax<int>(model.MinFullWeekendsOff, model.MaxFullWeekendsOff));
			inDb.WeekendDaysOff.Should().Be.EqualTo(new MinMax<int>(model.MinWeekendDaysOff, model.MaxWeekendDaysOff));
		}

		[Test]
		public void ShouldInsertSettingsForPlanningGroup()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);

			var model = new PlanningGroupSettingsModel
			{
				MinDayOffsPerWeek = 1,
				MaxDayOffsPerWeek = 2,
				MinConsecutiveDayOffs = 3,
				MaxConsecutiveDayOffs = 4,
				MinConsecutiveWorkdays = 5,
				MaxConsecutiveWorkdays = 6,
				PlanningGroupId = planningGroup.Id,
				BlockFinderType = BlockFinderType.BetweenDayOff,
				BlockSameStartTime = true,
				BlockSameShift = true,
				BlockSameShiftCategory = true,
				MinFullWeekendsOff = 2,
				MaxFullWeekendsOff = 6,
				MinWeekendDaysOff = 2,
				MaxWeekendDaysOff = 15
			};

			Target.Persist(model);

			
			var inDb = PlanningGroupRepository.Get(planningGroup.Id.Value).Settings.Single(x => !x.Default);
			inDb.DayOffsPerWeek.Should().Be.EqualTo(new MinMax<int>(model.MinDayOffsPerWeek, model.MaxDayOffsPerWeek));
			inDb.ConsecutiveDayOffs.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveDayOffs, model.MaxConsecutiveDayOffs));
			inDb.ConsecutiveWorkdays.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveWorkdays, model.MaxConsecutiveWorkdays));
			inDb.Default.Should().Be.False();
			inDb.BlockFinderType.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
			inDb.BlockSameStartTime.Should().Be.True();
			inDb.BlockSameShift.Should().Be.True();
			inDb.BlockSameShiftCategory.Should().Be.True();
			inDb.FullWeekendsOff.Should().Be.EqualTo(new MinMax<int>(model.MinFullWeekendsOff, model.MaxFullWeekendsOff));
			inDb.WeekendDaysOff.Should().Be.EqualTo(new MinMax<int>(model.MinWeekendDaysOff, model.MaxWeekendDaysOff));
		}

		[Test]
		public void ShouldInsertContractFilter()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);
			
			var contract = new Contract("_").WithId();
			ContractRepository.Add(contract);
			var model = new PlanningGroupSettingsModel()
			{
				PlanningGroupId = planningGroup.Id.Value
			};
			model.Filters.Add(
				new FilterModel
				{
					Id = contract.Id.Value,
					FilterType = FilterModel.ContractFilterType
				}
			);

			Target.Persist(model);

			var inDb = PlanningGroupRepository.Load(planningGroup.Id.Value).Settings.Single(x => !x.Default);
			var contractFilter = (ContractFilter)inDb.Filters.Single();
			contractFilter.Contract.Should().Be.EqualTo(contract);
		}

		[Test]
		public void ShouldInsertSiteFilter()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);
			
			var site = new Site("_").WithId();
			SiteRepository.Add(site);

			var model = new PlanningGroupSettingsModel
			{
				PlanningGroupId = planningGroup.Id.Value
			};
			model.Filters.Add(
				new FilterModel
				{
					Id = site.Id.Value,
					FilterType = FilterModel.SiteFilterType
				}
			);

			Target.Persist(model);

			var inDb = PlanningGroupRepository.Load(planningGroup.Id.Value).Settings.Single(x => !x.Default);
			var siteFilter = (SiteFilter)inDb.Filters.Single();
			siteFilter.Site.Should().Be.EqualTo(site);
		}

		[Test]
		public void ShouldInsertTeamFilter()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);
			
			var team = new Team().WithId();
			TeamRepository.Add(team);

			var model = new PlanningGroupSettingsModel
			{
				PlanningGroupId = planningGroup.Id.Value
			};
			model.Filters.Add(
				new FilterModel
				{
					Id = team.Id.Value,
					FilterType = "team"
				}
			);

			Target.Persist(model);

			var inDb = PlanningGroupRepository.Load(planningGroup.Id.Value).Settings.Single(x => !x.Default);
			var teamFilter = (TeamFilter)inDb.Filters.Single();
			teamFilter.Team.Should().Be.EqualTo(team);
		}

		[Test]
		public void ShouldNotAddExistingTeamFilter()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);
			
			var team = new Team().WithId();
			TeamRepository.Add(team);

			var model = new PlanningGroupSettingsModel
			{
				PlanningGroupId = planningGroup.Id.Value
			};
			model.Filters.Add(new FilterModel{Id = team.Id.Value,FilterType = "team"});
			model.Filters.Add(new FilterModel{Id = team.Id.Value,FilterType = "team"});

			Target.Persist(model);

			var inDb = PlanningGroupRepository.Load(planningGroup.Id.Value).Settings.Single(x => !x.Default);
			inDb.Filters.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotAddExistingContractFilter()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);
			
			var contract = new Contract("_").WithId();
			ContractRepository.Add(contract);
			
			var model = new PlanningGroupSettingsModel
			{
				PlanningGroupId = planningGroup.Id.Value
			};
			model.Filters.Add(new FilterModel { Id = contract.Id.Value, FilterType = "contract" });
			model.Filters.Add(new FilterModel { Id = contract.Id.Value, FilterType = "contract" });

			Target.Persist(model);

			var inDb = PlanningGroupRepository.Load(planningGroup.Id.Value).Settings.Single(x => !x.Default);
			inDb.Filters.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotAddExistingSiteFilter()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);
			var site = new Site("_").WithId();
			SiteRepository.Add(site);

			var model = new PlanningGroupSettingsModel
			{
				PlanningGroupId = planningGroup.Id.Value
			};
			model.Filters.Add(new FilterModel { Id = site.Id.Value, FilterType = "site" });
			model.Filters.Add(new FilterModel { Id = site.Id.Value, FilterType = "site" });

			Target.Persist(model);

			var inDb = PlanningGroupRepository.Load(planningGroup.Id.Value).Settings.Single(x => !x.Default);
			inDb.Filters.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldThrowIfUnknownFilter()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);
			var model = new PlanningGroupSettingsModel
			{
				PlanningGroupId = planningGroup.Id.Value
			};
			model.Filters.Add(
				new FilterModel
				{
					Id = Guid.NewGuid(),
					FilterType = "unknown"
				}
			);

			Assert.Throws<NotSupportedException>(() =>
				Target.Persist(model));
		}

		[Test]
		public void ShouldClearOldFiltersWhenUpdate()
		{
			var contract = new Contract("_").WithId();
			var existing = new PlanningGroupSettings().WithId();
			var team = new Team().WithId();
			existing.AddFilter(new ContractFilter(contract));
			var planningGroup = new PlanningGroup().WithId();
			planningGroup.AddSetting(existing);
			PlanningGroupRepository.Add(planningGroup);

			ContractRepository.Add(contract);
			TeamRepository.Add(team);

			var model = new PlanningGroupSettingsModel
			{
				Id = existing.Id.Value,
				MinDayOffsPerWeek = 2,
				MaxDayOffsPerWeek = 6,
				MinConsecutiveDayOffs = 1,
				MaxConsecutiveDayOffs = 1,
				MinConsecutiveWorkdays = 2,
				MaxConsecutiveWorkdays = 2,
				Filters = new List<FilterModel> { new FilterModel { FilterType = FilterModel.TeamFilterType, Name = team.Description.Name, Id=team.Id.Value} },
				PlanningGroupId = planningGroup.Id.Value
			};

			Target.Persist(model);

			var onlyFilterInDb = (TeamFilter)PlanningGroupRepository.Load(planningGroup.Id.Value).Settings.Single(x => !x.Default).Filters.Single();
			onlyFilterInDb.Team.Id.Value.Should().Be.EqualTo(team.Id.Value);
		}

		[Test]
		public void ShouldInsertName()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);
			var model = new PlanningGroupSettingsModel
			{
				PlanningGroupId = planningGroup.Id.Value
			};
			var expectedName = RandomName.Make();
			model.Name = expectedName;

			Target.Persist(model);
			var inDb = PlanningGroupRepository.Load(planningGroup.Id.Value).Settings.Single(x => !x.Default);
			inDb.Name.Should().Be.EqualTo(expectedName);
		}

		[Test]
		public void ShouldDeletePlanningGroupSettings()
		{
			var planningGroup = new PlanningGroup().WithId();
			var planningGroupSettings = new PlanningGroupSettings().WithId();
			planningGroup.AddSetting(planningGroupSettings);
			PlanningGroupRepository.Add(planningGroup);

			Target.Delete(planningGroupSettings.Id.Value);

			PlanningGroupRepository.Get(planningGroup.Id.Value).Settings.Should().Not.Contain(planningGroupSettings);
		}

		[Test]
		public void ShouldInsertWithHighestPriority()
		{
			var planningGroup = new PlanningGroup().WithId();
			planningGroup.AddSetting(new PlanningGroupSettings
			{
				Priority = 2
			});
			PlanningGroupRepository.Add(planningGroup);
			var model = new PlanningGroupSettingsModel
			{
				PlanningGroupId = planningGroup.Id.Value
			};

			Target.Persist(model);

			var inDbs = PlanningGroupRepository.Get(planningGroup.Id.Value).Settings;
			inDbs.Max(x=>x.Priority).Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldNotChangeThePriorityWhenModifySettings()
		{
			var settingId = Guid.NewGuid();
			var planningGroupSettings = new PlanningGroupSettings
			{
				Priority = 2
			}.WithId(settingId);
			var planningGroup = new PlanningGroup().WithId();
			planningGroup.AddSetting(planningGroupSettings);
			PlanningGroupRepository.Add(planningGroup);
			var model = new PlanningGroupSettingsModel
			{
				Id = settingId,
				Priority = 2,
				PlanningGroupId = planningGroup.Id.Value
			};

			Target.Persist(model);

			PlanningGroupRepository.Load(planningGroup.Id.Value).Settings.Single(x => !x.Default).Priority
				.Should().Be.EqualTo(2);
		}
	}
}