using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface IAvailableBusinessUnitsProvider
	{
		IEnumerable<IBusinessUnit> AvailableBusinessUnits(IPerson loggedOnPerson, IDataSource dataSource);
		IBusinessUnit LoadHierarchyInformation(IDataSource dataSource, IBusinessUnit businessUnit);
	}

	public class AvailableBusinessUnitsProvider : IAvailableBusinessUnitsProvider
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public AvailableBusinessUnitsProvider(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}
		
		public IEnumerable<IBusinessUnit> AvailableBusinessUnits(IPerson loggedOnPerson, IDataSource dataSource)
		{
			if (loggedOnPerson.PermissionInformation.HasAccessToAllBusinessUnits())
			{
				using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var businessUnitRepository = _repositoryFactory.CreateBusinessUnitRepository(uow);
					return businessUnitRepository.LoadAllBusinessUnitSortedByName();
				}
			}

			return loggedOnPerson.PermissionInformation.BusinessUnitAccessCollection();
		}

		public IBusinessUnit LoadHierarchyInformation(IDataSource dataSource, IBusinessUnit businessUnit)
		{
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var businessUnitRepository = _repositoryFactory.CreateBusinessUnitRepository(uow);
				uow.Reassociate(businessUnit);
				return businessUnitRepository.LoadHierarchyInformation(businessUnit);
			}
		}
	}
}