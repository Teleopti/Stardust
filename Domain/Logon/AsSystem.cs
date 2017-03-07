using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Domain.Logon
{
	public class AsSystem
	{
		private readonly ILogOnOff _logOnOff;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IDataSourceForTenant _dataSourceForTenant;

		public AsSystem(
			ILogOnOff logOnOff,
			IRepositoryFactory repositoryFactory,
			IDataSourceForTenant dataSourceForTenant
			)
		{
			_logOnOff = logOnOff;
			_repositoryFactory = repositoryFactory;
			_dataSourceForTenant = dataSourceForTenant;
		}

		public void Logon(string tenant, Guid businessUnitId)
		{
			var dataSource = _dataSourceForTenant.Tenant(tenant);
			using (var unitOfWork = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var systemUser = _repositoryFactory.CreatePersonRepository(unitOfWork).LoadPersonAndPermissions(SystemUser.Id);
				var businessUnit = _repositoryFactory.CreateBusinessUnitRepository(unitOfWork).Get(businessUnitId);
				_logOnOff.LogOn(dataSource, systemUser, businessUnit);
			}
		}
		
	}
}