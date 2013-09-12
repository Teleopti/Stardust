using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Generated;
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
		private PersonAccountUpdaterForWin _target;
		private MockRepository _mocks;
		private IPeopleAccountUpdaterInteraction _interaction;
		private IPerson _person;
		private IAbsence _absence1;
		private IAbsence _absence2;
		private IUnitOfWork _unitOfWork;
		private ITraceableRefreshService _refreshService;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_interaction = _mocks.Stub<IPeopleAccountUpdaterInteraction>();
			_person = PersonFactory.CreatePerson();
			_absence1 = new Absence();
			_absence2 = new Absence();
			_unitOfWork = _mocks.Stub<IUnitOfWork>();
			_refreshService = _mocks.StrictMock<ITraceableRefreshService>();
			_target = new PersonAccountUpdaterForWin(_interaction);
		}

		[Test]
		public void ShouldUpdateAllPersonAccounts()
		{
			AccountDay account1;
			AccountTime account2;
			var accountCollection = PersonAccountCollectionFactory.Create(_person, _absence1, _absence2, out account1, out account2);

			using (_mocks.Record())
			{
				_interaction.Stub(p => p.PersonAccounts(_person))
					.Return(accountCollection);
				_interaction.Stub(p => p.UnitOfWork)
					.Return(_unitOfWork);
				_interaction.Stub(p => p.RefreshService)
				         .Return(_refreshService);
				
				// must be called exactly once for each account
				Expect.Call(() => _refreshService.Refresh(account1, _unitOfWork))
				        .Repeat.Once();
				Expect.Call(() => _refreshService.Refresh(account2, _unitOfWork))
				        .Repeat.Once();
			}

			using (_mocks.Playback())
			{
				_target.UpdateOnActivation(_person);
			}
		}

	}
}
