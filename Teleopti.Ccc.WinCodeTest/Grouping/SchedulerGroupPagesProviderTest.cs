using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.WinCodeTest.Grouping
{
	[TestFixture]
	public class SchedulerGroupPagesProviderTest
	{
		[Test]
		public void ShouldProvideBuiltInWithoutSkill()
		{
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var personSelectorReadOnlyRepository = MockRepository.GenerateMock<IPersonSelectorReadOnlyRepository>();

			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			personSelectorReadOnlyRepository.Stub(x => x.GetUserDefinedTabs()).Return(new List<IUserDefinedTabLight>());

			var target = new SchedulerGroupPagesProvider(currentUnitOfWorkFactory, personSelectorReadOnlyRepository);
			var groups = target.GetGroups(false);
			
			Assert.That(groups.Count, Is.EqualTo(6));
		}

		[Test]
		public void ShouldProvideBuiltInWithSkill()
		{
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var personSelectorReadOnlyRepository = MockRepository.GenerateMock<IPersonSelectorReadOnlyRepository>();

			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			personSelectorReadOnlyRepository.Stub(x => x.GetUserDefinedTabs()).Return(new List<IUserDefinedTabLight>());

			var target = new SchedulerGroupPagesProvider(currentUnitOfWorkFactory, personSelectorReadOnlyRepository);
			var groups = target.GetGroups(true);

			Assert.That(groups.Count, Is.EqualTo(7));
		}

		[Test]
		public void ShouldProvideBuiltInAndUserDefined()
		{
			IList<IUserDefinedTabLight> lst = new List<IUserDefinedTabLight> { new UserDefinedTabLight { Id = Guid.NewGuid(), Name = "UserDefined" } };
			
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var personSelectorReadOnlyRepository = MockRepository.GenerateMock<IPersonSelectorReadOnlyRepository>();

			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			personSelectorReadOnlyRepository.Stub(x => x.GetUserDefinedTabs()).Return(lst);

			var target = new SchedulerGroupPagesProvider(currentUnitOfWorkFactory, personSelectorReadOnlyRepository);
			var groups = target.GetGroups(false);
			Assert.That(groups.Count, Is.EqualTo(7));
		}
	}

	
}