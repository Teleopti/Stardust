using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[DatabaseTest]
	public class OptimisticLockTest
	{
		public ICurrentUnitOfWorkFactory UnitOfWorkFactory;

		[Test]
		public void ShouldThrowOptimisticLockException()
		{
			Guid id;

			//client 1
			var uow1 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			var user1 = PersonFactory.CreatePerson();
			user1.Name = new Name("new", "name");
			new PersonRepository(new ThisUnitOfWork(uow1)).Add(user1);
			uow1.PersistAll();
			id = user1.Id.Value;

			//client 2
			var uow2 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			var user2 = new PersonRepository(new ThisUnitOfWork(uow2)).Load(id);
			user2.Name = new Name("change", "name");
			uow2.PersistAll();
			uow2.Dispose();

			//client 1
			user1.Name = new Name("change", "too");
			Assert.Throws<OptimisticLockException>(() => { uow1.PersistAll(); });
			uow1.Dispose();
		}

		[Test]
		public void ShouldThrowOptimisticLockExceptionWithDeepGraph()
		{
			Guid id;

			//setup
			var uow1 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			var site = SiteFactory.CreateSimpleSite();
			new SiteRepository(new ThisUnitOfWork(uow1)).Add(site);
			uow1.PersistAll();
			var team = TeamFactory.CreateSimpleTeam(".");
			team.Site = site;
			new TeamRepository(new ThisUnitOfWork(uow1)).Add(team);
			uow1.PersistAll();
			var partTimePercentage = new PartTimePercentage(".");
			new PartTimePercentageRepository(new ThisUnitOfWork(uow1)).Add(partTimePercentage);
			uow1.PersistAll();
			var contract = new Contract(".");
			new ContractRepository(new ThisUnitOfWork(uow1)).Add(contract);
			uow1.PersistAll();
			var contratSchedule = new ContractSchedule(".");
			new ContractScheduleRepository(new ThisUnitOfWork(uow1)).Add(contratSchedule);
			uow1.PersistAll();

			//client 1
			var user1 = PersonFactory.CreatePerson();
			var period = PersonPeriodFactory.CreatePersonPeriod("2016-04-13".Date());
			period.PersonContract = new PersonContract(contract, partTimePercentage, contratSchedule);
			period.Team = team;
			user1.AddPersonPeriod(period);
			new PersonRepository(new ThisUnitOfWork(uow1)).Add(user1);
			uow1.PersistAll();
			id = user1.Id.Value;

			//client 2
			var uow2 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			var user2 = new PersonRepository(new ThisUnitOfWork(uow2)).Load(id);
			user2.Period("2016-04-13".Date()).Note = "a note";
			uow2.PersistAll();
			uow2.Dispose();

			//client 1
			user1.Period("2016-04-13".Date()).Note = "another note";
			Assert.Throws<OptimisticLockException>(() => { uow1.PersistAll(); });
			uow1.Dispose();
		}
	}
}