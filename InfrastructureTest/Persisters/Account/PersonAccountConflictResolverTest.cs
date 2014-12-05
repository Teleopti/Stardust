using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	[TestFixture]
	public class PersonAccountConflictResolverTest
	{
		private MockRepository _mocks;
		private IPersonAccountConflictResolver _target;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private ITraceableRefreshService _traceableRefreshService;
		private IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private IPersonAbsenceAccount _personAbsenceAccount;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _unitOfWork;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_currentUnitOfWorkFactory = _mocks.StrictMock<ICurrentUnitOfWorkFactory>();
			_traceableRefreshService = _mocks.StrictMock<ITraceableRefreshService>();
			_personAbsenceAccountRepository = _mocks.StrictMock<IPersonAbsenceAccountRepository>();
			_target = new PersonAccountConflictResolver(_currentUnitOfWorkFactory, _traceableRefreshService, _personAbsenceAccountRepository);
			_personAbsenceAccount = new PersonAbsenceAccount(new Person(), new Absence());
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_unitOfWork = _mocks.StrictMock<IUnitOfWork>();
		}

		[Test]
		public void ShouldHandleDeletedPersonAccounts()
		{
			using (_mocks.Record())
			{
				Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
				Expect.Call(_unitOfWorkFactory.CurrentUnitOfWork()).Return(_unitOfWork);
				Expect.Call(_personAbsenceAccountRepository.Get(Guid.Empty)).Return(null); //skip this account
			}

			using (_mocks.Playback())
			{
				_target.Resolve(new List<IPersonAbsenceAccount> { _personAbsenceAccount });
			}
		}
	}
}