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
		private IRepositoryFactory _repositoryFactory;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();

			_target = new SchedulerGroupPagesProvider(_unitOfWorkFactory,_repositoryFactory);
		}

		[Test]
		public void ShouldProvideBuiltInWithoutSkill()
		{
			var uow = _mocks.StrictMock<IStatelessUnitOfWork>();
			var rep = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
			Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(uow);
			Expect.Call(_repositoryFactory.CreatePersonSelectorReadOnlyRepository(uow)).Return(rep);
			Expect.Call(rep.GetUserDefinedTabs()).Return(new List<IUserDefinedTabLight>());
			Expect.Call(uow.Dispose);
			_mocks.ReplayAll();
			var groups = _target.GetGroups(false);
			Assert.That(groups.Count, Is.EqualTo(6));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldProvideBuiltInWithSkill()
		{
			var uow = _mocks.StrictMock<IStatelessUnitOfWork>();
			var rep = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
			Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(uow);
			Expect.Call(_repositoryFactory.CreatePersonSelectorReadOnlyRepository(uow)).Return(rep);
			Expect.Call(rep.GetUserDefinedTabs()).Return(new List<IUserDefinedTabLight>());
			Expect.Call(uow.Dispose);
			_mocks.ReplayAll();
			var groups = _target.GetGroups(true);
			Assert.That(groups.Count, Is.EqualTo(7));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldProvideBuiltInAndUserDefined()
		{
			var uow = _mocks.StrictMock<IStatelessUnitOfWork>();
			var rep = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
			IList<IUserDefinedTabLight> lst = new List<IUserDefinedTabLight> { new UserDefinedTabLight { Id = Guid.NewGuid(), Name = "UserDefined" } };
			Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(uow);
			Expect.Call(_repositoryFactory.CreatePersonSelectorReadOnlyRepository(uow)).Return(rep);
			Expect.Call(rep.GetUserDefinedTabs()).Return(lst);
			Expect.Call(uow.Dispose);
			_mocks.ReplayAll();
			var groups = _target.GetGroups(false);
			Assert.That(groups.Count, Is.EqualTo(7));
			_mocks.VerifyAll();
		}
	}

	
}