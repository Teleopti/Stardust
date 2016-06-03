﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class NumberOfAgentsInSiteReaderTest : DatabaseTest
	{
		[Test]
		public void ShouldLoadNumberOfAgentesForSite()
		{
			var team = TeamFactory.CreateTeam("t", "s");
			var personPeriod = createPersonPeriodAndPersistDependencies(team);

			var person = new Person();
			person.AddPersonPeriod(personPeriod);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);

			var person2 = new Person();
			person2.AddPersonPeriod(personPeriod.EntityClone());
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);

			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(person);
			PersistAndRemoveFromUnitOfWork(person2);

			var target = new NumberOfAgentsInSiteReader(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()), new Now());
			var result = target.FetchNumberOfAgents(new[] {team.Site});

			result[team.Site.Id.Value].Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnSiteWithNoAgents()
		{
			var team = TeamFactory.CreateTeam("t", "s");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var target = new NumberOfAgentsInSiteReader(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()), new Now());
			var result = target.FetchNumberOfAgents(new[] { team.Site });

			result[team.Site.Id.Value].Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotLoadTerminatedAgent()
		{
			var team = TeamFactory.CreateTeam("t", "s");
			var personPeriod = createPersonPeriodAndPersistDependencies(team);

			var person = new Person();
			person.AddPersonPeriod(personPeriod);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);

			var person2 = new Person();
			person2.AddPersonPeriod(personPeriod.EntityClone());
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			person2.TerminatePerson("2015-02-26".Date(), new PersonAccountUpdaterDummy());

			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(person);
			PersistAndRemoveFromUnitOfWork(person2);

			var target = new NumberOfAgentsInSiteReader(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()), new ThisIsNow("2015-02-26 08:00"));
			var result = target.FetchNumberOfAgents(new[] { team.Site });

			result[team.Site.Id.Value].Should().Be.EqualTo(1);
		}

		private PersonPeriod createPersonPeriodAndPersistDependencies(ITeam team)
		{
			var ptp = new PartTimePercentage("ptp");
			var contract = new Contract("c");
			var contractSchedule = new ContractSchedule("cs");
			var pp = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(contract, ptp, contractSchedule), team);

			PersistAndRemoveFromUnitOfWork(contractSchedule);
			PersistAndRemoveFromUnitOfWork(contract);
			PersistAndRemoveFromUnitOfWork(ptp);
			return pp;
		}
	}
}