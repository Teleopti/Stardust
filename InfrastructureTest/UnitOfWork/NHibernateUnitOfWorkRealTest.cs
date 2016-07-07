﻿using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
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
