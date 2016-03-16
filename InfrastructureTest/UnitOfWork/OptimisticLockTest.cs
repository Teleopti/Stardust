using System;
using NHibernate.Cfg;
using NHibernate.Mapping;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    /// <summary>
    /// Tests for optimistic lock functionality
    /// </summary>
    [TestFixture, Category("LongRunning")]
    public class OptimisticLockTest
    {
        private MockRepository mocks;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [Test]
        public void VerifyHasVersionSuitsWithMapping()
        {
            Configuration appCfg = new Configuration()
                                .SetProperties(SetupFixtureForAssembly.Sql2005conf("doesnotmatter", null))
                                .AddAssembly(typeof(Person).Assembly);
            foreach (PersistentClass mapping in appCfg.ClassMappings)
            {
                Type entityType = mapping.MappedClass;
            }
        }

        /// <summary>
        /// Verifies the optimistic concurrency.
        /// Creates new IUnitOfWorks because the one
        /// shared among all tests is bound to a transaction.
        /// </summary>
        /// <remarks>
        /// Unfortunatly creates an User i db outside transaction.
        /// So far no problem - but have in mind!
        /// </remarks>
        [Test]
        [ExpectedException(typeof (OptimisticLockException))]
        public void VerifyOptimisticConcurrency()
        {
            try
            {
                IPerson user = PersonFactory.CreatePerson();
                user.Name = new Name("sdfsdf", "df");

							StateHolderProxyHelper.SetupFakeState(SetupFixtureForAssembly.DataSource, user, BusinessUnitFactory.BusinessUnitUsedInTest, StateHolderProxyHelper.DefaultPrincipalContext);
							
                IUnitOfWork uow1 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
                IUnitOfWork uow2 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
                IPersonRepository rep1 = new PersonRepository(new ThisUnitOfWork(uow1));
                IPersonRepository rep2 = new PersonRepository(new ThisUnitOfWork(uow2));
                mocks.ReplayAll();

                //save
                rep1.Add(user);
                uow1.PersistAll();

                //load
                IPerson user2 = rep2.Load(user.Id.Value);

                //update
                user.Name = new Name("sdfsdf", "nytt1");
                user2.Name = new Name("fff", "nytt2");


                //flush
                uow1.PersistAll();
                uow2.PersistAll();

                uow1.Dispose();
                uow2.Dispose();
                
            }

            finally
            {
                cleanUp();                
            }
        }


        /// <summary>
        /// Verifies that optimistic concurrency works as expected in deep graph.
        /// </summary>
        [Test]
        [ExpectedException(typeof(OptimisticLockException))]
        public void VerifyOptimisticConcurrencyInDeepGraph()
        {
            try
            {
                IPerson user = PersonFactory.CreatePerson("sdfd232sg");

							StateHolderProxyHelper.SetupFakeState(SetupFixtureForAssembly.DataSource, user, BusinessUnitFactory.BusinessUnitUsedInTest, StateHolderProxyHelper.DefaultPrincipalContext);

                IUnitOfWork uow1 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
                IUnitOfWork uow2 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
                IPersonRepository rep1 = new PersonRepository(new ThisUnitOfWork(uow1));
                IPersonRepository rep2 = new PersonRepository(new ThisUnitOfWork(uow2));
                mocks.ReplayAll();

                //save
                rep1.Add(user);
                uow1.PersistAll();

                //load
                IPerson user2 = rep2.Load(user.Id.Value);

                //update
	            user.Name = new Name(RandomName.Make(), RandomName.Make());
							user2.Name = new Name(RandomName.Make(), RandomName.Make());


                //flush
                uow1.PersistAll();
                uow2.PersistAll();

                uow1.Dispose();
                uow2.Dispose();   
            }
            finally
            {
                cleanUp();                
            }
        }


        private void cleanUp()
        {
            using (IUnitOfWork uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
            {
                PersonRepository rep = new PersonRepository(new ThisUnitOfWork(uow));
                foreach (IPerson person in rep.LoadAll())
                {
                    rep.Remove(person);
                }
                uow.PersistAll();
            }

            mocks.VerifyAll();
        }

    }
}