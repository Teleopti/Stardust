﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    //complement test fixture for nhibernateunitofwork
    //going to a physical db (not mocks)

    [TestFixture]
    [Category("BucketB")]
	[DatabaseTest]
    public class NHibernateUnitOfWorkRealTest
    {
        [Test]
        public void VerifyFlush()
        {
	        using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
	        {
		        var session = uow.FetchSession();
				session.SessionFactory.Statistics.Clear();
				var p = PersonFactory.CreatePerson();
				new PersonRepository(new ThisUnitOfWork(uow)).Add(p);
				Assert.IsTrue(uow.IsDirty());
				uow.Flush();
				Assert.IsFalse(uow.IsDirty());
				Assert.AreEqual(2, session.SessionFactory.Statistics.EntityInsertCount);
			}
        }

        [Test]
        public void VerifyImplicitTransaction()
        {
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
		public void ShouldIncludeTypeInMergingTransientRootErrorMessage()
		{
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var person = new Person();
				var ex = Assert.Throws<DataSourceException>(() =>uow.Merge(person));
				ex.Message.Should().Contain(".Person");
			}
		}
	}
}
