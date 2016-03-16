﻿using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Persisters.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Requests
{
	[TestFixture]
	public abstract class RequestPersisterBaseTest : DatabaseTestWithoutTransaction, IClearReferredShiftTradeRequests
	{
		protected IPerson Person { get; private set; }

		protected abstract IPersonRequest Given();
		protected abstract IPersonRequest When(IPersonRequest currentRequest);
		protected abstract void Then(IPersonRequest yourRequest);
		protected IPersonRequestRepository PersonRequestRepository { get; set; }
		protected IPrincipalAuthorization PrincipalAuthorization { get; set; }
		protected bool ClearRefferedRequestsWasCalled { get; private set; }

		private IRequestPersister target;
		private IPersonRequest requestToRemove;

		[Test]
		public void TheTest()
		{
			setState();
			var request = Given();
			makeTarget();
			var rep = new PersonRequestRepository(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()));
			if (request != null)
			{
				using (var givenUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					rep.Add(request);
					givenUow.PersistAll();
				}
			}

			target.Persist(new[] {When(request)});

			Then(request);
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Then(rep.Find(request.Id.Value));
			}

			requestToRemove = request;
		}

		private void setState()
		{
			PersonRequestRepository = new PersonRequestRepository(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()));
			PrincipalAuthorization = new PrincipalAuthorizationWithFullPermission();
		}

		private void makeTarget()
		{
			target = new RequestPersister(CurrentUnitOfWorkFactory.Make(), 
														PersonRequestRepository,
														this,
														MockRepository.GenerateMock<IInitiatorIdentifier>(),
														PrincipalAuthorization
														);
		}

		protected override void SetupForRepositoryTestWithoutTransaction()
		{
			Person = PersonFactory.CreatePerson("test person");
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonRepository(new ThisUnitOfWork(uow));
				rep.Add(Person);
				uow.PersistAll();
			}
		}

		public void ClearReferredShiftTradeRequests()
		{
			ClearRefferedRequestsWasCalled = true;
		}
	}
}