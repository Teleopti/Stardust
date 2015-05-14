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
		private IDataSourceContainer dataSourceContainer;
		private AvailableBusinessUnitsProvider target;
		private IPerson person;
		private IPermissionInformation permissionInformation;

		[SetUp]
		public void Setup()
		{
			person = MockRepository.GenerateMock<IPerson>();
			permissionInformation = MockRepository.GenerateMock<IPermissionInformation>();
			dataSourceContainer = MockRepository.GenerateMock<IDataSourceContainer>();
			target = new AvailableBusinessUnitsProvider(dataSourceContainer);
		}

		[Test]
		public void VerifyHasAccessToAllBusinessUnits()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();

			dataSourceContainer.Stub(x => x.DataSource).Return(dataSource);
			dataSourceContainer.Stub(x => x.User).Return(person).Repeat.AtLeastOnce();
			person.Stub(x => x.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
			permissionInformation.Stub(x => x.HasAccessToAllBusinessUnits()).Return(true);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			repositoryFactory.Stub(x => x.CreateBusinessUnitRepository(unitOfWork)).Return(businessUnitRepository);
			businessUnitRepository.Stub(x => x.LoadAllBusinessUnitSortedByName()).Return(new List<IBusinessUnit> { null });
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWork.Stub(x => x.Dispose());

			var result = target.AvailableBusinessUnits(repositoryFactory);
			Assert.AreEqual(1, result.Count());

		}

		[Test]
		public void VerifyHasAccessToOneBusinessUnit()
		{
			dataSourceContainer.Stub(x => x.User).Return(person).Repeat.AtLeastOnce();
			person.Stub(x => x.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
			permissionInformation.Stub(x => x.HasAccessToAllBusinessUnits()).Return(false);
			permissionInformation.Stub(x => x.BusinessUnitAccessCollection()).Return(new List<IBusinessUnit> { null });

			var result = target.AvailableBusinessUnits(null);
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

			dataSourceContainer.Stub(x => x.DataSource).Return(dataSource);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			repositoryFactory.Stub(x => x.CreateBusinessUnitRepository(unitOfWork)).Return(businessUnitRepository);
			unitOfWork.Stub(x => x.Reassociate(businessUnit));
			businessUnitRepository.Stub(x => x.LoadHierarchyInformation(businessUnit)).Return(businessUnit);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWork.Stub(x => x.Dispose());

			var result = target.LoadHierarchyInformation(businessUnit, repositoryFactory);
			Assert.AreEqual(businessUnit, result);

		}
	}
}