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
		private readonly IPerson _loggedOnPerson;
		private readonly IDataSource _dataSource;

		public AvailableBusinessUnitsProvider(IPerson loggedOnPerson, IDataSource dataSource)
		{
			_loggedOnPerson = loggedOnPerson;
			_dataSource = dataSource;
		}

		public IEnumerable<IBusinessUnit> AvailableBusinessUnits(IRepositoryFactory repositoryFactory)
		{
			if (_loggedOnPerson.PermissionInformation.HasAccessToAllBusinessUnits())
			{
				using (IUnitOfWork uow = _dataSource.Application.CreateAndOpenUnitOfWork())
				{
					IBusinessUnitRepository businessUnitRepository = repositoryFactory.CreateBusinessUnitRepository(uow);
					return businessUnitRepository.LoadAllBusinessUnitSortedByName();
				}
			}

			return _loggedOnPerson.PermissionInformation.BusinessUnitAccessCollection();
		}

		public IBusinessUnit LoadHierarchyInformation(IBusinessUnit businessUnit, IRepositoryFactory repositoryFactory)
		{
			using (IUnitOfWork uow = _dataSource.Application.CreateAndOpenUnitOfWork())
			{
				IBusinessUnitRepository businessUnitRepository = repositoryFactory.CreateBusinessUnitRepository(uow);
				uow.Reassociate(businessUnit);
				return businessUnitRepository.LoadHierarchyInformation(businessUnit);
			}
		}
	}
}