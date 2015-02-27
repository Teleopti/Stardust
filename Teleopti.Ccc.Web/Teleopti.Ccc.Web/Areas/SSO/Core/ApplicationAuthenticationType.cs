using System;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.SSO.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public interface IApplicationAuthenticationType
	{
		ApplicationAuthenticationModel BindModel(ModelBindingContext bindingContext);
	}

	public class ApplicationAuthenticationType : IApplicationAuthenticationType
	{
		private readonly Lazy<ISsoAuthenticator> _authenticator;
		private readonly Lazy<ILogLogonAttempt> _logLogonAttempt;

		public ApplicationAuthenticationType(Lazy<ISsoAuthenticator> authenticator, Lazy<ILogLogonAttempt> logLogonAttempt)
		{
			_authenticator = authenticator;
			_logLogonAttempt = logLogonAttempt;
		}

		public ApplicationAuthenticationModel BindModel(ModelBindingContext bindingContext)
		{
			return new ApplicationAuthenticationModel(_authenticator.Value, _logLogonAttempt.Value)
				{
					UserName = bindingContext.ValueProvider.GetValue("username").AttemptedValue,
					Password = bindingContext.ValueProvider.GetValue("password").AttemptedValue
				};
		}
	}
}