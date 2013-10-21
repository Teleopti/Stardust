using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface IAvailableBusinessUnitsProvider
    {
        IEnumerable<IBusinessUnit> AvailableBusinessUnits();
        IBusinessUnit LoadHierarchyInformation(IBusinessUnit businessUnit);
    }

    public class AvailableBusinessUnitsProvider : IAvailableBusinessUnitsProvider
    {
        private readonly IDataSourceContainer _dataSourceContainer;

        public AvailableBusinessUnitsProvider(IDataSourceContainer dataSourceContainer)
        {
            _dataSourceContainer = dataSourceContainer;
        }

        public IEnumerable<IBusinessUnit> AvailableBusinessUnits()
        {
            if (_dataSourceContainer.User.PermissionInformation.HasAccessToAllBusinessUnits())
            {
                using (IUnitOfWork uow = _dataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
                {
                    IBusinessUnitRepository businessUnitRepository = _dataSourceContainer.RepositoryFactory.CreateBusinessUnitRepository(uow);
                    return businessUnitRepository.LoadAllBusinessUnitSortedByName();
                }
            }

            return _dataSourceContainer.User.PermissionInformation.BusinessUnitAccessCollection();
        }

        public IBusinessUnit LoadHierarchyInformation(IBusinessUnit businessUnit)
        {
            using (IUnitOfWork uow = _dataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
            {
                IBusinessUnitRepository businessUnitRepository = _dataSourceContainer.RepositoryFactory.CreateBusinessUnitRepository(uow);
                uow.Reassociate(businessUnit);
                return businessUnitRepository.LoadHierarchyInformation(businessUnit);
            }
        }
    }
}