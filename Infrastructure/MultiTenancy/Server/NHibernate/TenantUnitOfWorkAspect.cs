using System;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkAspect : ITenantUnitOfWorkAspect
	{
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ITenantAuthentication _tenantAuthentication;

		public static int NoTenantAccessHttpErrorCode = 403;

		public TenantUnitOfWorkAspect(ITenantUnitOfWork tenantUnitOfWork, ITenantAuthentication tenantAuthentication)
		{
			_tenantUnitOfWork = tenantUnitOfWork;
			_tenantAuthentication = tenantAuthentication;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_tenantUnitOfWork.EnsureUnitOfWorkIsStarted();

			var hasNoTenantAuthenticationAttribute =
				invocation.Method.GetCustomAttributes(typeof (NoTenantAuthenticationAttribute), false).Any();

			if (hasNoTenantAuthenticationAttribute)
				return;

			if (!_tenantAuthentication.Logon())
			{
				//include logging here?
				_tenantUnitOfWork.CancelAndDisposeCurrent();
				throw new HttpException(NoTenantAccessHttpErrorCode, "Invalid tenant credentials!");
			}
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			if (exception == null)
			{
				_tenantUnitOfWork.CommitAndDisposeCurrent();
			}
			else
			{
				_tenantUnitOfWork.CancelAndDisposeCurrent();
			}
		}
	}
}