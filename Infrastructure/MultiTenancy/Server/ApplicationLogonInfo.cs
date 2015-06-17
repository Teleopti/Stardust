using System;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationLogonInfo
	{
		private static readonly OneWayEncryption oneWayEncryption = new OneWayEncryption();

		public ApplicationLogonInfo()
		{
			LastPasswordChange=DateTime.UtcNow;
			InvalidAttemptsSequenceStart = DateTime.UtcNow;
		}

		public virtual DateTime LastPasswordChange { get; protected set; }
		public virtual int InvalidAttempts { get; protected set; }
		public virtual bool IsLocked { get; protected set; }
		public virtual string LogonName { get; protected set; }

		public virtual string LogonPassword { get; set; }
		protected virtual DateTime InvalidAttemptsSequenceStart { get; set; }
		
		private void registerPasswordChange()
		{
			LastPasswordChange = DateTime.UtcNow;
			InvalidAttempts = 0;
			IsLocked = false;
		}

		protected internal virtual void SetApplicationLogonCredentialsInternal(ICheckPasswordStrength checkPasswordStrength, string logonName, string password)
		{
			setPassword(checkPasswordStrength, password);
			LogonName = logonName;
			registerPasswordChange();
		}

		private void setPassword(ICheckPasswordStrength checkPasswordStrength, string newPassword)
		{
			checkPasswordStrength.Validate(newPassword);
			//todo: tenant get rid of domain dependency here
			LogonPassword = new OneWayEncryption().EncryptString(newPassword);
		}

		public virtual void Lock()
		{
			IsLocked = true;
		}

		public virtual bool IsValidPassword(INow now, IPasswordPolicy passwordPolicy, string unencryptedPassword)
		{
			if (LogonName == null)
				return false;

			var encryptedPassword = oneWayEncryption.EncryptString(unencryptedPassword);

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
	}
}