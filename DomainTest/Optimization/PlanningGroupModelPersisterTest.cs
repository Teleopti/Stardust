using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class PlanningGroupModelPersisterTest
	{
		public FakePlanningGroupRepository PlanningGroupRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeContractRepository ContractRepository;
		public FakeSkillRepository SkillRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public IPlanningGroupModelPersister Target;

		[Test]
		public void ShouldInsertContractFilter()
		{
			var contract = new Contract("_").WithId();
			ContractRepository.Add(contract);
			var model = new PlanningGroupModel();
			model.Filters.Add(
				new FilterModel
				{
					Id = contract.Id.Value,
					FilterType = FilterModel.ContractFilterType
				}
			);

			Target.Persist(model);

			var inDb = PlanningGroupRepository.LoadAll().Single();
			var contractFilter = (ContractFilter)inDb.Filters.Single();
			contractFilter.Contract.Should().Be.EqualTo(contract);

			inDb.Settings.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldInsertSiteFilter()
		{
			var site = new Site("_").WithId();
			SiteRepository.Add(site);

			var model = new PlanningGroupModel();
			model.Filters.Add(
				new FilterModel
				{
					Id = site.Id.Value,
					FilterType = FilterModel.SiteFilterType
				}
				);

			Target.Persist(model);

			var inDb = PlanningGroupRepository.LoadAll().Single();
			var siteFilter = (SiteFilter)inDb.Filters.Single();
			siteFilter.Site.Should().Be.EqualTo(site);

			inDb.Settings.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldInsertTeamFilter()
		{
			var team = new Team().WithId();
			TeamRepository.Add(team);

			var model = new PlanningGroupModel();
			model.Filters.Add(
				new FilterModel
				{
					Id = team.Id.Value,
					FilterType = "team"
				}
				);

			Target.Persist(model);

			var inDb = PlanningGroupRepository.LoadAll().Single();
			var teamFilter = (TeamFilter)inDb.Filters.Single();
			teamFilter.Team.Should().Be.EqualTo(team);

			inDb.Settings.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldInsertSkillFilter()
		{
			var skill = new Skill("_").WithId();
			SkillRepository.Add(skill);
			var model = new PlanningGroupModel();
			model.Filters.Add(
				new FilterModel
				{
					Id = skill.Id.Value,
					FilterType = FilterModel.SkillFilterType
				}
			);

			Target.Persist(model);

			var inDb = PlanningGroupRepository.LoadAll().Single();
			var skillFilter = (SkillFilter)inDb.Filters.Single();
			skillFilter.Skill.Should().Be.EqualTo(skill);

			inDb.Settings.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotAddExistingTeamFilter()
		{
			var team = new Team().WithId();
			TeamRepository.Add(team);

			var model = new PlanningGroupModel();
			model.Filters.Add(new FilterModel { Id = team.Id.Value, FilterType = "team" });
			model.Filters.Add(new FilterModel { Id = team.Id.Value, FilterType = "team" });

			Target.Persist(model);

			var inDb = PlanningGroupRepository.LoadAll().Single();
			inDb.Filters.Count().Should().Be.EqualTo(1);

			inDb.Settings.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotAddExistingContractFilter()
		{
			var contract = new Contract("_").WithId();
			ContractRepository.Add(contract);

			var model = new PlanningGroupModel();
			model.Filters.Add(new FilterModel { Id = contract.Id.Value, FilterType = "contract" });
			model.Filters.Add(new FilterModel { Id = contract.Id.Value, FilterType = "contract" });

			Target.Persist(model);

			var inDb = PlanningGroupRepository.LoadAll().Single();
			inDb.Filters.Count().Should().Be.EqualTo(1);

			inDb.Settings.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotAddExistingSiteFilter()
		{
			var site = new Site("_").WithId();
			SiteRepository.Add(site);

			var model = new PlanningGroupModel();
			model.Filters.Add(new FilterModel { Id = site.Id.Value, FilterType = "site" });
			model.Filters.Add(new FilterModel { Id = site.Id.Value, FilterType = "site" });

			Target.Persist(model);

			var inDb = PlanningGroupRepository.LoadAll().Single();
			inDb.Filters.Count().Should().Be.EqualTo(1);

			inDb.Settings.Should().Not.Be.Empty();
		}


		[Test]
		public void ShouldNotAddExistingSkillFilter()
		{
			var skill = new Skill("_").WithId();
			SkillRepository.Add(skill);

			var model = new PlanningGroupModel();
			model.Filters.Add(new FilterModel { Id = skill.Id.Value, FilterType = "skill" });
			model.Filters.Add(new FilterModel { Id = skill.Id.Value, FilterType = "skill" });

			Target.Persist(model);

			var inDb = PlanningGroupRepository.LoadAll().Single();
			inDb.Filters.Count().Should().Be.EqualTo(1);

			inDb.Settings.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldThrowIfUnknownFilter()
		{
			var model = new PlanningGroupModel();
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
			var existing = new PlanningGroup()
				.WithId()
				.AddFilter(new ContractFilter(contract));
			var team = new Team().WithId();

			PlanningGroupRepository.Add(existing);
			ContractRepository.Add(contract);
			TeamRepository.Add(team);

			var model = new PlanningGroupModel
			{
				Id = existing.Id.Value,
				Filters = new List<FilterModel> { new FilterModel { FilterType = FilterModel.TeamFilterType, Name = team.Description.Name, Id = team.Id.Value } }
			};

			Target.Persist(model);

			var onlyFilterInDb = (TeamFilter)PlanningGroupRepository.LoadAll().Single().Filters.Single();
			onlyFilterInDb.Team.Id.Value.Should().Be.EqualTo(team.Id.Value);
		}

		[Test]
		public void ShouldUpdateDefaultPlanningGroupSettingWhenCreatePlanningGroup()
		{
			var model = new PlanningGroupModel
			{
				Settings = new[]
				{
					new PlanningGroupSettingsModel
					{
						Default = true,
						BlockFinderType = BlockFinderType.SchedulePeriod,
						BlockSameShiftCategory = true,
						MaxDayOffsPerWeek = 3
					}
				}
			};

			Target.Persist(model);

			var inDb = PlanningGroupRepository.LoadAll().Single();
			var planningGroupSettings = inDb.Settings.Single();
			planningGroupSettings.BlockFinderType.Should().Be.EqualTo(BlockFinderType.SchedulePeriod);
			planningGroupSettings.BlockSameShiftCategory.Should().Be.True();
			planningGroupSettings.DayOffsPerWeek.Maximum.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldInsertName()
		{
			var model = new PlanningGroupModel();
			var expectedName = RandomName.Make();
			model.Name = expectedName;

			Target.Persist(model);
			var inDb = PlanningGroupRepository.LoadAll().Single();
			inDb.Name.Should().Be.EqualTo(expectedName);
		}

		[Test]
		public void ShouldRemovePlanningGroupAndRemoveDayOffRules()
		{
			var existing = new PlanningGroup()
				.WithId();
			PlanningGroupRepository.Add(existing);

			Target.Delete(existing.Id.GetValueOrDefault());

			var inDb = PlanningGroupRepository.Get(existing.Id.GetValueOrDefault());
			inDb.Should().Not.Be.Null();
			((IDeleteTag)inDb).IsDeleted.Should().Be.True();

			inDb.Settings.Where(x => !x.Default).Should().Be.Empty();
		}
	}
}