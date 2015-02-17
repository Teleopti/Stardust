using System;
using System.Collections.Generic;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Helper
{
    public abstract class DatabaseTest
    {
        private bool skipRollback;
    	
    	protected ISession Session { get; private set; }
    	protected IPerson LoggedOnPerson { get; private set; }
    	protected IUnitOfWork UnitOfWork { get; private set; }
    	protected MockRepository Mocks { get; private set; }

        [SetUp]
        public void Setup()
        {
            BusinessUnitFactory.SetBusinessUnitUsedInTestToNull();
            skipRollback = false;

            Mocks = new MockRepository();
            var stateMock = Mocks.StrictMock<IState>();

            Guid buGuid = Guid.NewGuid();
            BusinessUnitFactory.BusinessUnitUsedInTest.SetId(buGuid);
            LoggedOnPerson = PersonFactory.CreatePersonWithBasicPermissionInfo(string.Concat("logOnName", Guid.NewGuid().ToString()), string.Empty);

            StateHolderProxyHelper.ClearAndSetStateHolder(Mocks,
                                                     LoggedOnPerson,
                                                     BusinessUnitFactory.BusinessUnitUsedInTest,
                                                     SetupFixtureForAssembly.ApplicationData,
																										 SetupFixtureForAssembly.DataSource,
                                                     stateMock);

            UnitOfWork = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
	        Session = UnitOfWork.FetchSession();

            ((IDeleteTag)LoggedOnPerson).SetDeleted();
            Session.Save(LoggedOnPerson);

            //force a insert
            BusinessUnitFactory.BusinessUnitUsedInTest.SetId(null);
            Session.Save(BusinessUnitFactory.BusinessUnitUsedInTest, buGuid);
            Session.Flush();

            
            SetupForRepositoryTest();
        }

        protected virtual void SetupForRepositoryTest(){}


        /// <summary>
        /// Runs after each test.
        /// </summary>
        [TearDown]
        public void BaseTeardown()
        {
            UnitOfWork.Dispose();
            if (skipRollback)
            {
                if(BusinessUnitFactory.BusinessUnitUsedInTest.Id.HasValue)
                {
                    using (IUnitOfWork uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
                    {
                        IBusinessUnitRepository buRep = new BusinessUnitRepository(uow);
                        buRep.Remove(BusinessUnitFactory.BusinessUnitUsedInTest);
                        uow.PersistAll();
                    }                    
                }
            }
            TeardownForRepositoryTest();
        }

        protected virtual void TeardownForRepositoryTest(){}


        protected void PersistAndRemoveFromUnitOfWork(IEntity obj)
        {
            Session.SaveOrUpdate(obj);
            Session.Flush();
            Session.Evict(obj);
        }

        protected void PersistAndRemoveFromUnitOfWork<T>(IEnumerable<T> persistList) where T : IEntity
        {
            foreach (var obj in persistList)
            {
                PersistAndRemoveFromUnitOfWork(obj);
            }
        }

        /// <summary>
        /// Skips the rollback and commit the transaction manually instead.
        /// </summary>
        protected void SkipRollback()
        {
            skipRollback = true;
        }
    }
}
