using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public class AnalyticsPermissionsUpdater : IAnalyticsPermissionsUpdater
	{
		private readonly IAnalyticsPermissionRepository _analyticsPermissionRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsPermissionExecutionRepository _analyticsPermissionExecutionRepository;
		private readonly IPermissionsConverter _permissionsConverter;

		public AnalyticsPermissionsUpdater(IAnalyticsPermissionRepository analyticsPermissionRepository,
			IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,
			IAnalyticsPermissionExecutionRepository analyticsPermissionExecutionRepository, IPermissionsConverter permissionsConverter)
		{
			_analyticsPermissionRepository = analyticsPermissionRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsPermissionExecutionRepository = analyticsPermissionExecutionRepository;
			_permissionsConverter = permissionsConverter;
		}

		public void Handle(Guid personId, Guid businessUnitId)
		{
			var businessUnit = _analyticsBusinessUnitRepository.Get(businessUnitId);
			if (businessUnit == null) throw new BusinessUnitMissingInAnalyticsException();

			var lastUpdate = _analyticsPermissionExecutionRepository.Get(personId, businessUnit.BusinessUnitId);
			var shouldUpdate = DateTime.UtcNow - lastUpdate >= TimeSpan.FromMinutes(15);
			if (!shouldUpdate)
				return;

			var retries = 3;
			while (true)
			{
				try
				{
					update(personId, businessUnit);
					break;
				}
				catch (ConstraintViolationException)
				{
					retries--;
					if (retries == 0)
						throw;
					Thread.Sleep(new Random().Next(0, 1000));
				}
			}
			
		}

		private void update(Guid personId, AnalyticBusinessUnit businessUnit)
		{
			var currentPermissions = new HashSet<AnalyticsPermission>(_permissionsConverter.GetApplicationPermissionsAndConvert(personId, businessUnit.BusinessUnitId));
			var currentAnalyticsPermissions = new HashSet<AnalyticsPermission>(_analyticsPermissionRepository.GetPermissionsForPerson(personId, businessUnit.BusinessUnitId));
			
			var toBeAdded = currentPermissions.Where(p => !currentAnalyticsPermissions.Contains(p)).ToList();
			var toBeDeleted = currentAnalyticsPermissions.Where(p => !currentPermissions.Contains(p)).ToList();
			_analyticsPermissionRepository.InsertPermissions(toBeAdded);
			_analyticsPermissionRepository.DeletePermissions(toBeDeleted);

			_analyticsPermissionExecutionRepository.Set(personId, businessUnit.BusinessUnitId);
		}
	}
}