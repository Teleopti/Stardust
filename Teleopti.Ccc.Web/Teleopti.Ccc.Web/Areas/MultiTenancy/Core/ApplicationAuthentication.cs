using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class ApplicationAuthentication : IApplicationAuthentication
	{
		private readonly IApplicationUserQuery _applicationUserQuery;
		private readonly Func<IPasswordPolicy> _passwordPolicy;
		private readonly INow _now;
		private readonly IVerifyPasswordPolicy _verifyPasswordPolicy;
		private readonly IDataSourceConfigurationEncryption _dataSourceConfigurationEncryption;
		private readonly IEnumerable<IHashFunction> _hashFunctions;
		private readonly IHashFunction _currentHashFunction;

		public ApplicationAuthentication(IApplicationUserQuery applicationUserQuery,
																	Func<IPasswordPolicy> passwordPolicy, 
																	INow now, 
																	IVerifyPasswordPolicy verifyPasswordPolicy,
																	IDataSourceConfigurationEncryption dataSourceConfigurationEncryption, 
																	IEnumerable<IHashFunction> hashFunctions, 
																	IHashFunction currentHashFunction)
		{
			_applicationUserQuery = applicationUserQuery;
			_passwordPolicy = passwordPolicy;
			_now = now;
			_verifyPasswordPolicy = verifyPasswordPolicy;
			_dataSourceConfigurationEncryption = dataSourceConfigurationEncryption;
			_hashFunctions = hashFunctions;
			_currentHashFunction = currentHashFunction;
		}

		public TenantAuthenticationResult Logon(string userName, string password)
		{
			var personInfo = _applicationUserQuery.Find(userName) ?? generatePersonInfoToHandleTimingAttack();

			var applicationLogonInfo = personInfo.ApplicationLogonInfo;

			var hashFunction = _hashFunctions.FirstOrDefault(x => x.IsGeneratedByThisFunction(applicationLogonInfo.LogonPassword));
			if (hashFunction == null)
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (!applicationLogonInfo.IsValidPassword(_now, _passwordPolicy(), password, hashFunction))
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (applicationLogonInfo.IsLocked)
				return createFailingResult(Resources.LogOnFailedAccountIsLocked);

			var passwordCheck = _verifyPasswordPolicy.Check(applicationLogonInfo);
			if (passwordCheck.HasMessage)
				return new TenantAuthenticationResult
				{
					FailReason = passwordCheck.Message,
					PasswordExpired = passwordCheck.PasswordExpired,
					Success = passwordCheck.Successful,
					Tenant = personInfo.Tenant.Name,
					TenantPassword = personInfo.TenantPassword,
					DataSourceConfiguration = _dataSourceConfigurationEncryption.EncryptConfig(personInfo.Tenant.DataSourceConfiguration),
					PersonId = personInfo.Id,
				};

			if (hashFunction.GetType() != _currentHashFunction.GetType())
				applicationLogonInfo.SetCurrentPasswordWithNewHashFunction(password, _currentHashFunction);
			return new TenantAuthenticationResult
			{
				Success = true,
				PersonId = personInfo.Id,
				Tenant = personInfo.Tenant.Name,
				DataSourceConfiguration = _dataSourceConfigurationEncryption.EncryptConfig(personInfo.Tenant.DataSourceConfiguration),
				ApplicationConfig = personInfo.Tenant.ApplicationConfig,
				TenantPassword = personInfo.TenantPassword
			};
		}

		private static PersonInfo generatePersonInfoToHandleTimingAttack()
		{
			var personInfo = new PersonInfo();
			personInfo.ApplicationLogonInfo.SetEncryptedPasswordIfLogonNameExistButNoPassword(
				"$2a$10$dgiqfELBa7ptsFj6vw1nTOMoVBoVxAvgh6Md.eLPbxyMgdd2tn6uS");
			return personInfo;
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