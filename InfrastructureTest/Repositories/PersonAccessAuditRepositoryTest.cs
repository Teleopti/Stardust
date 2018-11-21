using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class PersonAccessAuditRepositoryTest : RepositoryTest<IPersonAccess>
	{
		private IPerson _personActionOn;
		private IPersonAccess _personAccessBase;
		private IPersonAccessAuditRepository _personAccessAuditRepository;
		protected override void ConcreteSetup()
		{
			_personActionOn = PersonFactory.CreatePerson(new Name("Test1", "Test2"));
			PersistAndRemoveFromUnitOfWork(_personActionOn);
			_personAccessBase = new PersonAccess(LoggedOnPerson, _personActionOn, "RevokeRole", "Change", "{ \"Data\"=\"Some Json Data\" }", Guid.NewGuid());
			base.ConcreteSetup();
		}

		[Test]
		public void LoadAuditsShouldFilterOnPeriod()
		{
			_personAccessAuditRepository = new PersonAccessAuditRepository(new ThisUnitOfWork(UnitOfWork));

			var personAccessBefore = CreateAggregateWithCorrectBusinessUnit();
			personAccessBefore.TimeStamp = new DateTime(2018, 10, 21, 10, 00, 00, DateTimeKind.Utc);
			PersistAndRemoveFromUnitOfWork(personAccessBefore);

			var personAccess = CreateAggregateWithCorrectBusinessUnit();
			personAccess.TimeStamp = new DateTime(2018,10,22,10,00,00, DateTimeKind.Utc);
			PersistAndRemoveFromUnitOfWork(personAccess);

		var personAccess2 = CreateAggregateWithCorrectBusinessUnit();
			personAccess2.TimeStamp = new DateTime(2018, 10, 23, 10, 00, 00, DateTimeKind.Utc);
			PersistAndRemoveFromUnitOfWork(personAccess2);

			_personAccessAuditRepository.LoadAudits(LoggedOnPerson, new DateTime(2018, 10, 22), new DateTime(2018, 10, 22))
				.Single().Should().Be(personAccess);
		}

		[Test]
		public void LoadAuditsShouldFilterOnPerson()
		{
			_personAccessAuditRepository = new PersonAccessAuditRepository(new ThisUnitOfWork(UnitOfWork));

			var personAccess = CreateAggregateWithCorrectBusinessUnit();
			personAccess.TimeStamp = new DateTime(2018, 10, 22, 10, 00, 00, DateTimeKind.Utc);
			PersistAndRemoveFromUnitOfWork(personAccess);

			var _person2 = PersonFactory.CreatePerson(new Name("Test4", "Test5"));
			PersistAndRemoveFromUnitOfWork(_person2);
			var personAccess2 = CreateAggregateWithCorrectBusinessUnit();
			personAccess2.TimeStamp = new DateTime(2018, 10, 22, 10, 00, 00, DateTimeKind.Utc);
			personAccess2.ActionPerformedById = _person2.Id.GetValueOrDefault();
			PersistAndRemoveFromUnitOfWork(personAccess2);

			_personAccessAuditRepository.LoadAudits(_person2, new DateTime(2018, 10, 22), new DateTime(2018, 10, 22))
				.Single().Should().Be(personAccess2);
		}

		[Test]
		public void ShouldPurgeOldAudits()
		{
			var rep = new PersonAccessAuditRepository(CurrUnitOfWork);

			var personAccess1 = CreateAggregateWithCorrectBusinessUnit();

			var personAccess2 = CreateAggregateWithCorrectBusinessUnit();
			personAccess2.TimeStamp = DateTime.UtcNow.AddDays(-50);

			var personAccess3 = CreateAggregateWithCorrectBusinessUnit();
			personAccess3.TimeStamp = DateTime.UtcNow.AddDays(-100);

			PersistAndRemoveFromUnitOfWork(personAccess1);
			PersistAndRemoveFromUnitOfWork(personAccess2);
			PersistAndRemoveFromUnitOfWork(personAccess3);
			var audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-200), DateTime.UtcNow);
			audits.Count().Should().Be(3);

			rep.PurgeOldAudits(DateTime.UtcNow.AddDays(-60));

			audits = rep.LoadAudits(LoggedOnPerson, DateTime.UtcNow.AddDays(-200), DateTime.UtcNow);
			audits.Count().Should().Be(2);
		}

		protected override IPersonAccess CreateAggregateWithCorrectBusinessUnit()
		{
			return new PersonAccess(
				LoggedOnPerson,
				_personActionOn,
				_personAccessBase.Action, 
				_personAccessBase.ActionResult, 
				_personAccessBase.Data, 
				_personAccessBase.Correlation);
		}

		protected override Repository<IPersonAccess> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PersonAccessAuditRepository(currentUnitOfWork);
		}

		protected override void VerifyAggregateGraphProperties(IPersonAccess loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.ActionPerformedById.Should().Be.EqualTo(LoggedOnPerson.Id.GetValueOrDefault());
			loadedAggregateFromDatabase.ActionPerformedOn.Should().Be.EqualTo(_personAccessBase.ActionPerformedOn);
			loadedAggregateFromDatabase.Action.Should().Be.EqualTo(_personAccessBase.Action);
			loadedAggregateFromDatabase.ActionResult.Should().Be.EqualTo(_personAccessBase.ActionResult);
			loadedAggregateFromDatabase.Data.Should().Be.EqualTo(_personAccessBase.Data);
			loadedAggregateFromDatabase.Correlation.Should().Be.EqualTo(_personAccessBase.Correlation);
		}
	}
}
