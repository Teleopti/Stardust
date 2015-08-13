﻿using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class ApplicationAuthentication : IApplicationAuthentication
	{
		private readonly IApplicationUserQuery _applicationUserQuery;
		private readonly Func<IPasswordPolicy> _passwordPolicy;
		private readonly INow _now;
		private readonly IVerifyPasswordPolicy _verifyPasswordPolicy;
		private readonly IDataSourceConfigurationEncryption _dataSourceConfigurationEncryption;

		public ApplicationAuthentication(IApplicationUserQuery applicationUserQuery,
																	Func<IPasswordPolicy> passwordPolicy, 
																	INow now, 
																	IVerifyPasswordPolicy verifyPasswordPolicy,
																	IDataSourceConfigurationEncryption dataSourceConfigurationEncryption)
		{
			_applicationUserQuery = applicationUserQuery;
			_passwordPolicy = passwordPolicy;
			_now = now;
			_verifyPasswordPolicy = verifyPasswordPolicy;
			_dataSourceConfigurationEncryption = dataSourceConfigurationEncryption;
		}

		public TenantAuthenticationResult Logon(string userName, string password)
		{
			var personInfo = _applicationUserQuery.Find(userName);
			if (personInfo == null)
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			var applicationLogonInfo = personInfo.ApplicationLogonInfo;

			if(!applicationLogonInfo.IsValidPassword(_now, _passwordPolicy(), password))
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (applicationLogonInfo.IsLocked)
				return createFailingResult(Resources.LogOnFailedAccountIsLocked);

			var nhibConfig = _dataSourceConfigurationEncryption.EncryptConfig(personInfo.Tenant.DataSourceConfiguration);
			//TODO tenant: no need to keep this when #33685 is done
			if (nhibConfig == null)
				return createFailingResult(Resources.NoDatasource);

			var passwordCheck = _verifyPasswordPolicy.Check(applicationLogonInfo);
			if (passwordCheck.HasMessage)
				return new TenantAuthenticationResult
				{
					FailReason = passwordCheck.Message,
					PasswordExpired = passwordCheck.PasswordExpired,
					Success = passwordCheck.Successful,
					Tenant = personInfo.Tenant.Name,
					TenantPassword = personInfo.TenantPassword
				};

			return new TenantAuthenticationResult
			{
				Success = true,
				PersonId = personInfo.Id,
				Tenant = personInfo.Tenant.Name,
				DataSourceConfiguration = nhibConfig,
				TenantPassword = personInfo.TenantPassword
			};
		}

		private static TenantAuthenticationResult createFailingResult(string failReason)
		{
			return new TenantAuthenticationResult
			{
				Success = false,
				FailReason = failReason
			};
		}
	}
}