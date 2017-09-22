using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Security;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationLogonInfo
	{
		public ApplicationLogonInfo()
		{
			LastPasswordChange=DateTime.UtcNow;
			InvalidAttemptsSequenceStart = DateTime.UtcNow;
		}

		public virtual DateTime LastPasswordChange { get; protected set; }
		public virtual int InvalidAttempts { get; protected set; }
		public virtual bool IsLocked { get; protected set; }
		public virtual string LogonName { get; protected set; }

		public virtual string LogonPassword { get; protected set; }
		protected virtual DateTime InvalidAttemptsSequenceStart { get; set; }
		
		private void registerPasswordChange()
		{
			LastPasswordChange = DateTime.UtcNow;
			InvalidAttempts = 0;
			IsLocked = false;
		}

		protected internal virtual void SetApplicationLogonCredentialsInternal(ICheckPasswordStrength checkPasswordStrength, string logonName, string password, IHashFunction currentHashFunction)
		{
			if(!string.IsNullOrEmpty(password))
				setPassword(checkPasswordStrength, password, currentHashFunction);
			LogonName = logonName;
			registerPasswordChange();
		}

		public virtual void SetCurrentPasswordWithNewHashFunction(string password, IHashFunction currentHashFunction)
		{
			LogonPassword = currentHashFunction.CreateHash(password);
		}

		private void setPassword(ICheckPasswordStrength checkPasswordStrength, string newPassword, IHashFunction currentHashFunction)
		{
			checkPasswordStrength.Validate(newPassword);
			//todo: tenant get rid of domain dependency here
			LogonPassword = currentHashFunction.CreateHash(newPassword);
		}

		public virtual void Lock()
		{
			IsLocked = true;
		}

		public virtual bool IsValidPassword(INow now, IPasswordPolicy passwordPolicy, string unencryptedPassword, IHashFunction hashFunction)
		{
			if (LogonName == null)
				return false;

			var utcNow = now.UtcDateTime();
			if (utcNow > InvalidAttemptsSequenceStart.Add(passwordPolicy.InvalidAttemptWindow))
			{
				clearInvalidAttempts(utcNow);
			}
			var isValid = hashFunction.Verify(unencryptedPassword, LogonPassword);
			if (isValid)
			{
				clearInvalidAttempts(utcNow);
			}
			else
			{
				InvalidAttempts++;
				if (InvalidAttempts > passwordPolicy.MaxAttemptCount)
				{
					Lock();
				}
			}
			return isValid;
		}

		private void clearInvalidAttempts(DateTime dateTimeNow)
		{
			InvalidAttemptsSequenceStart = dateTimeNow;
			InvalidAttempts = 0;
		}

		public void SetLastPasswordChange_OnlyUseFromTests(DateTime lastPasswordChange)
		{
			LastPasswordChange = lastPasswordChange;
		}

		//used when importing tenant, we don't want to encrypt what is encrypted already
		// and when updating because we don't get password then
		public void SetEncryptedPasswordIfLogonNameExistButNoPassword(string encryptedPassword)
		{
			if (hasLogonNameButNoPassword())
				LogonPassword = encryptedPassword;
		}

		private bool hasLogonNameButNoPassword()
		{
			return !string.IsNullOrEmpty(LogonName) && string.IsNullOrEmpty(LogonPassword);
		}

	}
}