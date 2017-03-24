using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class ChangePersonPassword : IChangePersonPassword
	{
		private readonly INow _now;
		private readonly IPasswordPolicy _passwordPolicy;
		private readonly IFindPersonInfo _findPersonInfo;
		private readonly ICheckPasswordStrength _checkPasswordStrength;
		private readonly IEnumerable<IHashFunction> _hashFunctions;
		private readonly IHashFunction _currentHashFunction;

		public ChangePersonPassword(INow now,
													IPasswordPolicy passwordPolicy,
													IFindPersonInfo findPersonInfo,
													ICheckPasswordStrength checkPasswordStrength, 
													IEnumerable<IHashFunction> hashFunctions, 
													IHashFunction currentHashFunction)
		{
			_now = now;
			_passwordPolicy = passwordPolicy;
			_findPersonInfo = findPersonInfo;
			_checkPasswordStrength = checkPasswordStrength;
			_hashFunctions = hashFunctions;
			_currentHashFunction = currentHashFunction;
		}

		public void Modify(Guid personId, string oldPassword, string newPassword)
		{
			var personInfo = _findPersonInfo.GetById(personId);
			if (personInfo == null)
				throw new HttpException(403, "Invalid user name or password.");

			var hashFunction = _hashFunctions.FirstOrDefault(x => x.IsGeneratedByThisFunction(personInfo.ApplicationLogonInfo.LogonPassword));
			
			if (hashFunction == null || !personInfo.ApplicationLogonInfo.IsValidPassword(_now, _passwordPolicy, oldPassword, hashFunction))
				throw new HttpException(403, "Invalid user name or password.");
			
			if(oldPassword.Equals(newPassword))
				throw new HttpException(400, "No difference between old and new password.");
			
			try
			{
				personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfo.ApplicationLogonInfo.LogonName, newPassword, _currentHashFunction);
			}
			catch (PasswordStrengthException)
			{
				throw new HttpException(400, "The new password does not follow the password policy.");
			}
		}
	}
}