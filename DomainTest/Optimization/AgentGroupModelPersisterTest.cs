using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
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
	public class AgentGroupModelPersisterTest
	{
		public FakeAgentGroupRepository AgentGroupRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeContractRepository ContractRepository;
		public FakeSkillRepository SkillRepository;
		public FakeDayOffRulesRepository DayOffRulesRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public IAgentGroupModelPersister Target;

		[Test]
		public void ShouldInsertContractFilter()
		{
			var contract = new Contract("_").WithId();
			ContractRepository.Add(contract);
			var model = new AgentGroupModel();
			model.Filters.Add(
				new FilterModel
				{
					Id = contract.Id.Value,
					FilterType = FilterModel.ContractFilterType
				}
			);

			Target.Persist(model);

			var inDb = AgentGroupRepository.LoadAll().Single();
			var contractFilter = (ContractFilter)inDb.Filters.Single();
			contractFilter.Contract.Should().Be.EqualTo(contract);

			var dayOffRules = DayOffRulesRepository.LoadAllByAgentGroup(inDb);
			dayOffRules.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldInsertSiteFilter()
		{
			var site = new Site("_").WithId();
			SiteRepository.Add(site);

			var model = new AgentGroupModel();
			model.Filters.Add(
				new FilterModel
				{
					Id = site.Id.Value,
					FilterType = FilterModel.SiteFilterType
				}
				);

			Target.Persist(model);

			var inDb = AgentGroupRepository.LoadAll().Single();
			var siteFilter = (SiteFilter)inDb.Filters.Single();
			siteFilter.Site.Should().Be.EqualTo(site);

			var dayOffRules = DayOffRulesRepository.LoadAllByAgentGroup(inDb);
			dayOffRules.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldInsertTeamFilter()
		{
			var team = new Team().WithId();
			TeamRepository.Add(team);

			var model = new AgentGroupModel();
			model.Filters.Add(
				new FilterModel
				{
					Id = team.Id.Value,
					FilterType = "team"
				}
				);

			Target.Persist(model);

			var inDb = AgentGroupRepository.LoadAll().Single();
			var teamFilter = (TeamFilter)inDb.Filters.Single();
			teamFilter.Team.Should().Be.EqualTo(team);

			var dayOffRules = DayOffRulesRepository.LoadAllByAgentGroup(inDb);
			dayOffRules.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldInsertSkillFilter()
		{
			var skill = new Skill("_").WithId();
			SkillRepository.Add(skill);
			var model = new AgentGroupModel();
			model.Filters.Add(
				new FilterModel
				{
					Id = skill.Id.Value,
					FilterType = FilterModel.SkillFilterType
				}
			);

			Target.Persist(model);

			var inDb = AgentGroupRepository.LoadAll().Single();
			var skillFilter = (SkillFilter)inDb.Filters.Single();
			skillFilter.Skill.Should().Be.EqualTo(skill);

			var dayOffRules = DayOffRulesRepository.LoadAllByAgentGroup(inDb);
			dayOffRules.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotAddExistingTeamFilter()
		{
			var team = new Team().WithId();
			TeamRepository.Add(team);

			var model = new AgentGroupModel();
			model.Filters.Add(new FilterModel { Id = team.Id.Value, FilterType = "team" });
			model.Filters.Add(new FilterModel { Id = team.Id.Value, FilterType = "team" });

			Target.Persist(model);

			var inDb = AgentGroupRepository.LoadAll().Single();
			inDb.Filters.Count().Should().Be.EqualTo(1);

			var dayOffRules = DayOffRulesRepository.LoadAllByAgentGroup(inDb);
			dayOffRules.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotAddExistingContractFilter()
		{
			var contract = new Contract("_").WithId();
			ContractRepository.Add(contract);

			var model = new AgentGroupModel();
			model.Filters.Add(new FilterModel { Id = contract.Id.Value, FilterType = "contract" });
			model.Filters.Add(new FilterModel { Id = contract.Id.Value, FilterType = "contract" });

			Target.Persist(model);

			var inDb = AgentGroupRepository.LoadAll().Single();
			inDb.Filters.Count().Should().Be.EqualTo(1);

			var dayOffRules = DayOffRulesRepository.LoadAllByAgentGroup(inDb);
			dayOffRules.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotAddExistingSiteFilter()
		{
			var site = new Site("_").WithId();
			SiteRepository.Add(site);

			var model = new AgentGroupModel();
			model.Filters.Add(new FilterModel { Id = site.Id.Value, FilterType = "site" });
			model.Filters.Add(new FilterModel { Id = site.Id.Value, FilterType = "site" });

			Target.Persist(model);

			var inDb = AgentGroupRepository.LoadAll().Single();
			inDb.Filters.Count().Should().Be.EqualTo(1);

			var dayOffRules = DayOffRulesRepository.LoadAllByAgentGroup(inDb);
			dayOffRules.Should().Not.Be.Empty();
		}


		[Test]
		public void ShouldNotAddExistingSkillFilter()
		{
			var skill = new Skill("_").WithId();
			SkillRepository.Add(skill);

			var model = new AgentGroupModel();
			model.Filters.Add(new FilterModel { Id = skill.Id.Value, FilterType = "skill" });
			model.Filters.Add(new FilterModel { Id = skill.Id.Value, FilterType = "skill" });

			Target.Persist(model);

			var inDb = AgentGroupRepository.LoadAll().Single();
			inDb.Filters.Count().Should().Be.EqualTo(1);

			var dayOffRules = DayOffRulesRepository.LoadAllByAgentGroup(inDb);
			dayOffRules.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldThrowIfUnknownFilter()
		{
			var model = new AgentGroupModel();
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
			var existing = new AgentGroup()
				.WithId()
				.AddFilter(new ContractFilter(contract));
			var team = new Team().WithId();

			AgentGroupRepository.Add(existing);
			ContractRepository.Add(contract);
			TeamRepository.Add(team);

			var model = new AgentGroupModel
			{
				Id = existing.Id.Value,
				Filters = new List<FilterModel> { new FilterModel { FilterType = FilterModel.TeamFilterType, Name = team.Description.Name, Id = team.Id.Value } }
			};

			Target.Persist(model);

			var onlyFilterInDb = (TeamFilter)AgentGroupRepository.LoadAll().Single().Filters.Single();
			onlyFilterInDb.Team.Id.Value.Should().Be.EqualTo(team.Id.Value);
		}

		[Test]
		public void ShouldInsertName()
		{
			var model = new AgentGroupModel();
			var expectedName = RandomName.Make();
			model.Name = expectedName;

			Target.Persist(model);
			var inDb = AgentGroupRepository.LoadAll().Single();
			inDb.Name.Should().Be.EqualTo(expectedName);
		}

		[Test]
		public void ShouldRemoveAgentGroupAndRemoveDayOffRules()
		{
			var existing = new AgentGroup()
				.WithId();
			AgentGroupRepository.Add(existing);

			Target.Delete(existing.Id.GetValueOrDefault());

			var inDb = AgentGroupRepository.Get(existing.Id.GetValueOrDefault());
			inDb.Should().Not.Be.Null();
			((IDeleteTag)inDb).IsDeleted.Should().Be.True();

			var dayOffRules = DayOffRulesRepository.LoadAllByAgentGroup(inDb);
			dayOffRules.Should().Be.Empty();
		}
	}
}