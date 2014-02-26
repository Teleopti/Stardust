using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Grouping
{
	[TestFixture]
	public class SchedulerGroupPagesProviderTest
	{
		private MockRepository _mocks;
		private SchedulerGroupPagesProvider _target;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_personSelectorReadOnlyRepository = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();

			_target = new SchedulerGroupPagesProvider(_unitOfWorkFactory,_personSelectorReadOnlyRepository);
		}

		[Test]
		public void ShouldProvideBuiltInWithoutSkill()
		{
			var uow = _mocks.StrictMock<IUnitOfWork>();
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(_personSelectorReadOnlyRepository.GetUserDefinedTabs()).Return(new List<IUserDefinedTabLight>());
			Expect.Call(uow.Dispose);
			_mocks.ReplayAll();
			var groups = _target.GetGroups(false);
			Assert.That(groups.Count, Is.EqualTo(6));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldProvideBuiltInWithSkill()
		{
			var uow = _mocks.StrictMock<IUnitOfWork>();
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(_personSelectorReadOnlyRepository.GetUserDefinedTabs()).Return(new List<IUserDefinedTabLight>());
			Expect.Call(uow.Dispose);
			_mocks.ReplayAll();
			var groups = _target.GetGroups(true);
			Assert.That(groups.Count, Is.EqualTo(7));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldProvideBuiltInAndUserDefined()
		{
			var uow = _mocks.StrictMock<IUnitOfWork>();
			IList<IUserDefinedTabLight> lst = new List<IUserDefinedTabLight> { new UserDefinedTabLight { Id = Guid.NewGuid(), Name = "UserDefined" } };
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(_personSelectorReadOnlyRepository.GetUserDefinedTabs()).Return(lst);
			Expect.Call(uow.Dispose);
			_mocks.ReplayAll();
			var groups = _target.GetGroups(false);
			Assert.That(groups.Count, Is.EqualTo(7));
			_mocks.VerifyAll();
		}
	}

	
}