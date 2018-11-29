using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class SetSchedulePeriodWorktimeOverrideCommandHandlerTest
	{
		private MockRepository _mocks;
		private IPersonRepository _personRep;
		private SetSchedulePeriodWorktimeOverrideCommandHandler _target;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _uow;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_personRep = _mocks.DynamicMock<IPersonRepository>();
			_currentUnitOfWorkFactory = _mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			_unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
			_uow = _mocks.DynamicMock<IUnitOfWork>();
			_target = new SetSchedulePeriodWorktimeOverrideCommandHandler(_personRep, _currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetPersonFromRepository()
		{
			var id = Guid.NewGuid();
			var command = new SetSchedulePeriodWorktimeOverrideCommandDto { PersonId = id };
			Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_personRep.Get(id)).Return(null);

			_mocks.ReplayAll();
			_target.Handle(command);
			Assert.That(command.Result.AffectedItems, Is.EqualTo(0));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldGetPersonWithPeriodsFromRepository()
		{
			var person = _mocks.DynamicMock<IPerson>();
			var id = Guid.NewGuid();
			var command = new SetSchedulePeriodWorktimeOverrideCommandDto { PersonId = id, Date = new DateOnlyDto(2013, 6, 12) };
			Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_personRep.Get(id)).Return(person);
			Expect.Call(person.SchedulePeriod(new DateOnly(2013, 6, 12))).Return(null);
			_mocks.ReplayAll();

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(command);
			}

			Assert.That(command.Result.AffectedItems, Is.EqualTo(0));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSetPeriodWorkTimeOnPeriod()
		{
			var person = _mocks.DynamicMock<IPerson>();
			var id = Guid.NewGuid();
			var periodId = Guid.NewGuid();
			var workTime = 300;
			var period = _mocks.DynamicMock<ISchedulePeriod>();
			var command = new SetSchedulePeriodWorktimeOverrideCommandDto { PersonId = id, Date = new DateOnlyDto(2013, 6, 12), PeriodTimeInMinutes = workTime };
			Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_personRep.Get(id)).Return(person);
			Expect.Call(person.SchedulePeriod(new DateOnly(2013, 6, 12))).Return(period);
			Expect.Call(period.PeriodTime = TimeSpan.FromMinutes(workTime));
			Expect.Call(_uow.PersistAll()).Return(new List<IRootChangeInfo> {new RootChangeInfo()});
			Expect.Call(period.Id).Return(periodId);
			_mocks.ReplayAll();
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(command);
			}

			Assert.That(command.Result.AffectedItems, Is.EqualTo(1));
			Assert.That(command.Result.AffectedId, Is.EqualTo(periodId));
			_mocks.VerifyAll();
		}
	}

	

	
}