using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PasswordPolicyForUser
	{
#pragma warning disable 169
		private Guid id;
#pragma warning restore 169

		public PasswordPolicyForUser(PersonInfo personInfo)
		{
			PersonInfo = personInfo;
			LastPasswordChange=DateTime.UtcNow;
			InvalidAttemptsSequenceStart = DateTime.UtcNow;
		}
		protected PasswordPolicyForUser() { }

		public virtual DateTime LastPasswordChange { get; set; }
		public virtual DateTime InvalidAttemptsSequenceStart { get; set; }
		public virtual int InvalidAttempts { get; set; }
		public virtual bool IsLocked { get; protected set; }
		public virtual PersonInfo PersonInfo { get; protected set; }

		public virtual void Lock()
		{
			IsLocked = true;
		}

		public virtual bool IsValidPassword(string encryptedPassword)
		{
			var isValid = PersonInfo.Password.Equals(encryptedPassword);
			if (isValid)
			{
				ClearInvalidAttempts();
			}
			else
			{
				InvalidAttempts++;
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