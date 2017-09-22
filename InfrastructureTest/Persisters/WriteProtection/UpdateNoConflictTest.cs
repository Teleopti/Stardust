using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Persisters.WriteProtection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.WriteProtection
{
	//conflicts are not handled
	[TestFixture]
	[DatabaseTest]
	public class UpdateNoConflictTest
	{
		private IPersonWriteProtectionInfo writeProtection;
		private IWriteProtectionPersister target;
		private IWriteProtectionRepository repository;

		[Test]
		public void ShouldUpdateWriteProtection()
		{
			var newDate = new DateOnly(2010, 1, 1);
			writeProtection.PersonWriteProtectedDate = newDate;
			var writeProtectionsToSave = new List<IPersonWriteProtectionInfo> {writeProtection};

			target.Persist(writeProtectionsToSave);


			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.AreEqual(repository.Load(writeProtection.Id.Value).PersonWriteProtectedDate, newDate);
			}
		}

		[Test]
		public void ShouldClearSaveList()
		{
			var writeProtectionsToSave = new List<IPersonWriteProtectionInfo> { writeProtection };
			target.Persist(writeProtectionsToSave);

			writeProtectionsToSave.Should().Be.Empty();
		}

		[SetUp]
		protected void Setup()
		{
			//something is really wrong with the writeprotection mapping
			repository = new WriteProtectionRepository(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()));
			target = new WriteProtectionPersister(CurrentUnitOfWorkFactory.Make(), repository, MockRepository.GenerateMock<IInitiatorIdentifier>());
			var person = PersonFactory.CreatePerson("persist test");
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2000,1,1);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonRepository(new ThisUnitOfWork(uow));
				rep.Add(person);
				uow.PersistAll();
			}
			writeProtection = person.PersonWriteProtection;
		}

	}
}