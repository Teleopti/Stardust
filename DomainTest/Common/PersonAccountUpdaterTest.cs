using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class PersonAccountUpdaterTest
	{
		private PersonAccountUpdater _target;
		private MockRepository _mocks;
        private IPersonAbsenceAccountRepository _provider;
		private IPerson _person;
		private IAbsence _absence1;
		private IAbsence _absence2;
		private IUnitOfWork _unitOfWork;
		private ITraceableRefreshService _refreshService;
	    

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
            _provider = _mocks.Stub<IPersonAbsenceAccountRepository>();
            _person = PersonFactory.CreatePerson();
			_absence1 = new Absence();
			_absence2 = new Absence();
			_unitOfWork = _mocks.Stub<IUnitOfWork>();
			_refreshService = _mocks.StrictMock<ITraceableRefreshService>();
			_target = new PersonAccountUpdater(_provider, _refreshService);
		}

		[Test]
		public void ShouldUpdateAllPersonAccounts()
		{
			AccountDay account1;
			AccountTime account2;
			var accountCollection = PersonAccountCollectionFactory.Create(_person, _absence1, _absence2, out account1, out account2);
		    var absence1 = AbsenceFactory.CreateAbsence("Test Absence1");
            absence1.SetId(Guid.NewGuid());

		    using (_mocks.Record())
			{
                //_provider.Stub(p => p.GetPersonAccounts(_person))
                //    .Return(accountCollection);
                //_provider.Stub(p => p.GetUnitOfWork)
                //    .Return(_unitOfWork);
                //_provider.Stub(p => p.GetRefreshService())
                //         .Return(_refreshService);
				
				// must be called exactly once for each account

			    var personAccountCollection = new PersonAccountCollection(_person);
			    personAccountCollection.Add(absence1, account1);
                personAccountCollection.Add(absence1, account2);

                Expect.Call(_provider.Find(_person)).Return(personAccountCollection);
				Expect.Call(() => _refreshService.Refresh(account1))
				        .Repeat.Once();
                Expect.Call(() => _refreshService.Refresh(account2))
                        .Repeat.Once();
			}

			using (_mocks.Playback())
			{
				_target.Update(_person);
			}
		}

	}
}
