using System;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationLogonInfo
	{
		private static readonly IPasswordHashFunction passwordHashFunction = new OneWayEncryption();

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

		protected internal virtual void SetApplicationLogonCredentialsInternal(ICheckPasswordStrength checkPasswordStrength, string logonName, string password)
		{
			if(!string.IsNullOrEmpty(password))
				setPassword(checkPasswordStrength, password);
			LogonName = logonName;
			registerPasswordChange();
		}

		private void setPassword(ICheckPasswordStrength checkPasswordStrength, string newPassword)
		{
			checkPasswordStrength.Validate(newPassword);
			//todo: tenant get rid of domain dependency here
			LogonPassword = passwordHashFunction.CreateHash(newPassword);
		}

		public virtual void Lock()
		{
			IsLocked = true;
		}

		public virtual bool IsValidPassword(INow now, IPasswordPolicy passwordPolicy, string unencryptedPassword)
		{
			if (LogonName == null)
				return false;

			var encryptedPassword = passwordHashFunction.CreateHash(unencryptedPassword);

			var utcNow = now.UtcDateTime();
			if (utcNow > InvalidAttemptsSequenceStart.Add(passwordPolicy.InvalidAttemptWindow))
			{
				clearInvalidAttempts(utcNow);
			}

			var isValid = LogonPassword.Equals(encryptedPassword);
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