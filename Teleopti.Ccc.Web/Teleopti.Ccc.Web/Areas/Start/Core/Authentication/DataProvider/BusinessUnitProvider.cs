using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public class BusinessUnitProvider : IBusinessUnitProvider
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public BusinessUnitProvider(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public IEnumerable<IBusinessUnit> RetrieveBusinessUnitsForPerson(IDataSource dataSource, IPerson person)
		{
			if (person.PermissionInformation.HasAccessToAllBusinessUnits())
			{
				using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var rep = _repositoryFactory.CreateBusinessUnitRepository(uow);
					return rep.LoadAllBusinessUnitSortedByName();
				}
			}
			return person.PermissionInformation.BusinessUnitAccessCollection();
		}
	}
}