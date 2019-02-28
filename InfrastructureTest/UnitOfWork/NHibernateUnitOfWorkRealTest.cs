using System;
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
				using (session.SessionFactory.WithStats())
				{
					var p = PersonFactory.CreatePerson();
					PersonRepository.DONT_USE_CTOR(new ThisUnitOfWork(uow), null, null).Add(p);
					Assert.IsTrue(uow.IsDirty());
					uow.Flush();
					Assert.IsFalse(uow.IsDirty());
					Assert.AreEqual(2, session.SessionFactory.Statistics.EntityInsertCount);
				}
			}
        }

        [Test]
        public void VerifyImplicitTransaction()
        {
            IPerson p;
            Guid id;
            using (var uow1 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
            {
                var repository = PersonRepository.DONT_USE_CTOR(new ThisUnitOfWork(uow1), null, null);
                p = PersonFactory.CreatePerson();
                repository.Add(p);
                id = p.Id.Value;
                uow1.Flush();
            }

            using (var uow2 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
            {
                Assert.That(PersonRepository.DONT_USE_CTOR(new ThisUnitOfWork(uow2), null, null).Get(id), Is.Null);
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
