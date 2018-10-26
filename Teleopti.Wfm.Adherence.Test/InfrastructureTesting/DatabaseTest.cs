﻿using System.Collections.Generic;
using NHibernate;
using NUnit.Framework;
//using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Test;

namespace Teleopti.Ccc.InfrastructureTest.Helper
{
    public abstract class DatabaseTest : INotCompatibleWithIoCTest
	{
	    private ISession _session;
	    private IPerson _loggedOnPerson;
	    private IUnitOfWork _unitOfWork;
	    //private SetupFixtureForAssembly.TestScope _loginWithOpenUnitOfWork;

	    protected ISession Session => _session;
		protected IPerson LoggedOnPerson => _loggedOnPerson;
		protected IUnitOfWork UnitOfWork => _unitOfWork;
		protected ICurrentUnitOfWork CurrUnitOfWork => new ThisUnitOfWork(_unitOfWork);
		//protected MockRepository Mocks { get; private set; }

        [SetUp]
        public void Setup()
        {
//			Mocks = new MockRepository();
			InfrastructureTestStuff.BeforeWithLogon(out _loggedOnPerson);
			_unitOfWork = InfrastructureTestStuff.DataSource.Application.CreateAndOpenUnitOfWork();
//	        _loginWithOpenUnitOfWork = SetupFixtureForAssembly.CreatePersonAndLoginWithOpenUnitOfWork(out _loggedOnPerson, out _unitOfWork);
//	        _loginWithOpenUnitOfWork.CleanUpAfterTest = false;
			_session = _unitOfWork.FetchSession();
			SetupForRepositoryTest();
        }

        protected virtual void SetupForRepositoryTest(){}

        [TearDown]
        public void BaseTeardown()
        {
			_unitOfWork.Dispose();
			_unitOfWork = null;
			InfrastructureTestStuff.AfterWithLogon();
//			_loginWithOpenUnitOfWork.Teardown();
		}

		protected void PersistAndRemoveFromUnitOfWork(IEntity obj)
        {
            Session.SaveOrUpdate(obj);
            Session.Flush();
            Session.Evict(obj);
        }
		
//
//        protected void PersistAndRemoveFromUnitOfWork<T>(IEnumerable<T> persistList) where T : IEntity
//        {
//            foreach (var obj in persistList)
//            {
//                PersistAndRemoveFromUnitOfWork(obj);
//            }
//        }
//
//        protected void CleanUpAfterTest()
//        {
//			_loginWithOpenUnitOfWork.CleanUpAfterTest = true;
//		}
//
	    protected void Logout()
		{
			InfrastructureTestStuff.Logout();
		}
    }
}
