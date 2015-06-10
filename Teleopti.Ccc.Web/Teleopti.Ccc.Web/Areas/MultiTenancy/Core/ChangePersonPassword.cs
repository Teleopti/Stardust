using System;
using System.Web;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class ChangePersonPassword : IChangePersonPassword
	{
		private readonly INow _now;
		private readonly IPasswordPolicy _passwordPolicy;
		private readonly IFindPersonInfo _findPersonInfo;
		private readonly ICheckPasswordStrength _checkPasswordStrength;

		public ChangePersonPassword(INow now,
													IPasswordPolicy passwordPolicy,
													IFindPersonInfo findPersonInfo,
													ICheckPasswordStrength checkPasswordStrength)
		{
			_now = now;
			_passwordPolicy = passwordPolicy;
			_findPersonInfo = findPersonInfo;
			_checkPasswordStrength = checkPasswordStrength;
		}

		public void Modify(Guid personId, string oldPassword, string newPassword)
		{
			var personInfo = _findPersonInfo.GetById(personId);

			if (personInfo == null || !personInfo.ApplicationLogonInfo.IsValidPassword(_now, _passwordPolicy, oldPassword))
				throw new HttpException(403, "Invalid user name or password.");
			
			if(oldPassword.Equals(newPassword))
				throw new HttpException(400, "No difference between old and new password.");
			
			try
			{
				personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfo.ApplicationLogonInfo.LogonName, newPassword);
			}
			catch (PasswordStrengthException)
			{
				throw new HttpException(400, "The new password does not follow the password policy.");
			}
		}
	}
}