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

		public virtual bool IsValidPassword(IPasswordPolicy passwordPolicy, string unencryptedPassword)
		{
			var encryptedPassword = oneWayEncryption.EncryptString(unencryptedPassword);
			var isValid = PersonInfo.Password.Equals(encryptedPassword);
			if (isValid)
			{
				ClearInvalidAttempts();
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

		public virtual void ClearInvalidAttempts()
		{
			InvalidAttemptsSequenceStart = DateTime.UtcNow;
			InvalidAttempts = 0;
		}
	}
}