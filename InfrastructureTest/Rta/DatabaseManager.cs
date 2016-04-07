using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using BusinessUnit = Teleopti.Ccc.Domain.Common.BusinessUnit;
using Person = Teleopti.Ccc.Domain.Common.Person;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	/// <summary>
	/// ;-)
	/// </summary>
	public class DatabaseManager
	{
		private const int datasourceId = 9;

		private readonly IPersonRepository _persons;
		private readonly IContractRepository _contracts;
		private readonly IPartTimePercentageRepository _partTimePercentages;
		private readonly IContractScheduleRepository _contractSchedules;
		private readonly IExternalLogOnRepository _externalLogOns;
		private readonly ITeamRepository _teams;
		private readonly ISiteRepository _sites;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IRtaStateGroupRepository _stateGroups;
		private readonly IRtaRuleRepository _rules;
		private readonly IRtaMapRepository _mappings;

		public DatabaseManager(
			IPersonRepository persons,
			IContractRepository contracts,
			IPartTimePercentageRepository partTimePercentages,
			IContractScheduleRepository contractSchedules,
			IExternalLogOnRepository externalLogOns,
			ITeamRepository teams,
			ISiteRepository sites,
			IBusinessUnitRepository businessUnits,
			IRtaStateGroupRepository stateGroups,
			IRtaRuleRepository rules,
			IRtaMapRepository mappings)
		{
			_persons = persons;
			_contracts = contracts;
			_partTimePercentages = partTimePercentages;
			_contractSchedules = contractSchedules;
			_externalLogOns = externalLogOns;
			_teams = teams;
			_sites = sites;
			_businessUnits = businessUnits;
			_stateGroups = stateGroups;
			_rules = rules;
			_mappings = mappings;
		}

		[UnitOfWork]
		public virtual DatabaseManager WithStateGroup(string name)
		{
			var stateGroup = new RtaStateGroup(name, true, true);
			stateGroup.AddState(name, Guid.Empty);
			_stateGroups.Add(stateGroup);
			return this;
		}
		
		[UnitOfWork]
		public virtual DatabaseManager WithAgent(string externalLogOn)
		{
			var site = _sites.LoadAll().SingleOrDefault(x => x.Description.Name == "site");
			if (site == null)
			{
				site = new Site("site");
				_sites.Add(site);
			}
			var team = _teams.LoadAll().SingleOrDefault(x => x.Description.Name == "team");
			if (team == null)
			{
				team = new Team
				{
					Description = new Description("team"),
					Site = site
				};
				_teams.Add(team);
			}
			
			var businessUnit = _businessUnits.LoadAll().SingleOrDefault(x => x.Description.Name == "businessUnit");
			if (businessUnit == null)
			{
				businessUnit = new BusinessUnit("businessUnit");
				businessUnit.AddSite(site);
				_businessUnits.Add(businessUnit);
			}

			var contract = getOrAdd(_contracts, "contract", () =>
			{
				var c = new Contract("contract");
				_contracts.Add(c);
				return c;
			});

			var partTimePercentage = getOrAdd(_partTimePercentages, "partTimePercentage", () =>
			{
				var p = new PartTimePercentage("partTimePercentage");
				_partTimePercentages.Add(p);
				return p;
			});

			var contractSchedule = getOrAdd(_contractSchedules, "contractSchedule", () =>
			{
				var cs = new ContractSchedule("contractSchedule");
				_contractSchedules.Add(cs);
				return cs;
			});

			var personContract = new PersonContract(
				contract,
				partTimePercentage,
				contractSchedule);
			
			var personPeriod = new PersonPeriod("2000-01-01".Date(),
				personContract,
				team);

			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person.Name = new Name(externalLogOn, externalLogOn);
			person.AddPersonPeriod(personPeriod);

			var exteralLogOn = new ExternalLogOn
			{
				AcdLogOnName = externalLogOn, // is not used?
				DataSourceId = datasourceId,
				AcdLogOnOriginalId = externalLogOn // this is what the rta receives
			};
			_externalLogOns.Add(exteralLogOn);
			person.AddExternalLogOn(exteralLogOn, personPeriod);

			_persons.Add(person);

			return this;
		}

		private static T getOrAdd<T>(IRepository<T> loadAggregates, string name, Func<T> createAction)
			where T : IAggregateRoot
		{
			var all = loadAggregates.LoadAll();
			var existing = all.SingleOrDefault(x => ((dynamic)x).Description.Name == name);
			if (existing == null)
				return createAction();
			return existing;
		}

		[UnitOfWork]
		public virtual DatabaseManager WithRule(string name, Adherence adherence)
		{
			var rule = new RtaRule(new Description(name), Color.Brown, TimeSpan.Zero, 0.0);
			_rules.Add(rule);
			return this;
		}

		[UnitOfWork]
		public virtual DatabaseManager WithMapping(string stateGroupName, string ruleName)
		{
			var stateGroup = _stateGroups.LoadAll().Single(x => x.Name == stateGroupName);
			var rule = _rules.LoadAll().Single(x => x.Description.Name == ruleName);
			var mapping = new RtaMap(stateGroup, null)
			{
				RtaRule = rule
			};
			_mappings.Add(mapping);
			return this;
		}

		public DatabaseManager WithDataSource(string dataSourceId)
		{
			var datasource = new Datasources(datasourceId, " ", -1, " ", -1, " ", " ", 1, false, dataSourceId, false);
			new AnalyticsDataFactory().Apply(datasource);
			return this;
		}

	}
}