using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.Logon
{
	public class ImpersonateSystemAspect : IAspect
	{
		private readonly TenantFromArguments _tenant;
		private readonly IDataSourceScope _dataSource;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IPersonRepository _persons;
		private readonly WithUnitOfWork _withUnitOfWork;
		private readonly IBusinessUnitScope _businessUnit;
		private readonly IUpdatedByScope _updatedBy;
		private readonly IAuthorizationScope _authorization;
		private readonly SystemUserAuthorization _systemUserAuthorization;

		[ThreadStatic]
		private static IDisposable _tenantScope;

		public ImpersonateSystemAspect(
			TenantFromArguments tenant,
			IDataSourceScope dataSource,
			IBusinessUnitRepository businessUnits, 
			IPersonRepository persons,
			WithUnitOfWork withUnitOfWork,
			IBusinessUnitScope businessUnit,
			IUpdatedByScope updatedBy,
			IAuthorizationScope authorization,
			IDefinedRaptorApplicationFunctionFactory applicationFunctions)
		{
			_tenant = tenant;
			_dataSource = dataSource;
			_businessUnits = businessUnits;
			_persons = persons;
			_withUnitOfWork = withUnitOfWork;
			_businessUnit = businessUnit;
			_updatedBy = updatedBy;
			_authorization = authorization;
			_systemUserAuthorization = new SystemUserAuthorization(applicationFunctions);
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var tenant = _tenant.Resolve(invocation.Arguments);
			_tenantScope = _dataSource.OnThisThreadUse(tenant);
			_withUnitOfWork.Do(uow =>
			{
				var businessUnitId = invocation.Arguments.Cast<ILogOnContext>().First().LogOnBusinessUnitId;
				var businessUnit = _businessUnits.Load(businessUnitId);
				_businessUnit.OnThisThreadUse(businessUnit);

				var person = _persons.Load(SystemUser.Id);
				_updatedBy.OnThisThreadUse(person);

				_authorization.OnThisThreadUse(_systemUserAuthorization);
			});
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_authorization.OnThisThreadUse(null);
			_updatedBy.OnThisThreadUse(null);
			_businessUnit.OnThisThreadUse(null);
			_tenantScope.Dispose();
		}
	}
}