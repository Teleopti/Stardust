using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
	[TestFixture]
	public class AvailableBusinessUnitsProviderTest
	{
		private IPerson person;
		private IPermissionInformation permissionInformation;

		[SetUp]
		public void Setup()
		{
			person = MockRepository.GenerateMock<IPerson>();
			permissionInformation = MockRepository.GenerateMock<IPermissionInformation>();
		}

		[Test]
		public void VerifyHasAccessToAllBusinessUnits()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();

			var target = new AvailableBusinessUnitsProvider(repositoryFactory);
			person.Stub(x => x.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
			permissionInformation.Stub(x => x.HasAccessToAllBusinessUnits()).Return(true);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			repositoryFactory.Stub(x => x.CreateBusinessUnitRepository(unitOfWork)).Return(businessUnitRepository);
			businessUnitRepository.Stub(x => x.LoadAllBusinessUnitSortedByName()).Return(new List<IBusinessUnit> { null });
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWork.Stub(x => x.Dispose());

			var result = target.AvailableBusinessUnits(person, dataSource);
			Assert.AreEqual(1, result.Count());

		}

		[Test]
		public void VerifyHasAccessToOneBusinessUnit()
		{
			var target = new AvailableBusinessUnitsProvider(null);

			person.Stub(x => x.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
			permissionInformation.Stub(x => x.HasAccessToAllBusinessUnits()).Return(false);
			permissionInformation.Stub(x => x.BusinessUnitAccessCollection()).Return(new List<IBusinessUnit> { null });

			var result = target.AvailableBusinessUnits(person, null);
			Assert.AreEqual(1, result.Count());

		}

		[Test]
		public void VerifyCanLoadHierarchyInformation()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			var businessUnit = MockRepository.GenerateMock<IBusinessUnit>();

			var target = new AvailableBusinessUnitsProvider(repositoryFactory);

			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			repositoryFactory.Stub(x => x.CreateBusinessUnitRepository(unitOfWork)).Return(businessUnitRepository);
			unitOfWork.Stub(x => x.Reassociate(businessUnit));
			businessUnitRepository.Stub(x => x.LoadHierarchyInformation(businessUnit)).Return(businessUnit);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWork.Stub(x => x.Dispose());

			var result = target.LoadHierarchyInformation(dataSource, businessUnit);
			Assert.AreEqual(businessUnit, result);

		}
	}
}