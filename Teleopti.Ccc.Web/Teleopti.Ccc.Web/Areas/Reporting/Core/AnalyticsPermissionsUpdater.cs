using System;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.DistributedLock;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public class AnalyticsPermissionsUpdater : IAnalyticsPermissionsUpdater
	{
		private readonly IAnalyticsPermissionRepository _analyticsPermissionRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsPermissionExecutionRepository _analyticsPermissionExecutionRepository;
		private readonly IPermissionsConverter _permissionsConverter;
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsPermissionsUpdater(IAnalyticsPermissionRepository analyticsPermissionRepository,
			IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,
			IAnalyticsPermissionExecutionRepository analyticsPermissionExecutionRepository, IPermissionsConverter permissionsConverter, IDistributedLockAcquirer distributedLockAcquirer, ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsPermissionRepository = analyticsPermissionRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsPermissionExecutionRepository = analyticsPermissionExecutionRepository;
			_permissionsConverter = permissionsConverter;
			_distributedLockAcquirer = distributedLockAcquirer;
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public void Handle(Guid personId, Guid businessUnitId)
		{
			var businessUnit = _analyticsBusinessUnitRepository.Get(businessUnitId);
			if (businessUnit == null) throw new BusinessUnitMissingInAnalyticsException();

			var distributedLock = getDistributedLock(personId, businessUnit);
			if (distributedLock == null) return;
			try
			{
				var currentPermissions = _permissionsConverter.GetApplicationPermissionsAndConvert(personId, businessUnit.BusinessUnitId).ToList();
				var currentAnalyticsPermissions = _analyticsPermissionRepository.GetPermissionsForPerson(personId, businessUnit.BusinessUnitId);

				var toBeAdded = currentPermissions.Where(p => !currentAnalyticsPermissions.Any(x => x.Equals(p)));
				var toBeDeleted = currentAnalyticsPermissions.Where(p => !currentPermissions.Any(x => x.Equals(p)));
				_analyticsPermissionRepository.InsertPermissions(toBeAdded);
				_analyticsPermissionRepository.DeletePermissions(toBeDeleted);

				_analyticsPermissionExecutionRepository.Set(personId, businessUnit.BusinessUnitId);
			}
			catch (Exception)
			{
				distributedLock.Dispose();
				throw;
			}

			_analyticsUnitOfWork.Current().AfterSuccessfulTx(() =>
			{
				distributedLock.Dispose();
			});
		}

		private IDisposable getDistributedLock(Guid personId, AnalyticBusinessUnit businessUnit)
		{
			for (var i = 0; i < 5; i++)
			{
				try
				{
					var lastUpdate = _analyticsPermissionExecutionRepository.Get(personId, businessUnit.BusinessUnitId);
					if (DateTime.UtcNow - lastUpdate < TimeSpan.FromMinutes(15))
						return null;
					 return _distributedLockAcquirer.LockForGuid(this, personId);
				}
				catch (DistributedLockException)
				{
					if (i == 4)
						throw;
				}
			}
			throw new DistributedLockException("Failed to aquire a distributed lock");
		}
	}
}