using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class PersonAccountUpdaterForWinTest
	{
		private PersonAccountUpdaterWin _target;
		private MockRepository _mocks;
		private IPeopleAccountUpdaterProvider _provider;
		private IPerson _person;
		private IAbsence _absence1;
		private IAbsence _absence2;
		private IUnitOfWork _unitOfWork;
		private ITraceableRefreshService _refreshService;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_provider = _mocks.Stub<IPeopleAccountUpdaterProvider>();
			_person = PersonFactory.CreatePerson();
			_absence1 = new Absence();
			_absence2 = new Absence();
			_unitOfWork = _mocks.Stub<IUnitOfWork>();
			_refreshService = _mocks.StrictMock<ITraceableRefreshService>();
			_target = new PersonAccountUpdaterWin(_provider);
		}

		[Test]
		public void ShouldUpdateAllPersonAccounts()
		{
			AccountDay account1;
			AccountTime account2;
			var accountCollection = PersonAccountCollectionFactory.Create(_person, _absence1, _absence2, out account1, out account2);

			using (_mocks.Record())
			{
				_provider.Stub(p => p.GetPersonAccounts(_person))
					.Return(accountCollection);
				_provider.Stub(p => p.GetUnitOfWork)
					.Return(_unitOfWork);
				_provider.Stub(p => p.GetRefreshService())
				         .Return(_refreshService);
				
				// must be called exactly once for each account
				Expect.Call(() => _refreshService.Refresh(account1, _unitOfWork))
				        .Repeat.Once();
				Expect.Call(() => _refreshService.Refresh(account2, _unitOfWork))
				        .Repeat.Once();
			}

			using (_mocks.Playback())
			{
				_target.Update(_person);
			}
		}

	}
}
