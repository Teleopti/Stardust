using System.Collections.Generic;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Helper
{
    public abstract class DatabaseTest
    {
        private bool cleanUpAfterTest;
	    private ISession _session;
	    private IPerson _loggedOnPerson;
	    private IUnitOfWork _unitOfWork;

	    protected ISession Session
	    {
		    get { return _session; }
	    }

	    protected IPerson LoggedOnPerson
	    {
		    get { return _loggedOnPerson; }
	    }

	    protected IUnitOfWork UnitOfWork
	    {
		    get { return _unitOfWork; }
	    }

	    protected MockRepository Mocks { get; private set; }

        [SetUp]
        public void Setup()
        {
			Mocks = new MockRepository();
	        cleanUpAfterTest = false;
			SetupFixtureForAssembly.BeforeTestWithOpenUnitOfWork(out _loggedOnPerson, out _unitOfWork);
			_session = _unitOfWork.FetchSession();
			SetupForRepositoryTest();
        }

        protected virtual void SetupForRepositoryTest(){}

        [TearDown]
        public void BaseTeardown()
        {
			SetupFixtureForAssembly.AfterTestWithOpenUnitOfWork(UnitOfWork, cleanUpAfterTest);
		}

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

        protected void CleanUpAfterTest()
        {
            cleanUpAfterTest = true;
        }

	    protected void Logout()
	    {
			SetupFixtureForAssembly.ClearFakeState();
		}
    }
}
