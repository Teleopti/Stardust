using System;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationLogonInfo
	{
#pragma warning disable 169
		//remove me when oldschema is gone!
		private Guid id;
#pragma warning restore 169

		private static readonly OneWayEncryption oneWayEncryption = new OneWayEncryption();

		public ApplicationLogonInfo(PersonInfo personInfo)
		{
			PersonInfo = personInfo;
			LastPasswordChange=DateTime.UtcNow;
			InvalidAttemptsSequenceStart = DateTime.UtcNow;
			//todo: tenant - remove me!
			id = Guid.NewGuid();
		}
		protected ApplicationLogonInfo() { }

		public virtual DateTime LastPasswordChange { get; set; }
		public virtual DateTime InvalidAttemptsSequenceStart { get; set; }
		public virtual int InvalidAttempts { get; set; }
		public virtual bool IsLocked { get; protected set; }
		public virtual PersonInfo PersonInfo { get; protected set; }

		public virtual void Lock()
		{
			IsLocked = true;
		}

		public virtual bool IsValidPassword(INow now, IPasswordPolicy passwordPolicy, string unencryptedPassword)
		{
			if (PersonInfo.ApplicationLogonName == null)
				return false;

			var encryptedPassword = oneWayEncryption.EncryptString(unencryptedPassword);

			var utcNow = now.UtcDateTime();
			if (utcNow > InvalidAttemptsSequenceStart.Add(passwordPolicy.InvalidAttemptWindow))
			{
				clearInvalidAttempts(utcNow);
			}

			var isValid = PersonInfo.Password.Equals(encryptedPassword);
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
	}
}