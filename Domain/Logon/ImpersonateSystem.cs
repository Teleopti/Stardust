using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Domain.Logon
{
	public class ImpersonateSystem
	{
		private readonly IDataSourceScope _dataSource;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IPersonRepository _persons;
		private readonly WithUnitOfWork _withUnitOfWork;
		private readonly IBusinessUnitScope _businessUnit;
		private readonly IUpdatedByScope _updatedBy;
		private readonly FullPermissions _fullPermissions;

		[ThreadStatic]
		private static IDisposable _tenantScope;
		[ThreadStatic]
		private static IDisposable _permissionsScope;

		public ImpersonateSystem(
			IDataSourceScope dataSource,
			IBusinessUnitRepository businessUnits,
			IPersonRepository persons,
			WithUnitOfWork withUnitOfWork,
			IBusinessUnitScope businessUnit,
			IUpdatedByScope updatedBy,
			FullPermissions fullPermissions)
		{
			_dataSource = dataSource;
			_businessUnits = businessUnits;
			_persons = persons;
			_withUnitOfWork = withUnitOfWork;
			_businessUnit = businessUnit;
			_updatedBy = updatedBy;
			_fullPermissions = fullPermissions;
		}

		public void Impersonate(string tenant, Guid businessUnitId)
		{
			_tenantScope = _dataSource.OnThisThreadUse(tenant);
			_withUnitOfWork.Do(uow =>
			{
				var businessUnit = _businessUnits.Load(businessUnitId);
				_businessUnit.OnThisThreadUse(businessUnit);

				var person = _persons.Load(SystemUser.Id);
				_updatedBy.OnThisThreadUse(person);

				_permissionsScope = _fullPermissions.Apply();
			});
		}

		public void EndImpersonation()
		{
			_permissionsScope.Dispose();
			_updatedBy.OnThisThreadUse(null);
			_businessUnit.OnThisThreadUse(null);
			_tenantScope.Dispose();
		}

	}
}