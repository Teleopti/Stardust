using System.Web;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class ChangePersonPassword
	{
		private readonly IApplicationUserTenantQuery _applicationUserTenantQuery;
		private readonly INow _now;
		private readonly IPasswordPolicy _passwordPolicy;
		private readonly ICheckPasswordStrength _checkPasswordStrength;

		public ChangePersonPassword(IApplicationUserTenantQuery applicationUserTenantQuery,
													INow now,
													IPasswordPolicy passwordPolicy,
													ICheckPasswordStrength checkPasswordStrength)
		{
			_applicationUserTenantQuery = applicationUserTenantQuery;
			_now = now;
			_passwordPolicy = passwordPolicy;
			_checkPasswordStrength = checkPasswordStrength;
		}

		public void Modify(string userName, string oldPassword, string newPassword)
		{
			var personInfo = _applicationUserTenantQuery.Find(userName);

			if (personInfo == null || !personInfo.ApplicationLogonInfo.IsValidPassword(_now, _passwordPolicy, oldPassword))
				throw new HttpException(403, "Invalid user name or password.");
			
			if(oldPassword.Equals(newPassword))
				throw new HttpException(400, "No difference between old and new password.");
			
			try
			{
				personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, userName, newPassword);
			}
			catch (PasswordStrengthException)
			{
				throw new HttpException(400, "The new password does not follow the password policy.");
			}
		}
	}
}