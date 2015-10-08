using System;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    //complement test fixture for nhibernateunitofwork
    //going to a physical db (not mocks)

    [TestFixture]
    [Category("LongRunning")]
    public class NHibernateUnitOfWorkRealTest : DatabaseTest
    {
        [Test]
        public void VerifyDatabaseVersionOnExistingRoot()
        {
            CleanUpAfterTest();
            var p = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(p);
            UnitOfWork.PersistAll();

            using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
            {
                Assert.Greater(uow.DatabaseVersion(p), 0);
            }

            removeFromDb(p);
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

					using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
					{
						Assert.Greater(uow.DatabaseVersion(p, true), 0);
					}

					removeFromDb(p);
				}


        [Test]
        public void VerifyDatabaseVersionOnNonDatabaseExistingRoot()
        {
            IPerson p = PersonFactory.CreatePerson();
            p.SetId(Guid.NewGuid());
            Assert.That(UnitOfWork.DatabaseVersion(p), Is.Null);
        }

        [Test]
        public void VerifyDatabaseVersionOnTransientRoot()
        {
            Assert.Throws<ArgumentException>(() => UnitOfWork.DatabaseVersion(PersonFactory.CreatePerson()));
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
                Assert.Greater(uow.DatabaseVersion(pProxy), 0);
            }

            removeFromDb(p);
        }

        [Test]
        public void VerifyFlush()
        {
            Session.SessionFactory.Statistics.Clear();
            var p = PersonFactory.CreatePerson();
            new PersonRepository(new ThisUnitOfWork(UnitOfWork)).Add(p);
            Assert.IsTrue(UnitOfWork.IsDirty());
            UnitOfWork.Flush();
            Assert.IsFalse(UnitOfWork.IsDirty());
            Assert.AreEqual(2, Session.SessionFactory.Statistics.EntityInsertCount);
        }

        [Test]
        public void VerifyImplicitTransaction()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();

            IPerson p;
            Guid id;
            using (var uow1 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
            {
                var repository = new PersonRepository(new ThisUnitOfWork(uow1));
                p = PersonFactory.CreatePerson();
                repository.Add(p);
                id = p.Id.Value;
                uow1.Flush();
            }

            using (var uow2 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
            {
                Assert.That(new PersonRepository(new ThisUnitOfWork(uow2)).Get(id), Is.Null);
            }
        }

        [Test]
        public void SetIdToNullToNewlyAddedRootsIfTranRollback()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();

            IPerson cantBePersisted;
            using (var uow1 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
            {
                var repository = new PersonRepository(new ThisUnitOfWork(uow1));
                cantBePersisted = PersonFactory.CreatePerson();
                repository.Add(cantBePersisted);
                cantBePersisted.Email = null;
                try
                {
                    uow1.PersistAll();
                }
                catch (DataSourceException)
                {
                }
            }

            Assert.IsTrue(SetupFixtureForAssembly.loggedOnPerson.Id.HasValue);
            Assert.IsFalse(cantBePersisted.Id.HasValue);
        }


        private void removeFromDb(IPerson person)
        {
            Session.Delete(person);
            Session.Flush();
        }
    }
}
