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
        private MockRepository mocks;
        private IDataSourceContainer dataSourceContainer;
        private AvailableBusinessUnitsProvider target;
        private IPerson person;
        private IPermissionInformation permissionInformation;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            person = mocks.StrictMock<IPerson>();
            permissionInformation = mocks.StrictMock<IPermissionInformation>();
            dataSourceContainer = mocks.StrictMock<IDataSourceContainer>();
            target = new AvailableBusinessUnitsProvider(dataSourceContainer);
        }

        [Test]
        public void VerifyHasAccessToAllBusinessUnits()
        {
            IDataSource dataSource = mocks.StrictMock<IDataSource>();
            IUnitOfWorkFactory unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            IRepositoryFactory repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            IBusinessUnitRepository businessUnitRepository = mocks.StrictMock<IBusinessUnitRepository>();
            using (mocks.Record())
            {
                Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
                Expect.Call(dataSourceContainer.RepositoryFactory).Return(repositoryFactory);
                Expect.Call(dataSourceContainer.User).Return(person).Repeat.AtLeastOnce();
                Expect.Call(person.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(permissionInformation.HasAccessToAllBusinessUnits()).Return(true);
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(repositoryFactory.CreateBusinessUnitRepository(unitOfWork)).Return(businessUnitRepository);
                Expect.Call(businessUnitRepository.LoadAllBusinessUnitSortedByName()).Return(new List<IBusinessUnit>
                                                                                                 {null});
                Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
                Expect.Call(unitOfWork.Dispose);
            }
            using (mocks.Playback())
            {
                var result = target.AvailableBusinessUnits();
                Assert.AreEqual(1,result.Count());
            }
        }

        [Test]
        public void VerifyHasAccessToOneBusinessUnit()
        {
            using (mocks.Record())
            {
                Expect.Call(dataSourceContainer.User).Return(person).Repeat.AtLeastOnce();
                Expect.Call(person.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(permissionInformation.HasAccessToAllBusinessUnits()).Return(false);
                Expect.Call(permissionInformation.BusinessUnitAccessCollection()).Return(new List<IBusinessUnit> {null});
            }
            using (mocks.Playback())
            {
                var result = target.AvailableBusinessUnits();
                Assert.AreEqual(1, result.Count());
            }
        }

        [Test]
        public void VerifyCanLoadHierarchyInformation()
        {
            IDataSource dataSource = mocks.StrictMock<IDataSource>();
            IUnitOfWorkFactory unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            IRepositoryFactory repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            IBusinessUnitRepository businessUnitRepository = mocks.StrictMock<IBusinessUnitRepository>();
            IBusinessUnit businessUnit = mocks.StrictMock<IBusinessUnit>();
            using (mocks.Record())
            {
                Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
                Expect.Call(dataSourceContainer.RepositoryFactory).Return(repositoryFactory);
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(repositoryFactory.CreateBusinessUnitRepository(unitOfWork)).Return(businessUnitRepository);
                Expect.Call(() => unitOfWork.Reassociate(businessUnit));
                Expect.Call(businessUnitRepository.LoadHierarchyInformation(businessUnit)).Return(businessUnit);
                Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
                Expect.Call(unitOfWork.Dispose);
            }
            using (mocks.Playback())
            {
                var result = target.LoadHierarchyInformation(businessUnit);
                Assert.AreEqual(businessUnit, result);
            }
        }
    }
}