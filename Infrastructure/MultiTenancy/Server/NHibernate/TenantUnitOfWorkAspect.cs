using System;
using System.Linq;
using System.Text;
using System.Web;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.MultiTenancy;

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
				var methodString = buildMethodString(invocation);
				_tenantUnitOfWork.CancelAndDisposeCurrent();
				throw new HttpException(NoTenantAccessHttpErrorCode, $"Invalid tenant credentials! Calling method: {methodString}");
			}
		}
		private string buildMethodString(IInvocationInfo invocation)
		{
			var sb = new StringBuilder();
			var className = invocation.Method.DeclaringType.FullName;
			var methodName = invocation.Method.Name;

			var parameterNames = invocation.Arguments.Select(act => act.ToString());
			sb.Append($"{className}.{methodName}({string.Join(",", parameterNames)})");

			return sb.ToString();
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