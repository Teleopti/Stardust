using System;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

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
			var lastUpdate = _analyticsPermissionExecutionRepository.Get(personId);
			if (DateTime.UtcNow - lastUpdate < TimeSpan.FromMinutes(15))
				return;

			var businessUnit = _analyticsBusinessUnitRepository.Get(businessUnitId);
			var currentPermissions = _permissionsConverter.GetApplicationPermissionsAndConvert(personId, businessUnit.BusinessUnitId);
			var currentAnalyticsPermissions = _analyticsPermissionRepository.GetPermissionsForPerson(personId);

			var toBeAdded = currentPermissions.Where(p => !currentAnalyticsPermissions.Any(x => x.Equals(p)));
			var toBeDeleted = currentAnalyticsPermissions.Where(p => !currentPermissions.Any(x => x.Equals(p)));
			_analyticsPermissionRepository.InsertPermissions(toBeAdded);
			_analyticsPermissionRepository.DeletePermissions(toBeDeleted);

			_analyticsPermissionExecutionRepository.Set(personId);
		}
	}
}