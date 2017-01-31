using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta.PerformanceMeasurement
{
	public class PerformanceMeasurementTestBase : InfrastructureTestWithOneTimeSetup
	{
		public AnalyticsDatabase Analytics;
		public WithUnitOfWork Uow;

		public IPersonRepository Persons;
		public IActivityRepository Activities;
		public IPersonAssignmentRepository PersonAssignments;
		public IScenarioRepository Scenarios;
		public IAgentStatePersister AgentState;
		public IContractRepository Contracts;
		public IPartTimePercentageRepository PartTimePercentages;
		public IContractScheduleRepository ContractSchedules;
		public IExternalLogOnRepository ExternalLogOns;
		public ITeamRepository Teams;
		public ISiteRepository Sites;

		public void MakeUsersFaster(IEnumerable<string> userCodes)
		{

			Uow.Do(() =>
			{
				var contract = new Contract(RandomName.Make());
				Contracts.Add(contract);

				var partTimePercentage = new PartTimePercentage(RandomName.Make());
				PartTimePercentages.Add(partTimePercentage);

				var contractSchedule = new ContractSchedule(RandomName.Make());
				ContractSchedules.Add(contractSchedule);

				var site = new Site(RandomName.Make());
				Sites.Add(site);

				var team = new Team {Site = site}
					.WithDescription(new Description(RandomName.Make()));
				Teams.Add(team);
				site.AddTeam(team);

				userCodes.ForEach(name =>
				{
					var person = new Person().WithName(new Name(name, name));
					person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

					var personContract = new PersonContract(
						contract,
						partTimePercentage,
						contractSchedule);

					var personPeriod = new PersonPeriod("2001-01-01".Date(), personContract, team);
					person.AddPersonPeriod(personPeriod);

					var exteralLogOn = new ExternalLogOn
					{
						//AcdLogOnName = name, // is not used?
						AcdLogOnMartId = -1, // NotDefined should be there, 0 probably wont
						DataSourceId = Analytics.CurrentDataSourceId,
						AcdLogOnOriginalId = name // this is what the rta receives
					};
					ExternalLogOns.Add(exteralLogOn);
					person.AddExternalLogOn(exteralLogOn, personPeriod);

					Persons.Add(person);
				});
			});
		}
	}
}