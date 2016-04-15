using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	public class DatabaseVersionTest : DatabaseTest
	{
		[Test]
		public void VerifyDatabaseVersionOnExistingRoot()
		{
			CleanUpAfterTest();
			var p = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(p);
			UnitOfWork.PersistAll();

			new DatabaseVersion(new ThisUnitOfWork(UnitOfWork)).FetchFor(p, false)
				.Should().Be.GreaterThan(0);
		}

		[Test]
		public void VerifyDatabaseVersionOnExistingRootUsingPessimisticLock()
		{
			//does not verify that a pess lock is created, just that the method works logically
			//the lock itself will be tested in the use cases where needed
			CleanUpAfterTest();
			var p = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(p);
			UnitOfWork.PersistAll();

			new DatabaseVersion(new ThisUnitOfWork(UnitOfWork)).FetchFor(p, true)
				.Should().Be.GreaterThan(0);
		}


		[Test]
		public void VerifyDatabaseVersionOnNonDatabaseExistingRoot()
		{
			var p = PersonFactory.CreatePerson();
			p.SetId(Guid.NewGuid());
			new DatabaseVersion(new ThisUnitOfWork(UnitOfWork)).FetchFor(p, false)
				.Should().Not.Have.Value();
		}

		[Test]
		public void VerifyDatabaseVersionOnTransientRoot()
		{
			Assert.Throws<ArgumentException>(() => new DatabaseVersion(new ThisUnitOfWork(UnitOfWork)).FetchFor(PersonFactory.CreatePerson(), false));
		}

		[Test]
		public void VerifyDatabaseVersionOnProxy()
		{
			CleanUpAfterTest();
			var p = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(p);
			UnitOfWork.PersistAll();

			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var pProxy = new PersonRepository(new ThisUnitOfWork(uow)).Load(p.Id.Value);
				new DatabaseVersion(new ThisUnitOfWork(uow)).FetchFor(pProxy, false)
					.Should().Be.GreaterThan(0);
			}
		}
	}
}