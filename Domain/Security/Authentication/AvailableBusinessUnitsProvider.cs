using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface IAvailableBusinessUnitsProvider
	{
		IEnumerable<IBusinessUnit> AvailableBusinessUnits(IRepositoryFactory repositoryFactory);
		IBusinessUnit LoadHierarchyInformation(IBusinessUnit businessUnit, IRepositoryFactory repositoryFactory);
	}

	public class AvailableBusinessUnitsProvider : IAvailableBusinessUnitsProvider
	{
		private readonly IDataSourceContainer _dataSourceContainer;

		public AvailableBusinessUnitsProvider(IDataSourceContainer dataSourceContainer)
		{
			_dataSourceContainer = dataSourceContainer;
		}

		public IEnumerable<IBusinessUnit> AvailableBusinessUnits(IRepositoryFactory repositoryFactory)
		{
			if (_dataSourceContainer.User.PermissionInformation.HasAccessToAllBusinessUnits())
			{
				using (IUnitOfWork uow = _dataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
				{
					IBusinessUnitRepository businessUnitRepository = repositoryFactory.CreateBusinessUnitRepository(uow);
					return businessUnitRepository.LoadAllBusinessUnitSortedByName();
				}
			}

			return _dataSourceContainer.User.PermissionInformation.BusinessUnitAccessCollection();
		}

		public IBusinessUnit LoadHierarchyInformation(IBusinessUnit businessUnit, IRepositoryFactory repositoryFactory)
		{
			using (IUnitOfWork uow = _dataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				IBusinessUnitRepository businessUnitRepository = repositoryFactory.CreateBusinessUnitRepository(uow);
				uow.Reassociate(businessUnit);
				return businessUnitRepository.LoadHierarchyInformation(businessUnit);
			}
		}
	}
}