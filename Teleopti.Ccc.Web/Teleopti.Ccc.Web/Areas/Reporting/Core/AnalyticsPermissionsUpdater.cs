using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Util;

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
			IAnalyticsPermissionExecutionRepository analyticsPermissionExecutionRepository, 
			IPermissionsConverter permissionsConverter)
		{
			_analyticsPermissionRepository = analyticsPermissionRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsPermissionExecutionRepository = analyticsPermissionExecutionRepository;
			_permissionsConverter = permissionsConverter;
		}

		public void Handle(Guid personId, Guid businessUnitId)
		{
			runWithRetries(() =>
			{
				UpdatePermissions(personId, businessUnitId);
			});
		}

		[UnitOfWork]
		[AnalyticsUnitOfWork]
		protected virtual void UpdatePermissions(Guid personId, Guid businessUnitId)
		{
			var businessUnit = _analyticsBusinessUnitRepository.Get(businessUnitId);
			if (businessUnit == null) throw new BusinessUnitMissingInAnalyticsException();

			var lastUpdate = _analyticsPermissionExecutionRepository.Get(personId, businessUnit.BusinessUnitId);
			var shouldUpdate = DateTime.UtcNow - lastUpdate >= TimeSpan.FromMinutes(15);
			if (!shouldUpdate)
				return;
			_analyticsPermissionExecutionRepository.Set(personId, businessUnit.BusinessUnitId);

			var currentPermissions = new HashSet<AnalyticsPermission>(_permissionsConverter.GetApplicationPermissionsAndConvert(personId, businessUnit.BusinessUnitId));
			var currentAnalyticsPermissions = new HashSet<AnalyticsPermission>(_analyticsPermissionRepository.GetPermissionsForPerson(personId, businessUnit.BusinessUnitId));
			var toBeAdded = currentPermissions.Where(p => !currentAnalyticsPermissions.Contains(p)).ToList();
			var toBeDeleted = currentAnalyticsPermissions.Where(p => !currentPermissions.Contains(p)).ToList();
			_analyticsPermissionRepository.InsertPermissions(toBeAdded);
			_analyticsPermissionRepository.DeletePermissions(toBeDeleted);
		}

		private static void runWithRetries(Action action)
		{
			Retry.Handle<ConstraintViolationException>()
				.WaitAndRetry(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(400))
				.Do(action);
		}
	}
}